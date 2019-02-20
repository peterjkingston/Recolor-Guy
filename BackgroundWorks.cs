using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace Recolor_Guy
{
    public class BackgroundWorks:Object
    {
        public event EventHandler ImageProcessed;

        private MainWindow _Owner { get; set; }
        private WriteableBitmap _wBitmap { get; set; }

        public BackgroundWorks(MainWindow owner)
        {
            _Owner = owner;
        }

        private static Bitmap LoadRecolor(string Directory)
        {
            Bitmap result;

            result = new Bitmap(Directory);

            return result;
        }

        private static string FromFile()
        {
            string result = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Picture Files|*.bmp;*.gif;*.jpg;*.png|Bitmap files (*.bmp)|*.bmp|GIF Files(*.gif)|*.gif|JPEG Files(*.jpg)|*.jpg|PNG Files (*.png)|*.png";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return result;
        }
        
        private static ImageSource ToImageSource(Bitmap NewImage)
        {
            ImageSource imageSource = (ImageSource)WPFBitmapConverter.Convert(NewImage);

            return imageSource;
        }

        public void UpdatePreviewImage(Spectrum fromColor, Spectrum toColor)
        {
            /// <summary>
            /// Paints the preview image in the selected color.
            /// </summary>

            if (_Owner.IsOpen)
            {
                _Owner.Image2.ImageSource = _Owner.Image1.ImageSource;
                BitmapSource bSource = _Owner.Image2.ImageSource as BitmapSource;
                _wBitmap = new WriteableBitmap(bSource);
                GC.Collect();

                ((ImageBrush)_Owner.Image2Border.Background).ImageSource = _wBitmap;

                ImageProcessor ip = new ImageProcessor(_wBitmap);
                ip.WriteSyncCompleted += PassUp;

                _Owner.Dispatcher.Invoke(() =>
                {
                    ip.RequestChangeColorGroup(fromColor, toColor, _Owner.Dispatcher);
                });
            }
        }
        
        public void SaveAs()
        {
            /// <summary>
            /// Saves the image in the original image pane to file.
            /// </summary>

            MemoryStream ms = GetImageStream();

            Bitmap left = new Bitmap(ms);
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Picture Files|*.bmp;*.gif;*.jpg;*.png|Bitmap files (*.bmp)|*.bmp|GIF Files(*.gif)|*.gif|JPEG Files(*.jpg)|*.jpg|PNG Files (*.png)|*.png";
            saveFile.FilterIndex = 0;
            saveFile.DefaultExt = ".png";

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (saveFile.FileName != "")
                {
                    left.Save(saveFile.FileName);
                }
            }
        }

        public void ApplyImage()
        {
            /// <summary>
            /// Paints the image preview over the original image pane.
            /// </summary>

            ((ImageBrush)_Owner.Image1Border.Background).ImageSource = null;
            _Owner.Image1Border.Background = _Owner.Image2.Clone();
            _Owner.Image1 = _Owner.Image2.Clone();
        }

        public void OpenImage()
        {
            /// <summary>
            /// Paints selected image file to the window panes.
            /// </summary>

            string directory = FromFile();

            if (directory != "")
            {
                Bitmap left = LoadRecolor(directory);

                ImageBrush iBrush = new ImageBrush(ToImageSource(left));

                _Owner.Image1Border.Background = _Owner.Image1 = iBrush;
                _Owner.Image2Border.Background = _Owner.Image2 = iBrush.Clone() ;

                _Owner.FromBox.IsEnabled = true;
                _Owner.ToBox.IsEnabled = true;

                _Owner.IsOpen = true;
            }
        }

        private MemoryStream GetImageStream()
        {
            BitmapImage bmImage = new BitmapImage();
            MemoryStream stream = new MemoryStream();
            
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((WriteableBitmap)_Owner.Image1.ImageSource));
            encoder.Save(stream);
            bmImage.BeginInit();
            bmImage.CacheOption = BitmapCacheOption.OnLoad;
            bmImage.StreamSource = stream;
            bmImage.EndInit();
            return (MemoryStream)bmImage.StreamSource;
        }

        private static Bitmap GetBitmap(ImageSource imageSource)
        {
            MemoryStream ms = new MemoryStream();
            BitmapImage bitmapImage = imageSource as BitmapImage;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
           

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(ms);

            return new Bitmap(ms);
        }

        private void PassUp(object sender, EventArgs e)
        {
            OnImageProcessed(e);
        }

        protected virtual void OnImageProcessed(EventArgs e)
        {
            EventHandler handler = ImageProcessed;
            if(handler != null)
            {
                handler(this, e);
            }
        }
    }
}
