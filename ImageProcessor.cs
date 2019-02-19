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
    public class ImageProcessor
    {
        private delegate bool IsHueGroup(int color, Spectrum target);
        private delegate bool IsPixel(int pixel, Spectrum target);

        public event EventHandler WriteSyncCompleted;

        const int _PIXEL_CONCURRENCY = 6000;
        private WriteableBitmap _bitmap;
        private List<Task> tasks;
        private Memory<int> memCopy;
        private readonly bool _isInitialized;
        private int[] _bufferCopy;
        private readonly Thread _mainThread;
        private long _ImageLength;
        private IsPixel IsTargeted;
        private IsHueGroup IsNewHue;

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
                memCopy = GetImageSpan();

                int taskCount = GetTaskCount(memCopy.Length);
                int writableWidth = _bitmap.PixelWidth;
                int bufferWidth = _bitmap.BackBufferStride/4;
                WriteSyncCompleted += WrapUp;

                IsTargeted = GetTargetSelector(colorFrom);
                IsNewHue = GetHueSelector(colorTo);

                Task.Run(() =>
                {
                    tasks = new List<Task>();
                    for (int i = 0; i < taskCount; i++)
                    {
                        int startPixel = i * bufferWidth;
                        if (startPixel <= memCopy.Length)
                        {
                            Task t = new Task((a) =>
                                {
                                    if (startPixel + _PIXEL_CONCURRENCY < memCopy.Length)
                                    {
                                        ProcessBytes(memCopy.Span.Slice(startPixel, writableWidth), colorFrom, colorTo);
                                    }
                                    else
                                    {
                                        ProcessBytes(memCopy.Span.Slice(startPixel, memCopy.Length - startPixel), colorFrom, colorTo);
                                    }
                                },
                                i);

                            tasks.Add(t);
                        }
                    }

                    foreach (Task t in tasks)
                    {
                        t.Start();
                    }
                    Task.WaitAll(tasks.ToArray());
                    
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

        private Memory<int> GetImageSpan()
        {   
            _ImageLength = (int)((2 + _bitmap.BackBufferStride) * (_bitmap.PixelHeight));

            //Make a heap copy of the BackBuffer
            unsafe
            {
                try
                {
                    _bitmap.Lock();

                    int intPtr = (int)_bitmap.BackBuffer;

                    _bufferCopy = new int[_ImageLength];
                    for (long i = 0; i < _ImageLength; i++)
                    {
                        _bufferCopy[i] = *((int*)intPtr);
                        intPtr++;
                    }
                }
                catch
                {
                    
                }
                finally
                {
                    _bitmap.Unlock();
                }
            }

            return new Memory<int>(_bufferCopy);
        }

        private int GetTaskCount(int processUnits)
        {
            return processUnits % _PIXEL_CONCURRENCY;
        }

        private void WrapUp(object sender, EventArgs e)
        {
            System.Windows.Int32Rect dirtyRect = new System.Windows.Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight);
            try
            {
                unsafe
                {
                    _bitmap.Lock();
                    int intPtr = (int)_bitmap.BackBuffer;
                    for (int i = 0; i < _ImageLength; i++)
                    {
                        *((int*)intPtr) = _bufferCopy[i];
                        intPtr++;
                    }
                }
                _bitmap.AddDirtyRect(dirtyRect);
            }
            catch { }
            finally { _bitmap.Unlock(); }
        }

        private void ProcessBytes(Span<int> dataSlice, Spectrum targetHue, Spectrum alterTo)
        {
            for(int i = 0; i < dataSlice.Length; i++)
            {
                //If this hue meets the correct profile, then change it to the requested hue.
                int pxl = dataSlice[i];
                if (IsTargeted(pxl,targetHue))
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
                    int bth = th;
                    int tth = (bth + 30) > 360 ? bth -= 330 : bth + 30;
                    return (h >= bth && 
                            h <= tth &&
                            b < 0.85 &&
                            b > 0.15);
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
