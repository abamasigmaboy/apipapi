using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using SupplyTrackingSystem.Models;
using SupplyTrackingSystem.Services;

namespace SupplyTrackingSystem.Services
{
    public class ImportResult
    {
        public int Added { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportService
    {
        private readonly DatabaseService _db;
        public ImportService() => _db = DatabaseService.Instance;

        public ImportResult ImportDepartmentsFromCsv(string filePath)
        {
            var result = new ImportResult();
            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," };
            using var csv = new CsvReader(reader, config);
            csv.Read(); csv.ReadHeader();
            while (csv.Read())
            {
                try
                {
                    var code = csv.GetField<int>("код_отдела");
                    var name = csv.GetField("наименование_отдела");
                    var manager = csv.GetField("заведующий");
                    var phone = csv.GetField("телефон");
                    var plan = csv.GetField<decimal>("план_руб_день");
                    if (plan < 0) { result.Skipped++; result.Errors.Add($"Отдел {code}: план < 0"); continue; }
                    _db.AddDepartment(new Department { DepartmentCode = code, Name = name, Manager = manager, Phone = phone, DailyPlan = plan });
                    result.Added++;
                }
                catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка: {ex.Message}"); }
            }
            return result;
        }

        public ImportResult ImportProductsFromCsv(string filePath)
        {
            var result = new ImportResult();
            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," };
            using var csv = new CsvReader(reader, config);
            csv.Read(); csv.ReadHeader();
            while (csv.Read())
            {
                try
                {
                    var art = csv.GetField<int>("артикул");
                    var name = csv.GetField("наименование");
                    var unit = csv.GetField("ед_измерения");
                    var price = csv.GetField<decimal>("цена_руб");
                    var dept = csv.GetField<int>("код_отдела");
                    if (price < 0) { result.Skipped++; result.Errors.Add($"Товар {art}: цена < 0"); continue; }
                    var existing = _db.GetDepartments().Any(d => d.DepartmentCode == dept);
                    if (!existing) { result.Skipped++; result.Errors.Add($"Товар {art}: отдел {dept} не найден"); continue; }
                    _db.AddProduct(new Product { Article = art, Name = name, Unit = unit, Price = price, DepartmentCode = dept });
                    result.Added++;
                }
                catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка: {ex.Message}"); }
            }
            return result;
        }

        public ImportResult ImportSalesFromExcel(string filePath)
        {
            var result = new ImportResult();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets["Продажи"];
            if (ws == null) { result.Errors.Add("Лист 'Продажи' не найден"); return result; }
            var rows = ws.Dimension?.Rows ?? 0;
            for (int r = 2; r <= rows; r++)
            {
                try
                {
                    var art = int.Parse(ws.Cells[r, 1].Text);
                    var dateStr = ws.Cells[r, 2].Text;
                    var qty = int.Parse(ws.Cells[r, 3].Text);
                    var dept = int.Parse(ws.Cells[r, 4].Text);
                    if (!DateTime.TryParse(dateStr, out var date)) { result.Skipped++; result.Errors.Add($"Строка {r}: неверная дата"); continue; }
                    if (qty <= 0) { result.Skipped++; result.Errors.Add($"Строка {r}: количество <= 0"); continue; }
                    if (_db.GetProduct(art) == null) { result.Skipped++; result.Errors.Add($"Строка {r}: товар {art} не найден"); continue; }
                    if (!_db.GetDepartments().Any(d => d.DepartmentCode == dept)) { result.Skipped++; result.Errors.Add($"Строка {r}: отдел {dept} не найден"); continue; }
                    _db.AddSale(new Sale { Article = art, SaleDate = date, Quantity = qty, DepartmentCode = dept });
                    result.Added++;
                }
                catch (Exception ex) { result.Skipped++; result.Errors.Add($"Строка {r}: {ex.Message}"); }
            }
            return result;
        }

        public ImportResult ImportFromJson(string filePath)
        {
            var result = new ImportResult();
            var json = File.ReadAllText(filePath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("отделы", out var depts))
            {
                foreach (var item in depts.EnumerateArray())
                {
                    try
                    {
                        var code = item.GetProperty("код_отдела").GetInt32();
                        var name = item.GetProperty("наименование_отдела").GetString() ?? "";
                        var manager = item.GetProperty("заведующий").GetString() ?? "";
                        var phone = item.GetProperty("телефон").GetString() ?? "";
                        var plan = item.GetProperty("план_руб_день").GetDecimal();
                        if (plan < 0) { result.Skipped++; continue; }
                        _db.AddDepartment(new Department { DepartmentCode = code, Name = name, Manager = manager, Phone = phone, DailyPlan = plan });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка отдела: {ex.Message}"); }
                }
            }
            if (root.TryGetProperty("товары", out var prods))
            {
                foreach (var item in prods.EnumerateArray())
                {
                    try
                    {
                        var art = item.GetProperty("артикул").GetInt32();
                        var name = item.GetProperty("наименование").GetString() ?? "";
                        var unit = item.GetProperty("ед_измерения").GetString() ?? "";
                        var price = item.GetProperty("цена_руб").GetDecimal();
                        var dept = item.GetProperty("код_отдела").GetInt32();
                        if (price < 0) { result.Skipped++; continue; }
                        _db.AddProduct(new Product { Article = art, Name = name, Unit = unit, Price = price, DepartmentCode = dept });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка товара: {ex.Message}"); }
                }
            }
            if (root.TryGetProperty("продажи", out var sales))
            {
                foreach (var item in sales.EnumerateArray())
                {
                    try
                    {
                        var art = item.GetProperty("артикул_товара").GetInt32();
                        var dateStr = item.GetProperty("дата_продажи").GetString() ?? "";
                        var qty = item.GetProperty("количество").GetInt32();
                        var dept = item.GetProperty("код_отдела").GetInt32();
                        if (!DateTime.TryParse(dateStr, out var date)) { result.Skipped++; continue; }
                        if (qty <= 0) { result.Skipped++; continue; }
                        _db.AddSale(new Sale { Article = art, SaleDate = date, Quantity = qty, DepartmentCode = dept });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка продажи: {ex.Message}"); }
                }
            }
            return result;
        }

        public ImportResult ImportFromXml(string filePath)
        {
            var result = new ImportResult();
            var doc = XDocument.Load(filePath);
            var depts = doc.Root?.Element("отделы")?.Elements("отдел");
            if (depts != null)
            {
                foreach (var item in depts)
                {
                    try
                    {
                        var code = int.Parse(item.Element("код_отдела")?.Value ?? "0");
                        var name = item.Element("наименование_отдела")?.Value ?? "";
                        var manager = item.Element("заведующий")?.Value ?? "";
                        var phone = item.Element("телефон")?.Value ?? "";
                        var plan = decimal.Parse(item.Element("план_руб_день")?.Value ?? "0", CultureInfo.InvariantCulture);
                        if (plan < 0) { result.Skipped++; continue; }
                        _db.AddDepartment(new Department { DepartmentCode = code, Name = name, Manager = manager, Phone = phone, DailyPlan = plan });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка отдела XML: {ex.Message}"); }
                }
            }
            var prods = doc.Root?.Element("товары")?.Elements("товар");
            if (prods != null)
            {
                foreach (var item in prods)
                {
                    try
                    {
                        var art = int.Parse(item.Element("артикул")?.Value ?? "0");
                        var name = item.Element("наименование")?.Value ?? "";
                        var unit = item.Element("ед_измерения")?.Value ?? "";
                        var price = decimal.Parse(item.Element("цена_руб")?.Value ?? "0", CultureInfo.InvariantCulture);
                        var dept = int.Parse(item.Element("код_отдела")?.Value ?? "0");
                        if (price < 0) { result.Skipped++; continue; }
                        _db.AddProduct(new Product { Article = art, Name = name, Unit = unit, Price = price, DepartmentCode = dept });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка товара XML: {ex.Message}"); }
                }
            }
            var sales = doc.Root?.Element("продажи")?.Elements("продажа");
            if (sales != null)
            {
                foreach (var item in sales)
                {
                    try
                    {
                        var art = int.Parse(item.Element("артикул_товара")?.Value ?? "0");
                        var dateStr = item.Element("дата_продажи")?.Value ?? "";
                        var qty = int.Parse(item.Element("количество")?.Value ?? "0");
                        var dept = int.Parse(item.Element("код_отдела")?.Value ?? "0");
                        if (!DateTime.TryParse(dateStr, out var date)) { result.Skipped++; continue; }
                        if (qty <= 0) { result.Skipped++; continue; }
                        _db.AddSale(new Sale { Article = art, SaleDate = date, Quantity = qty, DepartmentCode = dept });
                        result.Added++;
                    }
                    catch (Exception ex) { result.Skipped++; result.Errors.Add($"Ошибка продажи XML: {ex.Message}"); }
                }
            }
            return result;
        }
    }
}
