using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SupplyTrackingSystem.Models;
using SupplyTrackingSystem.Models.Reports;
using Microsoft.Data.Sqlite;

namespace SupplyTrackingSystem.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static DatabaseService? _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        public DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop.db");
            _connectionString = $"Data Source={dbPath};";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createDepartments = @"
                CREATE TABLE IF NOT EXISTS Departments (
                    DepartmentCode INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Manager TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    DailyPlan REAL NOT NULL CHECK(DailyPlan >= 0)
                )";
            using (var cmd = new SqliteCommand(createDepartments, connection))
                cmd.ExecuteNonQuery();

            var createProducts = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Article INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Unit TEXT NOT NULL,
                    Price REAL NOT NULL CHECK(Price >= 0),
                    DepartmentCode INTEGER NOT NULL,
                    FOREIGN KEY (DepartmentCode) REFERENCES Departments(DepartmentCode)
                )";
            using (var cmd = new SqliteCommand(createProducts, connection))
                cmd.ExecuteNonQuery();

            var createSales = @"
                CREATE TABLE IF NOT EXISTS Sales (
                    SaleId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Article INTEGER NOT NULL,
                    SaleDate TEXT NOT NULL,
                    Quantity INTEGER NOT NULL CHECK(Quantity > 0),
                    DepartmentCode INTEGER NOT NULL,
                    FOREIGN KEY (Article) REFERENCES Products(Article),
                    FOREIGN KEY (DepartmentCode) REFERENCES Departments(DepartmentCode)
                )";
            using (var cmd = new SqliteCommand(createSales, connection))
                cmd.ExecuteNonQuery();

            var createIndex = @"
                CREATE INDEX IF NOT EXISTS idx_sales_date ON Sales(SaleDate);
                CREATE INDEX IF NOT EXISTS idx_sales_dept ON Sales(DepartmentCode);
                CREATE INDEX IF NOT EXISTS idx_sales_article ON Sales(Article);";
            using (var cmd = new SqliteCommand(createIndex, connection))
                cmd.ExecuteNonQuery();

            CheckAndInsertTestData(connection);
        }

        private void CheckAndInsertTestData(SqliteConnection connection)
        {
            using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Departments", connection);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0) return;

            var departments = new[]
            {
                (1, "Электроника", "Иванов Иван Иванович", "8-495-123-45-67", 150000m),
                (2, "Бытовая техника", "Петрова Анна Сергеевна", "8-812-456-78-90", 200000m),
                (3, "Компьютерная техника", "Сидоров Виктор Петрович", "8-383-222-33-44", 180000m),
                (4, "Аксессуары", "Козлова Елена Николаевна", "8-343-555-66-77", 80000m),
                (5, "Аудио/Видео", "Михайлов Дмитрий Алексеевич", "8-843-111-22-33", 120000m)
            };
            foreach (var (code, name, manager, phone, plan) in departments)
            {
                using var insertCmd = new SqliteCommand(
                    "INSERT INTO Departments (DepartmentCode, Name, Manager, Phone, DailyPlan) VALUES (@c, @n, @m, @p, @d)", connection);
                insertCmd.Parameters.AddWithValue("@c", code);
                insertCmd.Parameters.AddWithValue("@n", name);
                insertCmd.Parameters.AddWithValue("@m", manager);
                insertCmd.Parameters.AddWithValue("@p", phone);
                insertCmd.Parameters.AddWithValue("@d", plan);
                insertCmd.ExecuteNonQuery();
            }

            var products = new[]
            {
                (1001, "Смартфон Galaxy A", "шт", 25000m, 1),
                (1002, "Смартфон X100", "шт", 32000m, 1),
                (1003, "Ноутбук Pro 15", "шт", 65000m, 3),
                (1004, "Ноутбук Air 13", "шт", 48000m, 3),
                (1005, "Холодильник двухкамерный", "шт", 45000m, 2),
                (1006, "Микроволновая печь", "шт", 8000m, 2),
                (1007, "Мышь беспроводная", "шт", 1500m, 4),
                (1008, "Клавиатура механическая", "шт", 3500m, 4),
                (1009, "Телевизор 4K 55\"", "шт", 55000m, 1),
                (1010, "Наушники Bluetooth", "шт", 5000m, 5),
                (1011, "Планшет 10\"", "шт", 18000m, 3),
                (1012, "Колонка портативная", "шт", 3000m, 5)
            };
            foreach (var (art, name, unit, price, dept) in products)
            {
                using var insertCmd = new SqliteCommand(
                    "INSERT INTO Products (Article, Name, Unit, Price, DepartmentCode) VALUES (@a, @n, @u, @p, @d)", connection);
                insertCmd.Parameters.AddWithValue("@a", art);
                insertCmd.Parameters.AddWithValue("@n", name);
                insertCmd.Parameters.AddWithValue("@u", unit);
                insertCmd.Parameters.AddWithValue("@p", price);
                insertCmd.Parameters.AddWithValue("@d", dept);
                insertCmd.ExecuteNonQuery();
            }

            var sales = new[]
            {
                (1001, "2025-05-20", 2, 1),
                (1001, "2025-05-20", 1, 1),
                (1003, "2025-05-20", 1, 3),
                (1005, "2025-05-20", 1, 2),
                (1006, "2025-05-20", 3, 2),
                (1007, "2025-05-20", 5, 4),
                (1007, "2025-05-20", 3, 4),
                (1009, "2025-05-20", 1, 1),
                (1010, "2025-05-20", 4, 5),
                (1011, "2025-05-20", 2, 3),
                (1002, "2025-05-21", 2, 1),
                (1004, "2025-05-21", 1, 3),
                (1005, "2025-05-21", 1, 2),
                (1007, "2025-05-21", 6, 4),
                (1008, "2025-05-21", 3, 4),
                (1009, "2025-05-21", 1, 1),
                (1012, "2025-05-21", 5, 5),
                (1001, "2025-05-22", 3, 1),
                (1002, "2025-05-22", 1, 1),
                (1003, "2025-05-22", 2, 3),
                (1005, "2025-05-22", 2, 2),
                (1006, "2025-05-22", 2, 2),
                (1007, "2025-05-22", 4, 4),
                (1010, "2025-05-22", 3, 5),
                (1011, "2025-05-22", 1, 3),
                (1001, "2025-05-23", 2, 1),
                (1004, "2025-05-23", 1, 3),
                (1005, "2025-05-23", 1, 2),
                (1007, "2025-05-23", 5, 4),
                (1009, "2025-05-23", 2, 1),
                (1012, "2025-05-23", 4, 5),
                (1002, "2025-05-24", 2, 1),
                (1003, "2025-05-24", 1, 3),
                (1006, "2025-05-24", 3, 2),
                (1008, "2025-05-24", 4, 4),
                (1009, "2025-05-24", 1, 1),
                (1010, "2025-05-24", 5, 5),
                (1011, "2025-05-24", 2, 3),
                (1001, "2025-05-25", 2, 1),
                (1005, "2025-05-25", 2, 2),
                (1007, "2025-05-25", 4, 4),
                (1009, "2025-05-25", 1, 1),
                (1012, "2025-05-25", 3, 5),
                (1002, "2025-05-26", 1, 1),
                (1004, "2025-05-26", 2, 3),
                (1005, "2025-05-26", 1, 2),
                (1007, "2025-05-26", 5, 4),
                (1008, "2025-05-26", 3, 4),
                (1009, "2025-05-26", 1, 1),
                (1011, "2025-05-26", 1, 3),
                (1001, "2025-05-27", 3, 1),
                (1003, "2025-05-27", 1, 3),
                (1006, "2025-05-27", 2, 2),
                (1007, "2025-05-27", 6, 4),
                (1010, "2025-05-27", 4, 5),
                (1009, "2025-05-28", 2, 1),
                (1005, "2025-05-28", 1, 2),
                (1007, "2025-05-28", 5, 4),
                (1012, "2025-05-28", 4, 5),
                (1004, "2025-05-29", 2, 3),
                (1007, "2025-05-29", 4, 4),
                (1009, "2025-05-29", 1, 1),
                (1010, "2025-05-29", 3, 5)
            };
            foreach (var (art, date, qty, dept) in sales)
            {
                using var insertCmd = new SqliteCommand(
                    "INSERT INTO Sales (Article, SaleDate, Quantity, DepartmentCode) VALUES (@a, @d, @q, @dept)", connection);
                insertCmd.Parameters.AddWithValue("@a", art);
                insertCmd.Parameters.AddWithValue("@d", date);
                insertCmd.Parameters.AddWithValue("@q", qty);
                insertCmd.Parameters.AddWithValue("@dept", dept);
                insertCmd.ExecuteNonQuery();
            }
        }

        #region Departments
        public List<Department> GetDepartments()
        {
            var list = new List<Department>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Departments ORDER BY DepartmentCode", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Department
                {
                    DepartmentCode = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Manager = reader.GetString(2),
                    Phone = reader.GetString(3),
                    DailyPlan = reader.GetDecimal(4)
                });
            }
            return list;
        }

        public void AddDepartment(Department d)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Departments (DepartmentCode, Name, Manager, Phone, DailyPlan) VALUES (@c, @n, @m, @p, @d)", connection);
            cmd.Parameters.AddWithValue("@c", d.DepartmentCode);
            cmd.Parameters.AddWithValue("@n", d.Name);
            cmd.Parameters.AddWithValue("@m", d.Manager);
            cmd.Parameters.AddWithValue("@p", d.Phone);
            cmd.Parameters.AddWithValue("@d", d.DailyPlan);
            cmd.ExecuteNonQuery();
        }

        public void UpdateDepartment(Department d)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(
                "UPDATE Departments SET Name=@n, Manager=@m, Phone=@p, DailyPlan=@d WHERE DepartmentCode=@c", connection);
            cmd.Parameters.AddWithValue("@c", d.DepartmentCode);
            cmd.Parameters.AddWithValue("@n", d.Name);
            cmd.Parameters.AddWithValue("@m", d.Manager);
            cmd.Parameters.AddWithValue("@p", d.Phone);
            cmd.Parameters.AddWithValue("@d", d.DailyPlan);
            cmd.ExecuteNonQuery();
        }

        public void DeleteDepartment(int code)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("DELETE FROM Sales WHERE DepartmentCode=@c", connection);
            cmd.Parameters.AddWithValue("@c", code);
            cmd.ExecuteNonQuery();
            using var cmd2 = new SqliteCommand("DELETE FROM Products WHERE DepartmentCode=@c", connection);
            cmd2.Parameters.AddWithValue("@c", code);
            cmd2.ExecuteNonQuery();
            using var cmd3 = new SqliteCommand("DELETE FROM Departments WHERE DepartmentCode=@c", connection);
            cmd3.Parameters.AddWithValue("@c", code);
            cmd3.ExecuteNonQuery();
        }
        #endregion

        #region Products
        public List<Product> GetProducts()
        {
            var list = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Products ORDER BY Article", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Product
                {
                    Article = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Unit = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    DepartmentCode = reader.GetInt32(4)
                });
            }
            return list;
        }

        public Product? GetProduct(int article)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Products WHERE Article=@a", connection);
            cmd.Parameters.AddWithValue("@a", article);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Product
                {
                    Article = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Unit = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    DepartmentCode = reader.GetInt32(4)
                };
            }
            return null;
        }

        public void AddProduct(Product p)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Products (Article, Name, Unit, Price, DepartmentCode) VALUES (@a, @n, @u, @p, @d)", connection);
            cmd.Parameters.AddWithValue("@a", p.Article);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@d", p.DepartmentCode);
            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(Product p)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(
                "UPDATE Products SET Name=@n, Unit=@u, Price=@p, DepartmentCode=@d WHERE Article=@a", connection);
            cmd.Parameters.AddWithValue("@a", p.Article);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@d", p.DepartmentCode);
            cmd.ExecuteNonQuery();
        }

        public void DeleteProduct(int article)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("DELETE FROM Sales WHERE Article=@a", connection);
            cmd.Parameters.AddWithValue("@a", article);
            cmd.ExecuteNonQuery();
            using var cmd2 = new SqliteCommand("DELETE FROM Products WHERE Article=@a", connection);
            cmd2.Parameters.AddWithValue("@a", article);
            cmd2.ExecuteNonQuery();
        }
        #endregion

        #region Sales
        public List<Sale> GetSales()
        {
            var list = new List<Sale>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                SELECT s.SaleId, s.Article, s.SaleDate, s.Quantity, s.DepartmentCode, p.Price
                FROM Sales s
                JOIN Products p ON s.Article = p.Article
                ORDER BY s.SaleDate DESC, s.SaleId", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Sale
                {
                    SaleId = reader.GetInt32(0),
                    Article = reader.GetInt32(1),
                    SaleDate = DateTime.Parse(reader.GetString(2)),
                    Quantity = reader.GetInt32(3),
                    DepartmentCode = reader.GetInt32(4),
                    Price = reader.GetDecimal(5)
                });
            }
            return list;
        }

        public void AddSale(Sale s)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var price = GetProduct(s.Article)?.Price ?? 0;
            using var cmd = new SqliteCommand(
                "INSERT INTO Sales (Article, SaleDate, Quantity, DepartmentCode) VALUES (@a, @d, @q, @dept)", connection);
            cmd.Parameters.AddWithValue("@a", s.Article);
            cmd.Parameters.AddWithValue("@d", s.SaleDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@q", s.Quantity);
            cmd.Parameters.AddWithValue("@dept", s.DepartmentCode);
            cmd.ExecuteNonQuery();
        }

        public void UpdateSale(Sale s)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(
                "UPDATE Sales SET Article=@a, SaleDate=@d, Quantity=@q, DepartmentCode=@dept WHERE SaleId=@id", connection);
            cmd.Parameters.AddWithValue("@id", s.SaleId);
            cmd.Parameters.AddWithValue("@a", s.Article);
            cmd.Parameters.AddWithValue("@d", s.SaleDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@q", s.Quantity);
            cmd.Parameters.AddWithValue("@dept", s.DepartmentCode);
            cmd.ExecuteNonQuery();
        }

        public void DeleteSale(int saleId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("DELETE FROM Sales WHERE SaleId=@id", connection);
            cmd.Parameters.AddWithValue("@id", saleId);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Reports
        public List<DepartmentSummaryReport> GetDepartmentSummary(DateTime? from = null, DateTime? to = null)
        {
            var list = new List<DepartmentSummaryReport>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var sql = @"
                SELECT d.Name, d.Manager, d.Phone, d.DailyPlan,
                       COALESCE(SUM(s.Quantity * p.Price), 0) as Actual
                FROM Departments d
                LEFT JOIN Sales s ON d.DepartmentCode = s.DepartmentCode
                    AND (@from IS NULL OR s.SaleDate >= @from)
                    AND (@to IS NULL OR s.SaleDate <= @to)
                LEFT JOIN Products p ON s.Article = p.Article
                GROUP BY d.DepartmentCode, d.Name, d.Manager, d.Phone, d.DailyPlan
                ORDER BY d.DepartmentCode";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new DepartmentSummaryReport
                {
                    DepartmentName = reader.GetString(0),
                    Manager = reader.GetString(1),
                    Phone = reader.GetString(2),
                    DailyPlan = reader.GetDecimal(3),
                    ActualVolume = reader.GetDecimal(4)
                });
            }
            return list;
        }

        public List<ProductSummaryReport> GetProductSummary(DateTime? from = null, DateTime? to = null)
        {
            var list = new List<ProductSummaryReport>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var sql = @"
                SELECT p.Article, p.Name, p.Unit, p.Price,
                       COALESCE(SUM(s.Quantity), 0) as TotalQty,
                       COALESCE(SUM(s.Quantity * p.Price), 0) as TotalRev
                FROM Products p
                LEFT JOIN Sales s ON p.Article = s.Article
                    AND (@from IS NULL OR s.SaleDate >= @from)
                    AND (@to IS NULL OR s.SaleDate <= @to)
                GROUP BY p.Article, p.Name, p.Unit, p.Price
                ORDER BY p.Article";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var totalQty = reader.GetInt32(4);
                list.Add(new ProductSummaryReport
                {
                    Article = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Unit = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    TotalQuantity = totalQty,
                    TotalRevenue = reader.GetDecimal(5),
                    AvgDailyQuantity = totalQty / 10.0m // 10 дней в данных
                });
            }
            return list;
        }

        public List<ProductRatingReport> GetProductRating(DateTime? from = null, DateTime? to = null)
        {
            var list = new List<ProductRatingReport>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var sql = @"
                SELECT p.Article, p.Name, COALESCE(SUM(s.Quantity * p.Price), 0) as TotalRev
                FROM Products p
                LEFT JOIN Sales s ON p.Article = s.Article
                    AND (@from IS NULL OR s.SaleDate >= @from)
                    AND (@to IS NULL OR s.SaleDate <= @to)
                GROUP BY p.Article, p.Name
                HAVING TotalRev > 0
                ORDER BY TotalRev DESC
                LIMIT 10";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            using var reader = cmd.ExecuteReader();
            var total = 0m;
            var rows = new List<(int, string, decimal)>();
            while (reader.Read())
            {
                var rev = reader.GetDecimal(2);
                total += rev;
                rows.Add((reader.GetInt32(0), reader.GetString(1), rev));
            }
            foreach (var (art, name, rev) in rows)
            {
                list.Add(new ProductRatingReport
                {
                    Article = art,
                    ProductName = name,
                    TotalRevenue = rev,
                    SharePercent = total > 0 ? rev / total * 100 : 0
                });
            }
            return list;
        }

        public List<DailySalesReport> GetDailySales(DateTime? from = null, DateTime? to = null)
        {
            var list = new List<DailySalesReport>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var sql = @"
                SELECT s.SaleDate, COUNT(*) as SalesCount, SUM(s.Quantity * p.Price) as TotalRev,
                       (SELECT p2.Name FROM Sales s2 JOIN Products p2 ON s2.Article = p2.Article
                        WHERE s2.SaleDate = s.SaleDate
                        GROUP BY p2.Name ORDER BY SUM(s2.Quantity * p2.Price) DESC LIMIT 1) as TopProduct
                FROM Sales s
                JOIN Products p ON s.Article = p.Article
                WHERE (@from IS NULL OR s.SaleDate >= @from)
                  AND (@to IS NULL OR s.SaleDate <= @to)
                GROUP BY s.SaleDate
                ORDER BY s.SaleDate";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new DailySalesReport
                {
                    Date = DateTime.Parse(reader.GetString(0)),
                    SalesCount = reader.GetInt32(1),
                    TotalRevenue = reader.GetDecimal(2),
                    TopProductName = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
            return list;
        }

        public List<UnderperformersReport> GetUnderperformers(DateTime? from = null, DateTime? to = null)
        {
            var list = new List<UnderperformersReport>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var sql = @"
                SELECT p.Article, p.Name, p.Unit, p.Price, p.DepartmentCode
                FROM Products p
                WHERE NOT EXISTS (
                    SELECT 1 FROM Sales s
                    WHERE s.Article = p.Article
                    AND (@from IS NULL OR s.SaleDate >= @from)
                    AND (@to IS NULL OR s.SaleDate <= @to)
                )";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UnderperformersReport
                {
                    Article = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Unit = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    DepartmentCode = reader.GetInt32(4)
                });
            }
            return list;
        }
        #endregion

        #region Charts data
        public List<(string DeptName, decimal Revenue)> GetSalesByDepartment()
        {
            var list = new List<(string, decimal)>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                SELECT d.Name, COALESCE(SUM(s.Quantity * p.Price), 0)
                FROM Departments d
                LEFT JOIN Sales s ON d.DepartmentCode = s.DepartmentCode
                LEFT JOIN Products p ON s.Article = p.Article
                GROUP BY d.DepartmentCode
                ORDER BY d.DepartmentCode", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add((reader.GetString(0), reader.GetDecimal(1)));
            return list;
        }

        public List<(string ProductName, decimal Revenue)> GetSalesByProduct()
        {
            var list = new List<(string, decimal)>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                SELECT p.Name, COALESCE(SUM(s.Quantity * p.Price), 0)
                FROM Products p
                LEFT JOIN Sales s ON p.Article = s.Article
                GROUP BY p.Article
                ORDER BY p.Article", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add((reader.GetString(0), reader.GetDecimal(1)));
            return list;
        }

        public List<(string Date, decimal Revenue)> GetDailyRevenue()
        {
            var list = new List<(string, decimal)>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                SELECT SaleDate, SUM(Quantity * p.Price)
                FROM Sales s JOIN Products p ON s.Article = p.Article
                GROUP BY SaleDate ORDER BY SaleDate", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add((reader.GetString(0), reader.GetDecimal(1)));
            return list;
        }

        public List<(string Category, int Count)> GetPriceDistribution()
        {
            var list = new List<(string, int)>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                SELECT
                    CASE
                        WHEN Price < 5000 THEN 'До 5 000 ₽'
                        WHEN Price < 10000 THEN '5 000 – 10 000 ₽'
                        WHEN Price < 50000 THEN '10 000 – 50 000 ₽'
                        ELSE 'Более 50 000 ₽'
                    END as Category,
                    COUNT(*) as Cnt
                FROM Products
                GROUP BY Category
                ORDER BY MIN(Price)", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add((reader.GetString(0), reader.GetInt32(1)));
            return list;
        }
        #endregion

        public void ClearAllData()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd1 = new SqliteCommand("DELETE FROM Sales", connection);
            cmd1.ExecuteNonQuery();
            using var cmd2 = new SqliteCommand("DELETE FROM Products", connection);
            cmd2.ExecuteNonQuery();
            using var cmd3 = new SqliteCommand("DELETE FROM Departments", connection);
            cmd3.ExecuteNonQuery();
        }
    }
}
