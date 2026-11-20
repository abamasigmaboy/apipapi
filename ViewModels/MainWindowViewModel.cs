using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using SupplyTrackingSystem.Views;

namespace SupplyTrackingSystem.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private string _currentViewTitle = "Главная";

        public MainWindowViewModel()
        {
            _currentViewModel = new DepartmentsViewModel();

            ShowDepartmentsCommand = ReactiveCommand.Create(() => { CurrentViewModel = new DepartmentsViewModel(); CurrentViewTitle = "Отделы"; });
            ShowProductsCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ProductsViewModel(); CurrentViewTitle = "Товары"; });
            ShowSalesCommand = ReactiveCommand.Create(() => { CurrentViewModel = new SalesViewModel(); CurrentViewTitle = "Продажи"; });
            ShowReportsCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ReportsViewModel(); CurrentViewTitle = "Отчёты"; });
            ShowChartsCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ChartsViewModel(); CurrentViewTitle = "Графики"; });
            ShowImportExportCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ImportExportViewModel(); CurrentViewTitle = "Импорт/Экспорт"; });
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set => this.RaiseAndSetIfChanged(ref _currentViewTitle, value);
        }

        public ReactiveCommand<Unit, Unit> ShowDepartmentsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowProductsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowSalesCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowReportsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowChartsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowImportExportCommand { get; }
    }
}
