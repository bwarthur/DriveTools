using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v2.Data;

namespace DriveToolsSql.Model
{
    public class DriveContext : DbContext
    {
        public DriveContext() : base("DriveContextConnectionString")
        {
            
        }

        public DbSet<File> Files { get; set; }
        public DbSet<ParentReference> ParentReferences { get; set; } 
        public DbSet<User> Users { get; set; }
        //public DbSet<Google.Apis.Drive.v2.Data.Permission> Permissions { get; set; }
        public DbSet<Property> Properties { get; set; }
        //public DbSet<Google.Apis.Drive.v2.Data.Revision> Revisions { get; set; } 

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>().HasKey(p => p.Key);
            modelBuilder.Entity<File>().HasKey(f => f.Id);
            modelBuilder.Entity<ParentReference>().HasKey(p => p.Id);
            //modelBuilder.Entity<User>().HasKey(u => u.DisplayName);

            modelBuilder.Ignore<File.ImageMediaMetadataData>();
            modelBuilder.Ignore<User>();
            modelBuilder.Ignore<Permission>();

            modelBuilder.Entity<File>().Ignore(f => f.IndexableText);
            modelBuilder.Entity<File>().Ignore(f => f.Labels);
            modelBuilder.Entity<File>().Ignore(f => f.Thumbnail);
            modelBuilder.Entity<File>().Ignore(f => f.VideoMediaMetadata);
            modelBuilder.Entity<User>().Ignore(u => u.Picture);
        }
    }

}
