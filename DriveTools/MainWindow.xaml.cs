using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace DriveTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static byte[] Certificate { get; set; }
        public static CertificateType SelectedCertificateType { get; set; }

        public enum CertificateType
        {
            P12,
            JSON,
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenJSON_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.JSON;
            OpenCert("JSON (*.json)|*.json");
        }

        private void OpenP12_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.P12;
            OpenCert("P12 (*.p12)|*.p12");
        }

        private static void OpenCert(string fileExtensionFilter)
        {
            var stream = new MemoryStream();
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Filter = fileExtensionFilter + @"|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            try
            {
                openFileDialog.OpenFile().CopyTo(stream);
                Certificate = stream.ToArray();
            }
            catch (Exception ex)
            {
                    
                throw;
            }
        }
    }
}
