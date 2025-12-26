using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class ChangeRecipient : Window
    {
        private readonly int _userId;
        private HealthcareManagementContext _context;
        private ObservableCollection<RecipientItem> _recipients;
        private string _measurementName;
        private bool _isForSymptom = false;

        private _NotificationManager _notificationManager;

        public ПолучателиУхода? SelectedRecipient { get; private set; }
        public bool IsForMyselfSelected { get; private set; }

        // 1. Перегрузка для лекарств (базовый конструктор)
        public ChangeRecipient(int userId)
        {
            InitializeComponent();
            _userId = userId;
            InitializeDialog();

            _notificationManager = new _NotificationManager(_userId);
        }

        // 2. Перегрузка для измерений
        public ChangeRecipient(int userId, string measurementName)
        {
            InitializeComponent();
            _userId = userId;
            _measurementName = measurementName;
            InitializeDialog();

            _notificationManager = new _NotificationManager(_userId);
        }

        // 3. Перегрузка для симптомов
        public ChangeRecipient(int userId, bool isSymptom)
        {
            InitializeComponent();
            _userId = userId;
            _isForSymptom = true;
            InitializeDialog();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void InitializeDialog()
        {
            _recipients = new ObservableCollection<RecipientItem>();
            DataContext = this;
            _context = new HealthcareManagementContext();
            LoadRecipients();
            LoadUserInfo();
            UpdateWindow();
        }

        private void UpdateWindow()
        {
            if (!string.IsNullOrEmpty(_measurementName))
            {
                Title = $"Выбор получателя для измерения: {_measurementName}";
                TextForMyself.Text = "Для себя";
                TextRecipients.Text = "Получатели ухода";
            }
            else if (_isForSymptom)
            {
                Title = $"Выбор получателя для отслеживания симптомов и настроения";
                TextForMyself.Text = "Для себя";
                TextRecipients.Text = "Получатели ухода";
            }
            else
            {
                Title = "Выбор получателя для лекарства";
                TextForMyself.Text = "Для себя";
                TextRecipients.Text = "Получатели ухода";
            }
        }

        private void LoadUserInfo()
        {
            var user = _context.Пользовательs.FirstOrDefault(u => u.PkIdПользователя == _userId);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
                return;
            }

            var userAvatar = GetRecipientAvatarImage(new ПолучателиУхода
            {
                Аватар = user.Аватар
            });

            if (userAvatar != null)
            {
                UserAvatarImage.Source = userAvatar;
            }
        }

        private void LoadRecipients()
        {
            using var context = new HealthcareManagementContext();
            var recipients = context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).OrderBy(r => r.Имя).ToList();

            foreach (var recipient in recipients)
            {
                _recipients.Add(new RecipientItem(recipient));
            }

            RecipientsItemsControl.ItemsSource = _recipients;
        }

        private void RecipientCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RecipientItem item)
            {
                SelectedRecipient = item.Recipient;
                IsForMyselfSelected = false;
                DialogResult = true;
                Close();
            }
        }

        private void BtnSelectForMyself_Click(object sender, RoutedEventArgs e)
        {
            SelectedRecipient = null;
            IsForMyselfSelected = true;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public class RecipientItem
        {
            public ПолучателиУхода Recipient { get; }
            public string Имя => Recipient.Имя;
            public BitmapImage? AvatarImage => GetRecipientAvatarImage(Recipient);

            public RecipientItem(ПолучателиУхода recipient)
            {
                Recipient = recipient;
            }
        }

        public static BitmapImage? GetRecipientAvatarImage(ПолучателиУхода recipient)
        {
            if (recipient == null || string.IsNullOrEmpty(recipient.Аватар))
            {
                return null;
            }

            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(recipient.Аватар, UriKind.RelativeOrAbsolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            _notificationManager?.Dispose();
            base.OnClosed(e);
        }
    }
}