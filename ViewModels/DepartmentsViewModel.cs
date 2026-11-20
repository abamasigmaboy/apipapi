using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using SupplyTrackingSystem.Models;
using SupplyTrackingSystem.Services;
using System.Reactive.Linq;  // для .Select() на IObservable

namespace SupplyTrackingSystem.ViewModels
{
    public class DepartmentsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private ObservableCollection<Department> _departments;
        private Department? _selectedDepartment;
        private int _departmentCode;
        private string _name = "";
        private string _manager = "";
        private string _phone = "";
        private decimal _dailyPlan;
        private bool _isEditing;

        public DepartmentsViewModel()
        {
            _db = DatabaseService.Instance;
            _departments = new ObservableCollection<Department>();
            LoadDepartments();

            AddCommand = ReactiveCommand.Create(AddDepartment);
            UpdateCommand = ReactiveCommand.Create(UpdateDepartment, this.WhenAnyValue(x => x.IsEditing));
            DeleteCommand = ReactiveCommand.Create(
                DeleteDepartment,
                this.WhenAnyValue(x => x.SelectedDepartment)
                    .Select(selected => selected != null));
            NewCommand = ReactiveCommand.Create(ClearForm);

            this.WhenAnyValue(x => x.SelectedDepartment).Subscribe(d =>
            {
                if (d != null)
                {
                    DepartmentCode = d.DepartmentCode;
                    Name = d.Name;
                    Manager = d.Manager;
                    Phone = d.Phone;
                    DailyPlan = d.DailyPlan;
                    IsEditing = true;
                }
            });
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => this.RaiseAndSetIfChanged(ref _departments, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => this.RaiseAndSetIfChanged(ref _selectedDepartment, value);
        }

        public int DepartmentCode
        {
            get => _departmentCode;
            set => this.RaiseAndSetIfChanged(ref _departmentCode, value);
        }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Manager
        {
            get => _manager;
            set => this.RaiseAndSetIfChanged(ref _manager, value);
        }

        public string Phone
        {
            get => _phone;
            set => this.RaiseAndSetIfChanged(ref _phone, value);
        }

        public decimal DailyPlan
        {
            get => _dailyPlan;
            set => this.RaiseAndSetIfChanged(ref _dailyPlan, value);
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

        private void LoadDepartments()
        {
            Departments.Clear();
            foreach (var d in _db.GetDepartments())
                Departments.Add(d);
        }

        private void AddDepartment()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Manager) || DailyPlan < 0)
                return;
            var dept = new Department
            {
                DepartmentCode = DepartmentCode == 0 ? (_db.GetDepartments().Count > 0 ? _db.GetDepartments().Max(d => d.DepartmentCode) + 1 : 1) : DepartmentCode,
                Name = Name,
                Manager = Manager,
                Phone = Phone,
                DailyPlan = DailyPlan
            };
            _db.AddDepartment(dept);
            LoadDepartments();
            ClearForm();
        }

        private void UpdateDepartment()
        {
            if (SelectedDepartment == null) return;
            SelectedDepartment.Name = Name;
            SelectedDepartment.Manager = Manager;
            SelectedDepartment.Phone = Phone;
            SelectedDepartment.DailyPlan = DailyPlan;
            _db.UpdateDepartment(SelectedDepartment);
            LoadDepartments();
        }

        private void DeleteDepartment()
        {
            if (SelectedDepartment == null) return;
            _db.DeleteDepartment(SelectedDepartment.DepartmentCode);
            LoadDepartments();
            ClearForm();
        }

        private void ClearForm()
        {
            var nextCode = _db.GetDepartments().Count > 0 ? _db.GetDepartments().Max(d => d.DepartmentCode) + 1 : 1;
            DepartmentCode = nextCode;
            Name = "";
            Manager = "";
            Phone = "";
            DailyPlan = 0;
            SelectedDepartment = null;
            IsEditing = false;
        }
    }
}
