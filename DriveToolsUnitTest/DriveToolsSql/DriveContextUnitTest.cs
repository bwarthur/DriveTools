using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using DriveTools.Model;
using DriveToolsSql.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DriveToolsUnitTest.DriveToolsSql
{
    [TestClass]
    public class DriveContextUnitTest
    {
        [TestMethod]
        public void OpenContextTest()
        {
            using (var ctx = new DriveContext())
            {
                var property = ctx.Properties;
                Console.WriteLine(@"Property Count: " + property.Count());

                //var files = ctx.Files;
                //Console.WriteLine(@"File Count: " + files.Count());
            }
        }

        [TestMethod]
        public void SaveFileListTest()
        {
            var cert = new X509Certificate2(@"C:\FMO-DEVEL\Keys\SAFIRES-c911426d6622.p12", "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer("655398896227-6mo60i1r87i7vcgpq8e8tk460hei4a36@developer.gserviceaccount.com")
                {
                    Scopes = new[] { DriveService.Scope.Drive }
                }.FromCertificate(cert));

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SAFIRES",
            });

            var model = new GoogleDriveModel(service);

            //var files = model.GetAllFiles("not '0B3tlH3Zvt1QAdkpCUExsbDlicmc' in parents");
            var files = model.GetAllFiles().Result;

            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive3");

            var collection = database.GetCollection<File>("files");

            collection.InsertBatch(files);
        }

        [TestMethod]
        public void NoSqlDbAccess()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive1");

            var collection = database.GetCollection<File>("files");

            var entity = new File() {Id = "10023"};
            collection.Insert(entity);
        }

        [TestMethod]
        public void ListKeepFiles()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive2");

            var collection = database.GetCollection<BsonDocument>("keep");
            var keepList = collection.FindAll().Select(keep => keep.GetValue("keepId").AsString).ToList();
        }

        [TestMethod]
        public void CreateRemoveList()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive2");

            var removeList = new List<string>();

            var fileCollection = database.GetCollection<File>("files");
            var files = fileCollection.FindAll().Where(f => !String.IsNullOrEmpty(f.Md5Checksum)).ToList();

            var md5Collection = database.GetCollection<BsonDocument>("md5");
            var md5s = md5Collection.FindAll().Select(md5 => new string[]
            {
                md5.GetValue("keepId").ToString(),
                md5.GetValue("_id").ToString()
            }).ToList();

            files.ForEach(f =>
            {
                var md5 = md5s.FirstOrDefault(m => m[1] == f.Md5Checksum);
                if (md5[0] != f.Id) removeList.Add(f.Id);
            });

            var removeCollection = database.GetCollection<string>("remove");
            var removeDocumentList = removeList.Select(r => new BsonDocument().Add(new BsonElement("removeId", r)));

            removeCollection.InsertBatch(removeDocumentList);
        }

        [TestMethod]
        public void ValidateRemoveList()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var server = client.GetServer();
            var database = server.GetDatabase("Drive2");

            var keepCollection = database.GetCollection<BsonDocument>("keep");
            var keepList = keepCollection.FindAll().Select(keep => keep.GetValue("keepId").AsString).ToList();

            var removeCollection = database.GetCollection<BsonDocument>("remove");
            var removeList =
                removeCollection.FindAll()
                    .Select(remove => remove.GetValue("removeId").AsString)
                    .OrderBy(r => r)
                    .ToList();

            var fileCollection = database.GetCollection<File>("files");
            var fileIds =
                fileCollection.FindAll()
                    .Where(f => !String.IsNullOrEmpty(f.Md5Checksum))
                    .Select(file => file.Id)
                    .ToList();

            var diffRemoveList = fileIds.Except(keepList).OrderBy(r => r).ToList();

            var diffListOnly = diffRemoveList.Except(removeList).ToList();
            var removeListOnly = removeList.Except(diffRemoveList).ToList();

            if (diffListOnly.Any() && removeListOnly.Any())
            {
                Console.WriteLine(@"Diff List Only:");
                diffListOnly.ForEach(Console.WriteLine);

                Console.WriteLine(@"\n\nRemove List Only");
                removeListOnly.ForEach(Console.WriteLine);

                Assert.Fail();
            }
        }
    }
}
