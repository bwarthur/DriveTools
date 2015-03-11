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
            EnhancedLogging.Start();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenJSON_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.JSON;
            Certificate = Commands.OpenCommands.OpenJSONCert();
        }

        private void OpenP12_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.P12;
            Certificate = Commands.OpenCommands.OpenP12Cert();
        }

        private void OpenCertButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenP12_OnClick(sender, e);
        }
    }
}
