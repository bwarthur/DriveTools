using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DriveTools.Model;
using DriveToolsSql.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DriveToolsUnitTest.GoogleDrive
{
    [TestClass]
    public class GoogleDriveModelUnitTest
    {
        private const string ServiceAccountEmail = "655398896227-6mo60i1r87i7vcgpq8e8tk460hei4a36@developer.gserviceaccount.com";
        //private const string ServiceAccountEmail = "655398896227-ml5i3qppr7rqi6cn40vovmkm8tu2i254@developer.gserviceaccount.com";

        [TestMethod]
        public void GetAboutUserUnitTest()
        {
            var service = BuildService();
            var model = new GoogleDriveModel(service);
            var about = model.GetAboutUser();

            Console.WriteLine(@"Username: " + about.Name);
            Console.WriteLine(@"Total quota: " + about.QuotaBytesTotal);
            Console.WriteLine(@"Used quota:" + about.QuotaBytesUsed);
            about.QuotaBytesByService.ToList()
                .ForEach(q => Console.WriteLine(@"  " + q.ServiceName + @": " + q.BytesUsed));
        }

        [TestMethod]
        public void GetAllFilesTest()
        {
            var service = BuildService();
            var model = new GoogleDriveModel(service);

            //var files = model.GetAllFiles("not '0B3tlH3Zvt1QAdkpCUExsbDlicmc' in parents");
            var files = model.GetAllFiles();

            Console.WriteLine(@"Total File Size: " + files.Select(f => f.QuotaBytesUsed).Sum());

            Console.WriteLine(@"File List (" + files.Count + @") :");
            foreach (var file in files)
            {
                Console.WriteLine(@"Title: " + file.Title);
                Console.WriteLine(@"   File Size: " + file.FileSize);
                Console.WriteLine(@"   Quota Bytes Used: " + file.QuotaBytesUsed);
                Console.WriteLine(@"   MIME Type: " + file.MimeType);
                Console.WriteLine(@"   Modified Date: " + file.ModifiedDate);
                Console.Write(@"   Owners: ");
                file.Owners.ToList().ForEach(q => Console.WriteLine(q.EmailAddress + @" "));
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void GetAllTrashedFilesTest()
        {
            var service = BuildService();
            var model = new GoogleDriveModel(service);

            //var files = model.GetAllFiles("not '0B3tlH3Zvt1QAdkpCUExsbDlicmc' in parents");
            var files = model.GetAllFiles("trashed=true");

            Console.WriteLine(@"Total File Size: " + files.Select(f => f.QuotaBytesUsed).Sum());

            Console.WriteLine(@"File List (" + files.Count + @") :");
            foreach (var file in files)
            {
                Console.WriteLine(@"Title: " + file.Title);
                Console.WriteLine(@"   File Size: " + file.FileSize);
                Console.WriteLine(@"   Quota Bytes Used: " + file.QuotaBytesUsed);
                Console.WriteLine(@"   MIME Type: " + file.MimeType);
                Console.WriteLine(@"   Modified Date: " + file.ModifiedDate);
                Console.Write(@"   Owners: ");
                file.Owners.ToList().ForEach(q => Console.WriteLine(q.EmailAddress + @" "));
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void DeleteFileTest()
        {
            var service = BuildService();
            var model = new GoogleDriveModel(service);

            var noSqlContext = new NoSqlContext();
            var removeList = noSqlContext.GetRemoveList();
            removeList.ForEach(r => model.DeleteFile(r));
        }

        private static DriveService BuildService()
        {
            var cert = new X509Certificate2(@"C:\FMO-DEVEL\Keys\SAFIRES-c911426d6622.p12", "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(ServiceAccountEmail)
                {
                    Scopes = new[] { DriveService.Scope.Drive }
                }.FromCertificate(cert));

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SAFIRES",
            });

            return service;
        }
    }
}
