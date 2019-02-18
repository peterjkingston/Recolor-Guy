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
    class BackgroundWorks
    {
        public static ImageBrush iBrush { get; set; }

        private delegate ImageBrush ImageProcess(MainWindow mainWindow);

        public static Bitmap LoadRecolor(string Directory)
        {
            Bitmap result;

            result = new Bitmap(Directory);

            return result;
        }

        public static string FromFile()
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
        
        public static ImageSource UpdateImage(Bitmap NewImage)
        {
            ImageSource imageSource = (ImageSource)WPFBitmapConverter.Convert(NewImage);

            return imageSource;
        }

        public static void UpdateBothImages(MainWindow window,Border left, Border right)
        {
            if (window.IsOpen)
            {
                Recolorer.Spectrum leftbox = (Recolorer.Spectrum)window.FromBox.SelectedItem;
                Recolorer.Spectrum rightbox = (Recolorer.Spectrum)window.ToBox.SelectedItem;

                if (right != null)
                {
                    right.Background = left.Background.Clone();
                    BitmapSource bSource = ((ImageBrush)right.Background).ImageSource as BitmapSource;
                    WriteableBitmap wBitmap = new WriteableBitmap(bSource);

                    ((ImageBrush)window.Image2Border.Background).ImageSource = wBitmap;

                    window.Dispatcher.Invoke(() =>
                    {
                        Recolorer r = new Recolorer(wBitmap, window.Dispatcher, leftbox, rightbox);
                    });
                }
            }
        }
        
        public static void SaveAs(MainWindow window)
        {
            MemoryStream ms = GetImageStream(((ImageBrush)window.Image1Border.Background).ImageSource);

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

        public static void Apply(MainWindow window)
        {
            ImageBrush iBrush = (ImageBrush)window.Image2Border.Background;
            window.Image1Border.Background = iBrush;
        }

        public static void OpenImage(MainWindow window)
        {
            string directory = FromFile();

            if (directory != "")
            {
                Bitmap left = LoadRecolor(directory);

                ImageBrush iBrush = new ImageBrush(UpdateImage(left));
                window.Image1Border.Background = iBrush;
                window.Image2Border.Background = iBrush;
                window.IsOpen = true;
            }
        }

        public static MemoryStream GetImageStream(ImageSource fill)
        {
            BitmapImage bi = (BitmapImage)fill;

            return (MemoryStream)bi.StreamSource;
        }

        private static Bitmap GetBitmap(System.Windows.Controls.Border image)
        {
            ImageSource iSource = ((ImageBrush)image.Background).ImageSource;
            MemoryStream ms = new MemoryStream();
            BitmapImage bitmapImage = iSource as BitmapImage;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
           

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(ms);

            return new Bitmap(ms);
        }
    }
}
