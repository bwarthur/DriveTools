using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DriveTools.Model;
using DriveToolsSql.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
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
            var files = model.GetAllFiles("modifiedDate > '2015-02-01' and '0B3tlH3Zvt1QAdkpCUExsbDlicmc' in parents");

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
            //removeList.ForEach(r => model.DeleteFile(r));

            Parallel.ForEach(removeList, r => model.DeleteFile(r));
        }

        [TestMethod]
        public void GetFileAsyncTest()
        {
            var service = BuildService();
            var model = new GoogleDriveModel(service);

            var files = model.GetAllFiles();
            //files.ForEach(f =>
            //{
            //    var file = model.GetFileAsync(f).Result;
            //    if (file != null)
            //        System.IO.File.WriteAllBytes(@"E:\FMO-Documents\" + f.Id + "." + f.FileExtension, file);
            //});

            var filesToResume = new List<File>();
            var errorFiles = new List<File>();

            Parallel.ForEach(files, f =>
            {
                if (System.IO.File.Exists(@"E:\FMO-Documents\" + f.Id + "." + f.FileExtension)) return;
                var file = model.GetFileAsync(f).Result;
                if (file != null)
                    System.IO.File.WriteAllBytes(@"E:\FMO-Documents\" + f.Id + "." + f.FileExtension, file);
                else
                    filesToResume.Add(f);
            });

            var updatedService = BuildService();
            var updatedModel = new GoogleDriveModel(updatedService);

            Parallel.ForEach(filesToResume, f =>
            {
                var file = updatedModel.GetFileAsync(f).Result;
                if (file != null)
                    System.IO.File.WriteAllBytes(@"E:\FMO-Documents\" + f.Id + "." + f.FileExtension, file);
                else
                    errorFiles.Add(f);
            });

            Console.WriteLine(@"Files With Errors: " + errorFiles.Count);
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
