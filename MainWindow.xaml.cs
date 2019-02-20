using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Recolor_Guy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsOpen;
        private BackgroundWorks _BackgroundWorks;
        public ImageBrush Image1 { get; set; }
        public ImageBrush Image2 { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            for (Spectrum color = Spectrum.Red; color <= Spectrum.Black; color += 30)
            {
                FromBox.Items.Add(color);
                ToBox.Items.Add(color);
            }

            FromBox.SelectedIndex = 0;
            FromBox.IsEnabled = false;

            ToBox.SelectedIndex = 0;
            ToBox.IsEnabled = false;

            _BackgroundWorks = new BackgroundWorks(this);

            IsOpen = true;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (this != null)
                _BackgroundWorks.OpenImage();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this != null)
                _BackgroundWorks.SaveAs();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (this != null)
                _BackgroundWorks.ApplyImage();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsOpen)
            {
                if (ToBox.SelectedItem != null)
                {
                    
                    _BackgroundWorks.ImageProcessed += Processing_Finished;
                    _BackgroundWorks.UpdatePreviewImage(
                                                   (Spectrum)FromBox.SelectedItem,
                                                   (Spectrum)ToBox.SelectedItem);
                    FromBox.IsEnabled = false;
                    ToBox.IsEnabled = false;
                }
            }
        }

        public void Processing_Finished(object sender, EventArgs e)
        {
            FromBox.IsEnabled = true;
            ToBox.IsEnabled = true;
            
            System.Diagnostics.Debug.Print($"Are images equal? {Image1.ImageSource.Equals(Image2.ImageSource)}");
        }
    }
}
