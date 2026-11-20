using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using SupplyTrackingSystem.Models.Reports;
using SupplyTrackingSystem.Services;

namespace SupplyTrackingSystem.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly ExportService _exportService;
        private int _selectedReportIndex;
        private DateTime? _filterDateFrom;
        private DateTime? _filterDateTo;
        private int _filterDepartmentCode;

        private ObservableCollection<DepartmentSummaryReport> _departmentSummary = new();
        private ObservableCollection<ProductSummaryReport> _productSummary = new();
        private ObservableCollection<ProductRatingReport> _productRating = new();
        private ObservableCollection<DailySalesReport> _dailySales = new();
        private ObservableCollection<UnderperformersReport> _underperformers = new();

        public ReportsViewModel()
        {
            _db = DatabaseService.Instance;
            _exportService = new ExportService();
            LoadReportsCommand = ReactiveCommand.Create(LoadReports);
            ExportExcelCommand = ReactiveCommand.CreateFromTask(ExportToExcel);
            ExportCsvCommand = ReactiveCommand.CreateFromTask(ExportToCsv);
            ExportPdfCommand = ReactiveCommand.CreateFromTask(ExportToPdf);
            ApplyFiltersCommand = ReactiveCommand.Create(LoadReports);
            ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);
            LoadReports();
        }

        public int SelectedReportIndex
        {
            get => _selectedReportIndex;
            set { this.RaiseAndSetIfChanged(ref _selectedReportIndex, value); LoadReports(); }
        }

        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set => this.RaiseAndSetIfChanged(ref _filterDateFrom, value);
        }

        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set => this.RaiseAndSetIfChanged(ref _filterDateTo, value);
        }

        public int FilterDepartmentCode
        {
            get => _filterDepartmentCode;
            set => this.RaiseAndSetIfChanged(ref _filterDepartmentCode, value);
        }

        public ObservableCollection<DepartmentSummaryReport> DepartmentSummary
        {
            get => _departmentSummary;
            set => this.RaiseAndSetIfChanged(ref _departmentSummary, value);
        }

        public ObservableCollection<ProductSummaryReport> ProductSummary
        {
            get => _productSummary;
            set => this.RaiseAndSetIfChanged(ref _productSummary, value);
        }

        public ObservableCollection<ProductRatingReport> ProductRating
        {
            get => _productRating;
            set => this.RaiseAndSetIfChanged(ref _productRating, value);
        }

        public ObservableCollection<DailySalesReport> DailySales
        {
            get => _dailySales;
            set => this.RaiseAndSetIfChanged(ref _dailySales, value);
        }

        public ObservableCollection<UnderperformersReport> Underperformers
        {
            get => _underperformers;
            set => this.RaiseAndSetIfChanged(ref _underperformers, value);
        }

        public string[] ReportNames => new[]
        {
            "1. Сводка по отделам",
            "2. Сводка по товарам",
            "3. Рейтинг товаров",
            "4. Дневная динамика",
            "5. Товары-аутсайдеры"
        };

        public ReactiveCommand<Unit, Unit> LoadReportsCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportExcelCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCsvCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportPdfCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }

        private void LoadReports()
        {
            DepartmentSummary = new ObservableCollection<DepartmentSummaryReport>(_db.GetDepartmentSummary(FilterDateFrom, FilterDateTo));
            ProductSummary = new ObservableCollection<ProductSummaryReport>(_db.GetProductSummary(FilterDateFrom, FilterDateTo));
            ProductRating = new ObservableCollection<ProductRatingReport>(_db.GetProductRating(FilterDateFrom, FilterDateTo));
            DailySales = new ObservableCollection<DailySalesReport>(_db.GetDailySales(FilterDateFrom, FilterDateTo));
            Underperformers = new ObservableCollection<UnderperformersReport>(_db.GetUnderperformers(FilterDateFrom, FilterDateTo));
        }

        private void ClearFilters()
        {
            FilterDateFrom = null;
            FilterDateTo = null;
            FilterDepartmentCode = 0;
            LoadReports();
        }

        private async Task ExportToExcel()
        {
            var data = GetCurrentData();
            if (data == null) return;
            var path = await GetSavePath("xlsx");
            if (string.IsNullOrEmpty(path)) return;
            _exportService.ExportToExcel((IEnumerable<object>)data, path, ReportNames[SelectedReportIndex]);
            await ShowMessage("Экспорт завершён", $"Файл сохранён: {path}");
        }

        private async Task ExportToCsv()
        {
            var data = GetCurrentData();
            if (data == null) return;
            var path = await GetSavePath("csv");
            if (string.IsNullOrEmpty(path)) return;
            _exportService.ExportToCsv((IEnumerable<object>)data, path);
            await ShowMessage("Экспорт завершён", $"Файл сохранён: {path}");
        }

        private async Task ExportToPdf()
        {
            var data = GetCurrentData();
            if (data == null) return;
            var path = await GetSavePath("pdf");
            if (string.IsNullOrEmpty(path)) return;
            _exportService.ExportToPdf((IEnumerable<object>)data, path, ReportNames[SelectedReportIndex]);
            await ShowMessage("Экспорт завершён", $"Файл сохранён: {path}");
        }

        private System.Collections.IEnumerable? GetCurrentData()
        {
            return SelectedReportIndex switch
            {
                0 => DepartmentSummary,
                1 => ProductSummary,
                2 => ProductRating,
                3 => DailySales,
                4 => Underperformers,
                _ => null
            };
        }

        private async Task<string> GetSavePath(string ext)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var top = desktop.MainWindow;
                if (top == null) return "";
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

        private async Task ShowMessage(string title, string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok);
            await box.ShowAsync();
        }
    }
}
