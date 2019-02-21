using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Recolor_Guy
{
    public class ImageProcessor:Object
    {
        private delegate bool IsHueGroup(int color, Spectrum target);
        private delegate bool IsPixel(int pixel, Spectrum target);

        public event EventHandler WriteSyncCompleted;

        const int _PIXEL_CONCURRENCY = 6000;
        private WriteableBitmap _bitmap;
        private List<Task> _tasks;
        private Memory<int>[] _memCopy;
        private int[][] _bufferSegments;
        private readonly bool _isInitialized;
        private readonly Thread _mainThread;
        private long _ImageLength;
        private int _Width;
        private int _BufferSize;
        private IsPixel _IsTargeted;
        private IsHueGroup _IsNewHue;

        public ImageProcessor() { this._isInitialized = false; WriteSyncCompleted += WrapUp; }

        public ImageProcessor(Uri imageUri)
        {
            BitmapSource bs = new BitmapImage(imageUri);
            this._bitmap = new WriteableBitmap(bs);
            
            _mainThread = Thread.CurrentThread;
        }

        public ImageProcessor(WriteableBitmap bitmap)
        {
            this._bitmap = bitmap;
            this._isInitialized = true;

            _mainThread = Thread.CurrentThread;
        }

        ~ImageProcessor()
        {
            if(Thread.CurrentThread == _mainThread)
                if(_bitmap != null && 
                   _bitmap.IsFrozen)
                {
                    _bitmap.Unlock();
                }
        }

        public void RequestChangeColorGroup(Spectrum colorFrom, Spectrum colorTo, Dispatcher mainDispatcher)
        {
            if (_isInitialized)
            {
                _memCopy = GetImageSpan();

                int taskCount = GetTaskCount(_memCopy.Length);
                int writableWidth = _bitmap.PixelWidth;
                int bufferWidth = _bitmap.BackBufferStride/4;
                WriteSyncCompleted += WrapUp;

                _IsTargeted = GetTargetSelector(colorFrom);
                _IsNewHue = GetHueSelector(colorTo);

                Task.Run(() =>
                {
                    _tasks = new List<Task>();
                    for (int i = 0; i < taskCount; i++)
                    {
                        for (int m = 0; m < _memCopy.Length; m++)
                        {
                            int startPixel = i * bufferWidth;
                            if (startPixel <= _memCopy[m].Length)
                            {
                                Task t = new Task((a) =>
                                    {
                                        Debug.Print($"Get task {a.ToString()} span started.");
                                        int n = (int)a;
                                        if (startPixel + _PIXEL_CONCURRENCY < _memCopy[n].Length)
                                        {
                                            ProcessBytes(_memCopy[n].Span.Slice(startPixel, writableWidth), colorFrom, colorTo);
                                        }
                                        else
                                        {
                                            ProcessBytes(_memCopy[n].Span.Slice(startPixel, _memCopy[n].Length - startPixel), colorFrom, colorTo);
                                        }
                                        Debug.Print($"Get task {a.ToString()} span passed.");
                                    },
                                    m);

                                _tasks.Add(t);
                            }
                        }
                    }

                    foreach (Task t in _tasks)
                    {
                        t.Start();
                    }
                    Task.WaitAll(_tasks.ToArray());
                    
                    try
                    {
                        mainDispatcher.Invoke(() =>
                            { OnWriteSyncCompleted(null); }
                            );
                    }
                    catch { }
                }
                );
            }
        }

        private int[][] GetSegments()
        {
            ///<summary>
            ///Get pixel segments for this image. Maximum image size using this manner is 2147483647^2 pixels.
            ///</summary>
            _ImageLength = _bitmap.PixelWidth * _bitmap.PixelHeight;
            return _ImageLength < Int32.MaxValue ? new int[1][] :
                                                   new int[(_ImageLength % Int32.MaxValue) + 1][];
        }

        private Memory<int>[] GetImageSpan()
        {
            _bufferSegments = GetSegments(); 

            _Width = _bitmap.PixelWidth;

            _BufferSize = _bitmap.BackBufferStride - (_Width*4);
            //Make a heap copy of the BackBuffer
            unsafe
            {
                try
                { 
                    _bitmap.Lock();

                    int intPtr = (int)_bitmap.BackBuffer;

                    for (int a = 0; a < _bufferSegments.Length ; a++)
                    {
                        _bufferSegments[a] = _ImageLength % Int32.MaxValue < a ? new int[Int32.MaxValue] :
                                                                                 new int[_ImageLength - ((long)Int32.MaxValue * (a))];
                           
                        for (int i = 0; i < _bufferSegments[a].Length; i++)
                        {
                            _bufferSegments[a][i] = *((int*)intPtr);
                            intPtr += 4;
                            if (i % _Width == 0) { intPtr += _BufferSize; }
                        }
                    }
                    Debug.Print("Get Image span passed.");
                }
                catch
                {
                    ;
                }
                finally { _bitmap.Unlock(); }
            }

            Memory<int>[] memCopy = new Memory<int>[_bufferSegments.Length];
            for (int i = 0; i < _bufferSegments.Length; i++)
            {
                memCopy[i] = new Memory<int>(_bufferSegments[i]);
            }

            return memCopy;
        }

        private int GetTaskCount(int processUnits)
        {
            return processUnits % _PIXEL_CONCURRENCY;
        }

        private void WrapUp(object sender, EventArgs e)
        {
            System.Windows.Int32Rect dirtyRect = new System.Windows.Int32Rect(0, 0, _Width, _bitmap.PixelHeight);
            try
            {
                unsafe
                {
                    _bitmap.Lock();
                    int intPtr = (int)_bitmap.BackBuffer;
                    for (int m = 0; m < _bufferSegments.Length; m++)
                    {
                        for (int i = 0; i < _bufferSegments[m].Length; i++)
                        {
                            *((int*)intPtr) = _bufferSegments[m][i];
                            intPtr += 4;
                            if (i % _Width == 0) { intPtr += _BufferSize; }
                        }
                    }
                    Debug.Print("Wrap up passed.");
                }
                _bitmap.AddDirtyRect(dirtyRect);
            }
            catch
            {
                ;
            }
            finally { _bitmap.Unlock(); }
        }

        private void ProcessBytes(Span<int> dataSlice, Spectrum targetHue, Spectrum alterTo)
        {
            for(int i = 0; i < dataSlice.Length; i++)
            {
                //If this hue meets the correct profile, then change it to the requested hue.
                int pxl = dataSlice[i];
                if (_IsTargeted(pxl,targetHue))
                {
                    dataSlice[i] = RecolorPixel.Recolor(pxl, alterTo);
                }
            }
        }

        private static IsHueGroup GetHueSelector(Spectrum requiredHue)
        {
            return (hu,re) => 
            {
                int reNum = (int)re;

                if(hu > reNum && hu < reNum+30)
                    {return true;}
                else
                    {return false;}
            };
        }

        private static IsPixel GetTargetSelector(Spectrum targetedHue)
        {
            if((int)targetedHue >= -15 && (int)targetedHue <= 360)
            {
                return (pxl, th) => 
                {
                    float b = Color.FromArgb(pxl).GetBrightness();
                    int h = RecolorPixel.GetHue(pxl);
                    h = h > 345 ? h -= 360 : h;       

                    int bth = (int)th;
                    int tth = (bth + 30) > 360 ? bth -= 330 : bth + 30;

                    return (h >= bth && 
                            h <= tth &&
                            b < 0.95 &&
                            b > 0.05);
                };
            }
            else
            {
                switch (targetedHue)
                {
                    //TODO: Assign bools for special cases
                    case Spectrum.Null:
                        return (h, th) => { return false; };
                    case Spectrum.White:
                        return (h, th) => { return false; };
                    case Spectrum.Black:
                        return (h, th) => { return false; };
                    default:
                        return (h, th) => { return false; };
                }
            }
        }

        protected virtual void OnWriteSyncCompleted(EventArgs e)
        {
            EventHandler handler = this.WriteSyncCompleted;
            if(handler != null)
            {
                handler(this, e);
            }
        }
    }
}
