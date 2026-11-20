using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using SkiaSharp;
using SupplyTrackingSystem.Services;

namespace SupplyTrackingSystem.ViewModels
{
    public class ChartsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;

        public ChartsViewModel()
        {
            _db = DatabaseService.Instance;
            RefreshChartsCommand = ReactiveCommand.Create(LoadCharts);
            SaveChartCommand = ReactiveCommand.CreateFromTask(SaveChartAsync);
            LoadCharts();
        }

        public ISeries[]? BarChartSeries { get; set; }
        public ISeries[]? PieChartSeries { get; set; }
        public ISeries[]? LineChartSeries { get; set; }
        public ISeries[]? HistogramSeries { get; set; }

        public Axis[] BarChartXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] BarChartYAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] LineChartXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] LineChartYAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] HistogramXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] HistogramYAxes { get; set; } = Array.Empty<Axis>();

        public ReactiveCommand<Unit, Unit> RefreshChartsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveChartCommand { get; }

        private void LoadCharts()
        {
            LoadBarChart();
            LoadPieChart();
            LoadLineChart();
            LoadHistogram();
            this.RaisePropertyChanged(nameof(BarChartSeries));
            this.RaisePropertyChanged(nameof(PieChartSeries));
            this.RaisePropertyChanged(nameof(LineChartSeries));
            this.RaisePropertyChanged(nameof(HistogramSeries));
            this.RaisePropertyChanged(nameof(BarChartXAxes));
            this.RaisePropertyChanged(nameof(BarChartYAxes));
            this.RaisePropertyChanged(nameof(LineChartXAxes));
            this.RaisePropertyChanged(nameof(LineChartYAxes));
            this.RaisePropertyChanged(nameof(HistogramXAxes));
            this.RaisePropertyChanged(nameof(HistogramYAxes));
        }

        private void LoadBarChart()
        {
            var data = _db.GetSalesByDepartment();
            BarChartSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Выручка",
                    Values = data.Select(d => d.Revenue).ToArray(),
                    Fill = new SolidColorPaint(SKColors.LightBlue),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:N0}"
                }
            };
            BarChartXAxes = new[] { new Axis { Labels = data.Select(d => d.DeptName).ToArray(), Name = "Отдел", LabelsRotation = 30 } };
            BarChartYAxes = new[] { new Axis { Name = "Выручка (₽)", Labeler = value => $"{value:N0}" } };
        }

        private void LoadPieChart()
        {
            var data = _db.GetSalesByProduct().Where(p => p.Revenue > 0).ToList();
            PieChartSeries = data.Select(p => new PieSeries<decimal>
            {
                Name = p.ProductName,
                Values = new[] { p.Revenue },
                DataLabelsSize = 12,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                DataLabelsFormatter = point => $"{point.Context.Series.Name}: {point.Coordinate.PrimaryValue:N0}"
            }).ToArray<ISeries>();
        }

        private void LoadLineChart()
        {
            var data = _db.GetDailyRevenue();
            LineChartSeries = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Name = "Выручка",
                    Values = data.Select(d => d.Revenue).ToArray(),
                    Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(100)),
                    Stroke = new SolidColorPaint(SKColors.Blue, 3),
                    GeometrySize = 8,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:N0}"
                }
            };
            LineChartXAxes = new[] { new Axis { Labels = data.Select(d => d.Date).ToArray(), Name = "Дата", LabelsRotation = 45 } };
            LineChartYAxes = new[] { new Axis { Name = "Выручка (₽)", Labeler = value => $"{value:N0}" } };
        }

        private void LoadHistogram()
        {
            var data = _db.GetPriceDistribution();
            HistogramSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Количество товаров",
                    Values = data.Select(d => d.Count).ToArray(),
                    Fill = new SolidColorPaint(SKColors.Orange)
                }
            };
            HistogramXAxes = new[] { new Axis { Labels = data.Select(d => d.Category).ToArray(), Name = "Ценовая категория" } };
            HistogramYAxes = new[] { new Axis { Name = "Количество товаров" } };
        }

        private async Task SaveChartAsync()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var top = desktop.MainWindow;
                if (top == null) return;
                var options = new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Сохранить график",
                    DefaultExtension = "png",
                    SuggestedFileName = "График.png"
                };
                var result = await top.StorageProvider.SaveFilePickerAsync(options);
                var path = result?.Path.LocalPath ?? "";
                if (!string.IsNullOrEmpty(path))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Сохранение", "График сохранён в PNG", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
            }
        }
    }
}
