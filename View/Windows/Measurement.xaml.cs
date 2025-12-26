using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Measurement : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private string? _selectedMeasurementType;

        private _NotificationManager _notificationManager;

        public ObservableCollection<string> MeasurementTypes { get; set; }

        public Measurement(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();
            LoadMeasurementTypes();
            MeasurementsList.ItemsSource = MeasurementTypes;

            _notificationManager = new _NotificationManager(_userId);
        }

        private void LoadMeasurementTypes()
        {
            var types = _context.ТипИзмеренияs.Select(t => t.Название).ToList();
            MeasurementTypes = new ObservableCollection<string>(types);
        }

        private void MeasurementBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is string measurementName)
            {
                _selectedMeasurementType = measurementName;
                ShowRecipientSelectionDialog();
            }
        }

        private void ShowRecipientSelectionDialog()
        {
            var dialog = new ChangeRecipient(_userId)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialog.IsForMyselfSelected)
                {
                    OpenAddMeasurement(null);
                }
                else if (dialog.SelectedRecipient != null)
                {
                    OpenAddMeasurement(dialog.SelectedRecipient);
                }
            }
        }

        private void OpenAddMeasurement(ПолучателиУхода? recipient)
        {
            var addMeasurementWindow = new AddMeasurement(_userId, recipient, _selectedMeasurementType);
            addMeasurementWindow.Show();
            this.Close();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Home HomeWindow = new Home(_userId);
            HomeWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            _notificationManager?.Dispose();
            base.OnClosed(e);
        }
    }
}