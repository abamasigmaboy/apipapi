using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SkiaSharp;

namespace SupplyTrackingSystem.Services
{
    public class ExportService
    {
        public void ExportToExcel(System.Collections.IEnumerable data, string filePath, string sheetName = "Отчет")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            var list = new List<object>();
            foreach (var item in data) list.Add(item);
            if (list.Count == 0) return;
            var properties = list[0].GetType().GetProperties()
                .Where(p => p.CanRead && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(int)))
                .ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }
            int row = 2;
            foreach (var item in list)
            {
                for (int i = 0; i < properties.Count; i++)
                {
                    var value = properties[i].GetValue(item);
                    worksheet.Cells[row, i + 1].Value = value;
                    if (value is decimal || value is double)
                        worksheet.Cells[row, i + 1].Style.Numberformat.Format = "#,##0.00";
                }
                row++;
            }
            worksheet.Cells.AutoFitColumns();
            package.SaveAs(new FileInfo(filePath));
        }

        public void ExportToCsv(System.Collections.IEnumerable data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = System.Text.Encoding.UTF8 };
            using var csv = new CsvWriter(writer, config);
            var list = new List<object>();
            foreach (var item in data) list.Add(item);
            if (list.Count == 0) return;
            var properties = list[0].GetType().GetProperties()
                .Where(p => p.CanRead && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(int)))
                .ToList();
            foreach (var prop in properties)
                csv.WriteField(prop.Name);
            csv.NextRecord();
            foreach (var item in list)
            {
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    if (value != null)
                        csv.WriteField(value);
                    else
                        csv.WriteField("");
                }
                csv.NextRecord();
            }
        }

        public void ExportToPdf(System.Collections.IEnumerable data, string filePath, string title, string? chartImagePath = null)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            var boldFont = new XFont("Arial", 10, XFontStyleEx.Bold);
            var titleFont = new XFont("Arial", 14, XFontStyleEx.Bold);
            gfx.DrawString(title, titleFont, XBrushes.Black, new XRect(0, 20, page.Width.Point, 30), XStringFormats.TopCenter);
            var list = new List<object>();
            foreach (var item in data) list.Add(item);
            var properties = list.Count > 0 ? list[0].GetType().GetProperties()
                .Where(p => p.CanRead && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(int)))
                .ToList() : new List<System.Reflection.PropertyInfo>();
            double y = 60;
            double colWidth = properties.Count > 0 ? (page.Width.Point - 80) / properties.Count : 100;
            double x = 40;
            foreach (var prop in properties)
            {
                gfx.DrawString(prop.Name, boldFont, XBrushes.Black, new XRect(x, y, colWidth, 20), XStringFormats.TopLeft);
                x += colWidth;
            }
            y += 20;
            foreach (var item in list)
            {
                x = 40;
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? "";
                    gfx.DrawString(value, font, XBrushes.Black, new XRect(x, y, colWidth, 15), XStringFormats.TopLeft);
                    x += colWidth;
                }
                y += 15;
                if (y > page.Height.Point - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }
            if (!string.IsNullOrEmpty(chartImagePath) && File.Exists(chartImagePath))
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                using var image = XImage.FromFile(chartImagePath);
                double imgWidth = page.Width.Point - 80;
                double imgHeight = image.PixelHeight * imgWidth / image.PixelWidth;
                gfx.DrawImage(image, 40, 40, imgWidth, imgHeight);
            }
            document.Save(filePath);
        }

        public void SaveChartToPng(string filePath, SKSurface surface)
        {
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(filePath, data.ToArray());
        }
    }
}
