using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SimplePM.WebAPI.Library.Models;

namespace SimplePM.WebAPI.Library.Repositories.Models
{
    public partial class AssetsContext : DbContext
    {
        public AssetsContext()
        {
        }

        public AssetsContext(DbContextOptions<AssetsContext> options)
            : base(options)
        {
        }
        public virtual DbSet<UserData> DistributionData { get; set; }
        public virtual DbSet<RepositoryEntry> Entries { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // reserved for future features
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

            modelBuilder.Entity<UserData>(entity =>
            {

                entity.Property(e => e.ID)
                    .HasColumnName("ID")
                    .HasMaxLength(100);

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasColumnName("Login")
                    .HasMaxLength(256);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("Password")
                    .HasMaxLength(512);

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasColumnName("Salt")
                    .HasMaxLength(128);

                entity.Property(e => e.MasterPassword)
                    .HasColumnName("MasterPassword")
                    .HasMaxLength(512);

                entity.Property(e => e.MasterSalt)
                    .HasColumnName("MasterSalt")
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<RepositoryEntry>(entity =>
            {
                entity.ToTable("entries");

                entity.Property(e => e.ID)
                    .HasMaxLength(100)
                    .HasColumnName("ID");

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasColumnName("Version");

                entity.Property(e => e.Name)
                    .HasColumnName("Name")
                    .HasMaxLength(512);

                entity.Property(e => e.URL)
                    .HasColumnName("Url")
                    .HasMaxLength(4000);

                entity.Property(e => e.Login)
                    .HasColumnName("login")
                    .HasMaxLength(512);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(512);

                entity.HasOne(d => d.UserData)
                    .WithMany(p => p.Entries)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Entries_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelbuilder);
    }
}