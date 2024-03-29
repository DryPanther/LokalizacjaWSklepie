﻿

using Microsoft.EntityFrameworkCore;

namespace LokalizacjaWSklepie.Models;

public partial class LokalizacjaWsklepieContext : DbContext
{
    public LokalizacjaWsklepieContext()
    {
    }

    public LokalizacjaWsklepieContext(DbContextOptions<LokalizacjaWsklepieContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Container> Containers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductQuantity> ProductQuantities { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<ShoppingList> ShoppingLists { get; set; }

    public virtual DbSet<ShoppingListProduct> ShoppingListProducts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=lokalizacjawsklepie.cv5pxbaesbr7.eu-central-1.rds.amazonaws.com,1433;Initial Catalog=LokalizacjaWSklepie;User Id=admin;Password=admin123;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Container>(entity =>
        {
            entity.Property(e => e.ContainerType)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Shop).WithMany(p => p.Containers)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_Containers_Shops");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Barcode)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.QuantityType)
                .IsRequired()
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<ProductQuantity>(entity =>
        {
            entity.HasKey(e => new { e.ShopId, e.ProductId });

            entity.HasOne(d => d.Product).WithMany(p => p.ProductQuantities)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuantities_Products");

            entity.HasOne(d => d.Shop).WithMany(p => p.ProductQuantities)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductQuantities_Shops");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .IsRequired()
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Street)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ShoppingList>(entity =>
        {
            entity.Property(e => e.ListName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.ShoppingLists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingLists_Users");
        });

        modelBuilder.Entity<ShoppingListProduct>(entity =>
        {
            entity.HasKey(e => new { e.ShoppingListId, e.ProductId });

            entity.HasOne(d => d.Product).WithMany(p => p.ShoppingListProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingListProducts_Products");

            entity.HasOne(d => d.ShoppingList).WithMany(p => p.ShoppingListProducts)
                .HasForeignKey(d => d.ShoppingListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingListProducts_ShoppingListProducts");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "IX_Users").IsUnique();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
