using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;

namespace DriveTools.Model
{
    public class GoogleDriveModel
    {
        private DriveService Service { get; set; }

        public GoogleDriveModel(DriveService service)
        {
            Service = service;
        }

        public GoogleDriveModel()
        {
            
        }

        public Permission UpdatePermission( string fileId, string permissionId,
            string newRole)
        {
            try
            {
                var permission = Service.Permissions.Get(fileId, permissionId).Execute();
                permission.Role = newRole;

                return Service.Permissions.Update(permission, fileId, permissionId).Execute();
            }
            catch (Exception ex)
            {
                EnhancedLogging.Log.Error(ex);
                throw;
            }
        }

        public About GetAboutUser()
        {
            try
            {
                var about = Service.About.Get().Execute();

                return about;
            }
            catch (Exception ex)
            {
                EnhancedLogging.Log.Error(ex);
                throw;
            }
        }

        public List<File> GetAllFiles(string query = null)
        {
            var result = new List<File>();
            var request = Service.Files.List();

            request.MaxResults = 0;
            //request.Fields =
            //    "items(id, title, mimeType, description, createdDate, modifiedDate, downloadUrl, fileExtension, fileSize, quotaBytesUsed, parents, ownerNames, owners), nextPageToken, nextLink";

            if (!String.IsNullOrEmpty(query))
                request.Q = query;

            do
            {
                try
                {
                    var files = request.Execute();

                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception ex)
                {
                    EnhancedLogging.Log.Error(ex);
                    throw;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public bool DeleteFile(string id)
        {
            try
            {
                var result = Service.Files.Delete(id).Execute();
                return true;
            }
            catch (Exception ex)
            {
                EnhancedLogging.Log.Error(ex);
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string id)
        {
            var file = Service.Files.Get(id).Execute();
            return await GetFileAsync(file);
        }

        public async Task<byte[]> GetFileAsync(File file)
        {
            if (string.IsNullOrEmpty(file.DownloadUrl)) return null;
            try
            {
                var fileBytes = await Service.HttpClient.GetByteArrayAsync(file.DownloadUrl);

                return fileBytes;
            }
            catch (Exception ex)
            {
                EnhancedLogging.Log.Error(ex);
                return null;
            }
        }
    }
}
