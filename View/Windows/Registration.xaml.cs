using System.Windows;
using System.Windows.Controls;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Registration : Window
    {
        private HealthcareManagementContext _context;

        public static readonly DependencyProperty SelectedDateTextProperty = DependencyProperty.Register(nameof(SelectedDateText), typeof(string), typeof(Registration), new PropertyMetadata("  Выбрать..."));

        public string SelectedDateText
        {
            get => (string)GetValue(SelectedDateTextProperty);
            set => SetValue(SelectedDateTextProperty, value);
        }

        public static readonly DependencyProperty SelectedSexTextProperty = DependencyProperty.Register(nameof(SelectedSexText), typeof(string), typeof(Registration), new PropertyMetadata("  Выбрать..."));

        public string SelectedSexText
        {
            get => (string)GetValue(SelectedSexTextProperty);
            set => SetValue(SelectedSexTextProperty, value);
        }

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(nameof(SelectedDate), typeof(DateOnly?), typeof(Registration), new PropertyMetadata(null));

        public DateOnly? SelectedDate
        {
            get => (DateOnly?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly DependencyProperty SelectedGenderProperty = DependencyProperty.Register(nameof(SelectedGender), typeof(string), typeof(Registration), new PropertyMetadata(null));

        public string SelectedGender
        {
            get => (string)GetValue(SelectedGenderProperty);
            set => SetValue(SelectedGenderProperty, value);
        }

        public static readonly DependencyProperty GendersProperty = DependencyProperty.Register(nameof(Genders), typeof(List<string>), typeof(Registration), new PropertyMetadata(new List<string> { "Мужской", "Женский" }));

        public List<string> Genders
        {
            get => (List<string>)GetValue(GendersProperty);
            set => SetValue(GendersProperty, value);
        }

        public Registration()
        {
            InitializeComponent();
            _context = new HealthcareManagementContext();
            DataContext = this;
            InitializeDatePicker();
            InitializeSexList();
        }

        private void InitializeDatePicker()
        {
            BirthDatePicker.DisplayDateStart = DateTime.Now.AddYears(-200);
            BirthDatePicker.DisplayDateEnd = DateTime.Now;
            BirthDatePicker.SelectedDate = null;
        }

        private void InitializeSexList()
        {
            SexListBox.ItemsSource = Genders;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BirthDatePicker.SelectedDate.HasValue)
            {
                SelectedDate = DateOnly.FromDateTime(BirthDatePicker.SelectedDate.Value);
                SelectedDateText = $"  {SelectedDate.Value:dd.MM.yyyy}";
                DateExpander.IsExpanded = false;
            }
            else
            {
                SelectedDate = null;
                SelectedDateText = "  Выбрать...";
            }
        }

        private void SexListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SexListBox.SelectedItem != null)
            {
                SelectedGender = (string)SexListBox.SelectedItem;
                SelectedSexText = $"  {SelectedGender}";
                SexExpander.IsExpanded = false;
            }
        }

        private void BtnReady_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            if (!ConfirmIncompleteData())
            {
                return;
            }

            var newUser = CreateNewUser();

            _context.Пользовательs.Add(newUser);
            _context.SaveChanges();

            Start startWindow = new Start(newUser.PkIdПользователя);
            startWindow.Show();
            this.Close();
        }

        private Пользователь CreateNewUser()
        {
            return new Пользователь
            {
                Логин = BoxUserLogin.Text.Trim(),
                Email = BoxUserEmail.Text.Trim(),
                ДатаРождения = SelectedDate,
                Пол = SelectedGender
            };
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(BoxUserLogin.Text))
            {
                MessageBox.Show("Введите имя пользователя!", "Обязательное поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(BoxUserEmail.Text))
            {
                MessageBox.Show("Введите email!", "Обязательное поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            bool userExists = _context.Пользовательs.Any(u => u.Email == BoxUserEmail.Text.Trim() || u.Логин == BoxUserLogin.Text.Trim());

            if (userExists)
            {
                MessageBox.Show("Пользователь с таким email или логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ConfirmIncompleteData()
        {
            string warningMessage = "";

            if (!SelectedDate.HasValue)
            {
                warningMessage += "• Дата рождения не указана\n";
            }
            if (string.IsNullOrEmpty(SelectedGender))
            {
                warningMessage += "• Пол не указан\n";
            }

            if (string.IsNullOrEmpty(warningMessage))
            {
                return true;
            }

            warningMessage = "Вы не заполнили следующие поля (рекомендуется заполнить):\n\n" + warningMessage + "\nВы можете заполнить их позже в настройках профиля.";

            var result = MessageBox.Show(warningMessage, "Неполные данные", MessageBoxButton.YesNo, MessageBoxImage.Information);

            return result == MessageBoxResult.Yes;
        }

        private void LoginHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Authorization loginWindow = new Authorization();
            loginWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}