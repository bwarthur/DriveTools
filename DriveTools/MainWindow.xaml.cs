using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Forms;
using DriveTools.Commands;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;

namespace DriveTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DriveService Service { get; set; }
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

        private void BuildService()
        {
            var cert = new X509Certificate2(Certificate, "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer("655398896227-6mo60i1r87i7vcgpq8e8tk460hei4a36@developer.gserviceaccount.com")
                {
                    Scopes = new[] { DriveService.Scope.Drive }
                }.FromCertificate(cert));

            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SAFIRES",
            });
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenJSON_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.JSON;
            Certificate = OpenCommands.OpenJSONCert();
            BuildService();
        }

        private void OpenP12_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCertificateType = CertificateType.P12;
            Certificate = OpenCommands.OpenP12Cert();
            BuildService();
        }

        private void OpenCertButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenP12_OnClick(sender, e);
        }

        private void RemoveDuplicated_OnClick(object sender, RoutedEventArgs e)
        {
            ActionCommands.OpenRemoveDuplicateWindow();
        }

        private void AboutUser_OnClick(object sender, RoutedEventArgs e)
        {
            ActionCommands.OpenAboutUser(MainViewer);
        }
    }
}
