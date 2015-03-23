using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using DriveTools.Events;
using DriveTools.Model;
using FirstFloor.ModernUI.Presentation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using File = Google.Apis.Drive.v2.Data.File;

namespace DriveTools.ViewModels
{
    public class RemoveDuplicateWindowViewModel : INotifyPropertyChanged
    {
        private TextWriter _writer = null;

        private RelayCommand _runCommand;
        private RelayCommand _refreshCommand;

        private int _startCount = 0;
        private int _endCount = 0;
        private DateTime _countStartTime;

        private string _timestamp;

        private readonly List<string> _errorList = new List<string>();
        private readonly List<string> _successList = new List<string>();

        private readonly List<List<string>> _errorBatchList = new List<List<string>>();
        private readonly List<List<string>> _successBatchList = new List<List<string>>();

        public RemoveDuplicateWindowViewModel()
        {
            _timestamp = DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + "_" +
                         (DateTime.Now.TimeOfDay.TotalMinutes * 100).ToString("N0");
            //_timestamp = DateTime.Now.Ticks.ToString();

            MongoDBServerAddress = @"mongodb://localhost:27017";
            DatabaseName = "Drive_" + _timestamp;
            CollectionName = "files";
            ParentFolderId = "0B3tlH3Zvt1QAdkpCUExsbDlicmc";
        }

        private void TestTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            NotifyPropertyChanged("Status");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int StartCount
        {
            get { return _startCount; }
            set { _startCount = value; UpdateStatus(StartCount + @"/" + EndCount); }
        }

        public int EndCount
        {
            get { return _endCount; }
            set { _endCount = value; UpdateStatus(StartCount + @"/" + EndCount); }
        }

        public string Status { get; set; }

