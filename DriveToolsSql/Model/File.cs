using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveToolsSql.Model
{
    public class DriveFile
    {
        public DriveFile()
        {
            
        }

        public DriveFile(Google.Apis.Drive.v2.Data.File driveFile)
        {
            Id = driveFile.Id;
            AlternateLink = driveFile.AlternateLink;
            Editable = driveFile.Editable;
            EmbedLink = driveFile.EmbedLink;
            ETag = driveFile.ETag;
            ExplicitlyTrashed = driveFile.ExplicitlyTrashed;
            FileExtension = driveFile.FileExtension;
            FileSize = driveFile.FileSize;
            FolderColorRgb = driveFile.FolderColorRgb;
            HeadRevisionId = driveFile.HeadRevisionId;
            Trashed = driveFile.Labels.Trashed;
            LastModifyingUserName = driveFile.LastModifyingUserName;
            Md5Checksum = driveFile.Md5Checksum;
            MimeType = driveFile.MimeType;
            ModifiedDateRaw = driveFile.ModifiedDateRaw;
            ModifiedDate = driveFile.ModifiedDate;
            OriginalFilename = driveFile.OriginalFilename;
            QuotaBytesUsed = driveFile.QuotaBytesUsed;
            Title = driveFile.Title;
            Version = driveFile.Version;
        }

        [Key]
        public string Id { get; set; }
        public string AlternateLink { get; set; }
        public bool? AppDataContents { get; set; }
        public bool? Copyable { get; set; }
        public string CreateDateRaw { get; set; }
        public DateTime? CreateDate { get; set; }
        public string DefaultOpenWithLink { get; set; }
        public string Description { get; set; }
        public string DownloadUrl { get; set; }
        public bool? Editable { get; set; }
        public string EmbedLink { get; set; }
        public string ETag { get; set; }
        public bool? ExplicitlyTrashed { get; set; }
        public string FileExtension { get; set; }
        public long? FileSize { get; set; }
        public string FolderColorRgb { get; set; }
        public string HeadRevisionId { get; set; }
        public bool? Trashed { get; set; }
        public string LastModifyingUserName { get; set; }
        public string Md5Checksum { get; set; }
        public string MimeType { get; set; }
        public string ModifiedDateRaw { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string OriginalFilename { get; set; }
        public long? QuotaBytesUsed { get; set; }
        public string Title { get; set; }
        public long? Version { get; set; }

    }
}
