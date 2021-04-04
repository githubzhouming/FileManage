using System;
using FileManage.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileManage.DBContexts
{
    public partial class BaseContext : DbContext
    {
        public BaseContext()
        {
        }

        public BaseContext(DbContextOptions<DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FMFileInfo> FMFileInfos { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new Exception ("数据库连接字符串未定义");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<FMFileInfo>(entity =>
            {
                entity.ToTable("fm_fileinfo");
                entity.Property(e => e.FMFileInfoId)
                    .HasColumnName("fmfileinfoid")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("createdon")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modifiedon")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Path)
                    .HasColumnName("path")
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

            });

        }
    }
}
