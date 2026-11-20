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
    public class ProductsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Department> _departments;
        private Product? _selectedProduct;
        private int _article;
        private string _name = "";
        private string _unit = "шт";
        private decimal _price;
        private int _selectedDepartmentCode;
        private bool _isEditing;

        public ProductsViewModel()
        {
            _db = DatabaseService.Instance;
            _products = new ObservableCollection<Product>();
            _departments = new ObservableCollection<Department>();
            LoadData();

            AddCommand = ReactiveCommand.Create(AddProduct);
            UpdateCommand = ReactiveCommand.Create(UpdateProduct, this.WhenAnyValue(x => x.IsEditing));
            DeleteCommand = ReactiveCommand.Create(DeleteProduct, this.WhenAnyValue(x => x.SelectedProduct).Select(x => x != null));
            NewCommand = ReactiveCommand.Create(ClearForm);

            this.WhenAnyValue(x => x.SelectedProduct).Subscribe(p =>
            {
                if (p != null)
                {
                    Article = p.Article;
                    Name = p.Name;
                    Unit = p.Unit;
                    Price = p.Price;
                    SelectedDepartmentCode = p.DepartmentCode;
                    IsEditing = true;
                }
            });
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

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => this.RaiseAndSetIfChanged(ref _selectedProduct, value);
        }

        public int Article
        {
            get => _article;
            set => this.RaiseAndSetIfChanged(ref _article, value);
        }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Unit
        {
            get => _unit;
            set => this.RaiseAndSetIfChanged(ref _unit, value);
        }

        public decimal Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }

        public int SelectedDepartmentCode
        {
            get => _selectedDepartmentCode;
            set => this.RaiseAndSetIfChanged(ref _selectedDepartmentCode, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> NewCommand { get; }

        private void LoadData()
        {
            Products.Clear();
            foreach (var p in _db.GetProducts())
                Products.Add(p);
            Departments.Clear();
            foreach (var d in _db.GetDepartments())
                Departments.Add(d);
        }

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(Name) || Price < 0 || SelectedDepartmentCode == 0)
                return;
            var product = new Product
            {
                Article = Article == 0 ? (_db.GetProducts().Count > 0 ? _db.GetProducts().Max(p => p.Article) + 1 : 1001) : Article,
                Name = Name,
                Unit = Unit,
                Price = Price,
                DepartmentCode = SelectedDepartmentCode
            };
            _db.AddProduct(product);
            LoadData();
            ClearForm();
        }

        private void UpdateProduct()
        {
            if (SelectedProduct == null) return;
            SelectedProduct.Name = Name;
            SelectedProduct.Unit = Unit;
            SelectedProduct.Price = Price;
            SelectedProduct.DepartmentCode = SelectedDepartmentCode;
            _db.UpdateProduct(SelectedProduct);
            LoadData();
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            _db.DeleteProduct(SelectedProduct.Article);
            LoadData();
            ClearForm();
        }

        private void ClearForm()
        {
            var nextArt = _db.GetProducts().Count > 0 ? _db.GetProducts().Max(p => p.Article) + 1 : 1001;
            Article = nextArt;
            Name = "";
            Unit = "шт";
            Price = 0;
            SelectedDepartmentCode = 0;
            SelectedProduct = null;
            IsEditing = false;
        }
    }
}