        public string MongoDBServerAddress { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string ParentFolderId { get; set; }
        public int FileListCountCap { get; set; }

        public ICommand RunCommand
        {
            get { return _runCommand ?? (_runCommand = new RelayCommand(Run)); }
        }

        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); }
        }

        private void Refresh(object args)
        {

            //Reset Timestamp
            _timestamp = DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + "_" +
                         (DateTime.Now.TimeOfDay.TotalMinutes * 100).ToString("N0");
            DatabaseName = "Drive_" + _timestamp;

            NotifyPropertyChanged("DatabaseName");
        }

        private async void Run(object args)
        {
            EventHandlers.GetAllFilesStatusEvent += EventHandlersOnGetAllFilesStatusEvent;
            _writer = new TextBoxStreamWriter((TextBox) args);
            Console.SetOut(_writer);

            Console.WriteLine(@"Loading...");

            var files = await GetFiles();
            await SaveFilesToDB(files, CollectionName);

            var removeList = await CreateRemoveList(files);
            EndCount = removeList.Count;
            Console.WriteLine(removeList.Count + @" files pending deletion");

            var isValid = await ValidateRemoveList(files, removeList);

            if (!isValid)
            {
                Console.WriteLine(@"Unable to validate remove list, ending...");
                return;
            }

            if (removeList.Any())
                await SaveFilesToDB(removeList, "RemoveList");

            //RemoveDocuments(removeList);
            RemoveDocumentsBatch(removeList);

            EventHandlers.GetAllFilesStatusEvent -= EventHandlersOnGetAllFilesStatusEvent;
        }

        private void EventHandlersOnGetAllFilesStatusEvent(object source, GetAllFilesStatusEventHandlerArgs args)
        {
            Console.WriteLine(@"Current File.List() Count: " + args.CurrentFileCount);
        }

        private async Task<List<File>> GetFiles()
        {
            var model = new GoogleDriveModel(MainWindow.Service);

            //var files = model.GetAllFiles("not '0B3tlH3Zvt1QAdkpCUExsbDlicmc' in parents");
            var files = await model.GetAllFiles("'" + ParentFolderId + "' in parents", FileListCountCap);

            Console.WriteLine(@"files.list() returned " + files.Count + @" results");

            return files;
        }

        private async Task SaveFilesToDB(IEnumerable<File> files, string collectionName)
        {
            Console.Write(@"Saving records to db...");
            await TaskEx.Run(() =>
            {
                var client = new MongoClient(MongoDBServerAddress);
                var server = client.GetServer();
                var database = server.GetDatabase(DatabaseName);

                var collection =
                    database.GetCollection<File>(collectionName);

                collection.InsertBatch(files);
            });
            Console.WriteLine(@"Complete");
        }

        private async Task SaveListToDB<T>(IEnumerable<T> data, string collectionName)
        {
            Console.Write(@"Saving records to " + collectionName + @" collection in db...");
            await TaskEx.Run(() =>
            {
                var client = new MongoClient(MongoDBServerAddress);
                var server = client.GetServer();
                var database = server.GetDatabase(DatabaseName);

                var collection =
                    database.GetCollection<T>(collectionName);

                var batch = data.Select(d => new BsonDocument {{"value", BsonValue.Create(d)}});
                collection.InsertBatch(batch);
            });
            Console.WriteLine(@"Complete");
        }

        private async Task<List<File>> CreateRemoveList(List<File> files)
        {
            Console.WriteLine(@"Creating remove list...");

            return await TaskEx.Run(() =>
            {

                var md5List = files.GroupBy(f => f.Md5Checksum).Select(f => f.Key).ToList();

                var keepList = new List<File>();
                md5List.ForEach(m => keepList.Add(files.FirstOrDefault(f => f.Md5Checksum == m)));

                return files.Except(keepList).ToList();
            });
        }

        private async Task<bool> ValidateRemoveList(List<File> files, List<File> removeList)
        {
            var isValid = true;

            Console.WriteLine(@"Validating Remove List");

            //foreach (var f in removeList)
            //{
            //    count++;
            //    var matchedFiles = files.Where(c => c.Md5Checksum == f.Md5Checksum);
            //    if (matchedFiles.Count() < 2)
            //        isValid = false;
            //}

            StartCount = 0;
            EndCount = removeList.Count;
            _countStartTime = DateTime.Now;
            await TaskEx.Run(() =>
            {
                removeList.ForEach(f =>
                {
                    StartCount++;
                    var matchedFiles = files.Where(c => c.Md5Checksum == f.Md5Checksum);
                    if (matchedFiles.Count() < 2)
                        isValid = false;
                });
            });

            return isValid;
        }

        private async void RemoveDocumentsBatch(List<File> removeList)
        {
            var model = new GoogleDriveModel(MainWindow.Service);

            Console.WriteLine(@"Removing documents...");

            var partitions = removeList.Partition(1000).ToList();
            StartCount = 0;
            EndCount = removeList.Count;

            _countStartTime = DateTime.Now;
            await partitions.ForEachAsync(6, async f =>
            {
                var result = await model.DeleteFileBatchAsync(f.Select(v => v.Id).ToList());
                StartCount += f.Count;

                if (result) _successBatchList.Add(f.Select(v => v.Id).ToList());
                else _errorBatchList.Add(f.Select(v => v.Id).ToList());
            });



            if (_errorBatchList.Any())
                await SaveListToDB(_errorBatchList, "ErrorBatchList");

            if (_successBatchList.Any())
            {
                await SaveListToDB(_successBatchList, "SuccessBatchList");
            }

            Console.WriteLine(@"Files.Delete() complete...");
            Console.WriteLine(@"Partitions deleted: " + _successBatchList.Count);
            Console.WriteLine(@"Partitions with errors: " + _errorBatchList.Count);
            Console.WriteLine(@"Files deleted: " + _successBatchList.Sum(s => s.Count));
            Console.WriteLine(@"Files with errors: " + _errorBatchList.Sum(s => s.Count));
            Console.WriteLine();
        }

        private async void RemoveDocuments(List<File> removeList)
        {
            var model = new GoogleDriveModel(MainWindow.Service);

            Console.WriteLine(@"Removing documents...");

            //Parallel.ForEach(removeList, f =>
            //{
            //    var result = model.DeleteFile(f.Id);

            //    if (result) _successList.Add(f.Id);
            //    else _errorList.Add(f.Id);
            //});

            StartCount = 0;
            EndCount = removeList.Count;
            _countStartTime = DateTime.Now;
            await removeList.ForEachAsync(async f =>
            {
                var result = await model.DeleteFileAsync(f.Id);
                StartCount++;

                if (result) _successList.Add(f.Id);
                else _errorList.Add(f.Id);
            });

            //removeList.ForEach(async f =>
            //{
            //    var result = await model.DeleteFileAsync(f.Id);

            //    if (result) successList.Add(f.Id);
            //    else errorList.Add(f.Id);
            //});

            if (_errorList.Any())
                await SaveListToDB(_errorList, "ErrorList");

            if (_successList.Any())
                await SaveListToDB(_successList, "SuccessList");

            Console.WriteLine(@"Files.Delete() complete...");
            Console.WriteLine(@"Files deleted: " + _successList.Count);
            Console.WriteLine(@"Files with errors: " + _errorList.Count);
            Console.WriteLine();
        }

        private void UpdateStatus(string status)
        {
            var timespan = (DateTime.Now - _countStartTime).TotalSeconds;
            var rate = _startCount / timespan;

            Status = status + @" (" + rate.ToString("N2") + @" records/sec)";
            
            NotifyPropertyChanged("Status");
        }
    }
}
