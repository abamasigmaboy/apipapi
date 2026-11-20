using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SupplyTrackingSystem.Models;
using SupplyTrackingSystem.Services;

namespace SupplyTrackingSystem.ViewModels
{
    public class SalesViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private ObservableCollection<Sale> _sales;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Department> _departments;
        private Sale? _selectedSale;
        private int _saleId;
        private int _selectedArticle;
        private DateTime _saleDate = DateTime.Now;
        private int _quantity;
        private int _selectedDepartmentCode;
        private decimal _calculatedTotal;
        private bool _isEditing;
        private string _statusMessage = "";

        public SalesViewModel()
        {
            _db = DatabaseService.Instance;
            _sales = new ObservableCollection<Sale>();
            _products = new ObservableCollection<Product>();
            _departments = new ObservableCollection<Department>();
            LoadData();

            AddCommand = ReactiveCommand.Create(AddSale);
            UpdateCommand = ReactiveCommand.Create(UpdateSale, this.WhenAnyValue(x => x.IsEditing));
            DeleteCommand = ReactiveCommand.Create(DeleteSale, this.WhenAnyValue(x => x.SelectedSale).Select(x => x != null));
            NewCommand = ReactiveCommand.Create(ClearForm);

            this.WhenAnyValue(x => x.SelectedSale).Subscribe(s =>
            {
                if (s != null)
                {
                    SaleId = s.SaleId;
                    SelectedArticle = s.Article;
                    SaleDate = s.SaleDate;
                    Quantity = s.Quantity;
                    SelectedDepartmentCode = s.DepartmentCode;
                    IsEditing = true;
                }
            });

            this.WhenAnyValue(x => x.SelectedArticle, x => x.Quantity).Subscribe(_ => UpdateTotal());
        }

        public ObservableCollection<Sale> Sales
        {
            get => _sales;
            set => this.RaiseAndSetIfChanged(ref _sales, value);
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => this.RaiseAndSetIfChanged(ref _products, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => this.RaiseAndSetIfChanged(ref _departments, value);
        }

        public Sale? SelectedSale
        {
            get => _selectedSale;
            set => this.RaiseAndSetIfChanged(ref _selectedSale, value);
        }

        public int SaleId
        {
            get => _saleId;
            set => this.RaiseAndSetIfChanged(ref _saleId, value);
        }

        public int SelectedArticle
        {
            get => _selectedArticle;
            set => this.RaiseAndSetIfChanged(ref _selectedArticle, value);
        }

        public DateTime SaleDate
        {
            get => _saleDate;
            set => this.RaiseAndSetIfChanged(ref _saleDate, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => this.RaiseAndSetIfChanged(ref _quantity, value);
        }

        public int SelectedDepartmentCode
        {
            get => _selectedDepartmentCode;
            set => this.RaiseAndSetIfChanged(ref _selectedDepartmentCode, value);
        }

        public decimal CalculatedTotal
        {
            get => _calculatedTotal;
            set => this.RaiseAndSetIfChanged(ref _calculatedTotal, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> NewCommand { get; }

        private void LoadData()
        {
            Sales.Clear();
            foreach (var s in _db.GetSales())
                Sales.Add(s);
            Products.Clear();
            foreach (var p in _db.GetProducts())
                Products.Add(p);
            Departments.Clear();
            foreach (var d in _db.GetDepartments())
                Departments.Add(d);
        }

        private void UpdateTotal()
        {
            var product = _db.GetProduct(SelectedArticle);
            if (product != null && Quantity > 0)
            {
                CalculatedTotal = Quantity * product.Price;
                StatusMessage = $"Сумма: {CalculatedTotal:N0} ₽";
            }
            else
            {
                CalculatedTotal = 0;
                StatusMessage = "";
            }
        }

        private void AddSale()
        {
            if (SelectedArticle == 0 || Quantity <= 0 || SelectedDepartmentCode == 0)
                return;
            var sale = new Sale
            {
                Article = SelectedArticle,
                SaleDate = SaleDate,
                Quantity = Quantity,
                DepartmentCode = SelectedDepartmentCode
            };
            _db.AddSale(sale);
            LoadData();
            ClearForm();
        }

        private void UpdateSale()
        {
            if (SelectedSale == null) return;
            SelectedSale.Article = SelectedArticle;
            SelectedSale.SaleDate = SaleDate;
            SelectedSale.Quantity = Quantity;
            SelectedSale.DepartmentCode = SelectedDepartmentCode;
            _db.UpdateSale(SelectedSale);
            LoadData();
        }

        private void DeleteSale()
        {
            if (SelectedSale == null) return;
            _db.DeleteSale(SelectedSale.SaleId);
            LoadData();
            ClearForm();
        }

        private void ClearForm()
        {
            SaleId = 0;
            SelectedArticle = 0;
            SaleDate = DateTime.Now;
            Quantity = 0;
            SelectedDepartmentCode = 0;
            CalculatedTotal = 0;
            SelectedSale = null;
            IsEditing = false;
            StatusMessage = "";
        }
    }
}
