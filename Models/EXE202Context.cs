using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EXE202.Models
{
    public partial class EXE202Context : DbContext
    {
        public EXE202Context()
        {
        }

        public EXE202Context(DbContextOptions<EXE202Context> options)
            : base(options)
        {
        }

        public virtual DbSet<BookingContract> BookingContracts { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Feedback> Feedbacks { get; set; } = null!;
        public virtual DbSet<Homestay> Homestays { get; set; } = null!;
        public virtual DbSet<Host> Hosts { get; set; } = null!;
        public virtual DbSet<ImageHomestay> ImageHomestays { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=TamDaoStay_DB;Integrated Security = true;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingContract>(entity =>
            {
                entity.HasKey(e => e.BookingId)
                    .HasName("PK__BookingC__73951ACD1E54EF4F");

                entity.ToTable("BookingContract");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.Destination).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.BookingContracts)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__BookingCo__Custo__6754599E");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");

                entity.HasIndex(e => e.Email, "UQ__Customer__A9D105345985F15D")
                    .IsUnique();

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Image).HasMaxLength(500);

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.UnlockTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.FeedbackContent).HasMaxLength(1000);

                entity.Property(e => e.FeedbackDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HomestayId).HasColumnName("HomestayID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Feedback__Custom__6C190EBB");

                entity.HasOne(d => d.Homestay)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.HomestayId)
                    .HasConstraintName("FK__Feedback__Homest__6D0D32F4");
            });

            modelBuilder.Entity<Homestay>(entity =>
            {
                entity.ToTable("Homestay");

                entity.Property(e => e.HomestayId).HasColumnName("HomestayID");

                entity.Property(e => e.Address).HasColumnType("text");

                entity.Property(e => e.AirConditional).HasDefaultValueSql("((0))");

                entity.Property(e => e.Balcony).HasDefaultValueSql("((0))");

                entity.Property(e => e.BreakfirstIncluded).HasDefaultValueSql("((0))");

                entity.Property(e => e.Cafe).HasDefaultValueSql("((0))");

                entity.Property(e => e.City)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.Gym).HasDefaultValueSql("((0))");

                entity.Property(e => e.HostId).HasColumnName("HostID");

                entity.Property(e => e.Kitchen).HasDefaultValueSql("((0))");

                entity.Property(e => e.Laundry).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Parking).HasDefaultValueSql("((0))");

                entity.Property(e => e.PetFriendly).HasDefaultValueSql("((0))");

                entity.Property(e => e.Pool).HasDefaultValueSql("((0))");

                entity.Property(e => e.PricePerNight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.SmokingAllowed).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.Wifi).HasDefaultValueSql("((0))");

                entity.Property(e => e.RoomSize).HasColumnName("RoomSize");

                entity.HasOne(d => d.Host)
                    .WithMany(p => p.Homestays)
                    .HasForeignKey(d => d.HostId)
                    .HasConstraintName("FK__Homestay__HOST_I__5AEE82B9");
            });

            modelBuilder.Entity<Host>(entity =>
            {
                entity.ToTable("Host");

                entity.HasIndex(e => e.Email, "UQ__Host__A9D105342B7551F0")
                    .IsUnique();

                entity.Property(e => e.HostId).HasColumnName("HostID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<ImageHomestay>(entity =>
            {
                entity.HasKey(e => e.ImageId)
                    .HasName("PK__Image_Ho__7EA986891EC723C2");

                entity.ToTable("Image_Homestay");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HomestayId).HasColumnName("HomestayID");

                entity.Property(e => e.Image1).HasColumnType("text");

                entity.Property(e => e.Image2).HasColumnType("text");

                entity.Property(e => e.Image3).HasColumnType("text");

                entity.Property(e => e.Image4).HasColumnType("text");

                entity.Property(e => e.Image5).HasColumnType("text");

                entity.HasOne(d => d.Homestay)
                    .WithMany(p => p.ImageHomestays)
                    .HasForeignKey(d => d.HomestayId)
                    .HasConstraintName("FK__Image_Hom__HOMES__70DDC3D8");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Action).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.IssueDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Transacti__Custo__6477ECF3");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
