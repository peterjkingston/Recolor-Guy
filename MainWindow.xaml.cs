using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Recolor_Guy;

namespace Recolor_Guy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsOpen { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            IsOpen = false;

            for(Recolorer.Spectrum color = Recolorer.Spectrum.Red; color <= Recolorer.Spectrum.Black; color += 30)
            {
                FromBox.Items.Add(color);
                ToBox.Items.Add(color);
            }

            FromBox.SelectedIndex = 0;
            ToBox.SelectedIndex = 0;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorks.OpenImage(this);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { BackgroundWorks.UpdateBothImages(this, Image1Border, Image2Border); });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorks.SaveAs(this);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorks.Apply(this);
        }
    }
}
