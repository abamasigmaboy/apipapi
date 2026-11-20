using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using SupplyTrackingSystem.Services;

namespace SupplyTrackingSystem.ViewModels
{
    public class ImportExportViewModel : ViewModelBase
    {
        private readonly ImportService _importService;
        private readonly ExportService _exportService;
        private readonly DatabaseService _db;
        private string _importResult = "";
        private int _selectedImportFormat;
        private int _selectedExportFormat;
        private int _selectedExportReport;

        public ImportExportViewModel()
        {
            _importService = new ImportService();
            _exportService = new ExportService();
            _db = DatabaseService.Instance;
            ImportCommand = ReactiveCommand.CreateFromTask(ImportAsync);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync);
            ClearDataCommand = ReactiveCommand.CreateFromTask(ClearDataAsync);
        }

        public string ImportResult
        {
            get => _importResult;
            set => this.RaiseAndSetIfChanged(ref _importResult, value);
        }

        public int SelectedImportFormat
        {
            get => _selectedImportFormat;
            set => this.RaiseAndSetIfChanged(ref _selectedImportFormat, value);
        }

        public int SelectedExportFormat
        {
            get => _selectedExportFormat;
            set => this.RaiseAndSetIfChanged(ref _selectedExportFormat, value);
        }

        public int SelectedExportReport
        {
            get => _selectedExportReport;
            set => this.RaiseAndSetIfChanged(ref _selectedExportReport, value);
        }

        public string[] ImportFormats => new[] { "CSV (Отделы)", "CSV (Товары)", "Excel (Продажи)", "JSON (полные данные)", "XML (полные данные)" };
        public string[] ExportFormats => new[] { "Excel", "CSV", "PDF" };
        public string[] ExportReports => new[] { "Сводка по отделам", "Сводка по товарам", "Рейтинг товаров", "Дневная динамика", "Товары-аутсайдеры" };

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearDataCommand { get; }

        private async Task ImportAsync()
        {
            try
            {
                var path = await GetOpenFilePath();
                if (string.IsNullOrEmpty(path)) return;
                ImportResult result;
                switch (SelectedImportFormat)
                {
                    case 0: result = _importService.ImportDepartmentsFromCsv(path); break;
                    case 1: result = _importService.ImportProductsFromCsv(path); break;
                    case 2: result = _importService.ImportSalesFromExcel(path); break;
                    case 3: result = _importService.ImportFromJson(path); break;
                    case 4: result = _importService.ImportFromXml(path); break;
                    default: return;
                }
                ImportResult = $"Добавлено: {result.Added}\nПропущено: {result.Skipped}\n" +
                    (result.Errors.Count > 0 ? $"Ошибки:\n{string.Join("\n", result.Errors.Take(10))}" : "");
                var box = MessageBoxManager.GetMessageBoxStandard("Импорт", "Импорт завершён!", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message, ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

        private async Task ExportAsync()
        {
            try
            {
                var path = await GetSaveFilePath();
                if (string.IsNullOrEmpty(path)) return;
                object? data = SelectedExportReport switch
                {
                    0 => _db.GetDepartmentSummary(),
                    1 => _db.GetProductSummary(),
                    2 => _db.GetProductRating(),
                    3 => _db.GetDailySales(),
                    4 => _db.GetUnderperformers(),
                    _ => null
                };
                if (data == null) return;
                switch (SelectedExportFormat)
                {
                    case 0: _exportService.ExportToExcel((System.Collections.IEnumerable)data, path, ExportReports[SelectedExportReport]); break;
                    case 1: _exportService.ExportToCsv((System.Collections.IEnumerable)data, path); break;
                    case 2: _exportService.ExportToPdf((System.Collections.IEnumerable)data, path, ExportReports[SelectedExportReport]); break;
                }
                var box = MessageBoxManager.GetMessageBoxStandard("Экспорт", "Экспорт завершён!", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message, ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

        private async Task ClearDataAsync()
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Удалить все данные?", ButtonEnum.YesNo);
            var result = await box.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                _db.ClearAllData();
                var msg = MessageBoxManager.GetMessageBoxStandard("Готово", "Данные очищены", ButtonEnum.Ok);
                await msg.ShowAsync();
            }
        }

        private async Task<string> GetOpenFilePath()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var top = desktop.MainWindow;
                if (top == null) return "";
                var options = new FilePickerOpenOptions
                {
                    Title = "Выберите файл",
                    AllowMultiple = false
                };
                var result = await top.StorageProvider.OpenFilePickerAsync(options);
                return result.FirstOrDefault()?.Path.LocalPath ?? "";
            }
            return "";
        }

        private async Task<string> GetSaveFilePath()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var top = desktop.MainWindow;
                if (top == null) return "";
                var ext = SelectedExportFormat switch { 0 => "xlsx", 1 => "csv", 2 => "pdf", _ => "txt" };
                var options = new FilePickerSaveOptions
                {
                    Title = "Сохранить отчёт",
                    DefaultExtension = ext,
                    SuggestedFileName = $"Отчёт.{ext}"
                };
                var result = await top.StorageProvider.SaveFilePickerAsync(options);
                return result?.Path.LocalPath ?? "";
            }
            return "";
        }
    }
}
