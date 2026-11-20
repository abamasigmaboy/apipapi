using Microsoft.EntityFrameworkCore;
using TradeRestAPI.Models;

namespace TradeRestAPI.Models.Data
{
    public class TradeDbContext : DbContext
    {
        public TradeDbContext()
        {
            try
            {
                Database.EnsureCreated();
            }
            catch
            {
                // Игнорируем ошибку подключения при отсутствии базы в момент инициализации
            }
        }

        public TradeDbContext(DbContextOptions<TradeDbContext> options)
            : base(options)
        {
            try
            {
                Database.EnsureCreated();
            }
            catch
            {
                // Игнорируем ошибку при первом запуске если еще не готов postgres
            }
        }

        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<Sale> Sales { get; set; } = null!;
        public virtual DbSet<Client> Clients { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=trade_db;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Отделы
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentCode).HasName("departments_pk");
                entity.ToTable("departments");

                entity.Property(e => e.DepartmentCode).HasColumnName("department_code");
                entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
                entity.Property(e => e.Manager).HasMaxLength(150).HasColumnName("manager");
                entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
                entity.Property(e => e.DailyPlan).HasColumnType("numeric(15,2)").HasColumnName("daily_plan");
            });

            // Товары
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Article).HasName("products_pk");
                entity.ToTable("products");

                entity.Property(e => e.Article).HasColumnName("article");
                entity.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");
                entity.Property(e => e.Unit).HasMaxLength(50).HasColumnName("unit");
                entity.Property(e => e.Price).HasColumnType("numeric(15,2)").HasColumnName("price");
                entity.Property(e => e.DepartmentCode).HasColumnName("department_code");

                entity.HasOne(d => d.DepartmentNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.DepartmentCode)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_products_department");
            });

            // Продажи
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.SaleId).HasName("sales_pk");
                entity.ToTable("sales");

                entity.Property(e => e.SaleId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("sale_id");

                entity.Property(e => e.Article).HasColumnName("article");
                entity.Property(e => e.SaleDate).HasColumnName("sale_date");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.DepartmentCode).HasColumnName("department_code");

                entity.Ignore(e => e.TotalAmountRub);

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.Article)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_sales_product");

                entity.HasOne(d => d.DepartmentNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.DepartmentCode)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_sales_department");
            });

            // Пользователи / Клиенты
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Login).HasName("clients_pk");
                entity.ToTable("clients");

                entity.Property(e => e.Login).HasMaxLength(100).HasColumnName("login");
                entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
                entity.Property(e => e.Firstname).HasMaxLength(100).HasColumnName("firstname");
                entity.Property(e => e.Lastname).HasMaxLength(100).HasColumnName("lastname");
                entity.Property(e => e.Middlename).HasMaxLength(100).HasColumnName("middlename");
                entity.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
                entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
                entity.Property(e => e.Passport).HasMaxLength(100).HasColumnName("passport");
                entity.Property(e => e.Type).HasMaxLength(50).HasColumnName("type");
            });
        }
    }
}
