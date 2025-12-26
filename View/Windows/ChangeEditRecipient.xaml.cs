using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class ChangeEditRecipient : Window
    {
        private readonly int _userId;
        private HealthcareManagementContext _context;
        private ObservableCollection<RecipientItem> _recipients;

        private _NotificationManager _notificationManager;

        public int? SelectedRecipientId { get; private set; }
        public bool IsForMyself { get; private set; }

        public ChangeEditRecipient(int userId, string recordType)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();
            _recipients = new ObservableCollection<RecipientItem>();
            DataContext = this;

            Title = $"Изменение получателя для {GetRecordTypeText(recordType)}";
            LoadRecipients();
            LoadUserInfo();

            _notificationManager = new _NotificationManager(_userId);
        }

        private string GetRecordTypeText(string recordType)
        {
            return recordType switch
            {
                "Medicine" => "лекарства",
                "Measurement" => "измерения",
                "Symptom" => "симптома",
                _ => "записи"
            };
        }

        private void LoadUserInfo()
        {
            var user = _context.Пользовательs.FirstOrDefault(u => u.PkIdПользователя == _userId);

            if (user != null)
            {
                var userAvatar = ChangeRecipient.GetRecipientAvatarImage(new ПолучателиУхода
                {
                    Аватар = user.Аватар
                });

                if (userAvatar != null)
                {
                    UserAvatarImage.Source = userAvatar;
                }
            }
        }

        private void LoadRecipients()
        {
            var recipients = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).OrderBy(r => r.Имя).ToList();

            foreach (var recipient in recipients)
            {
                _recipients.Add(new RecipientItem(recipient));
            }

            RecipientsItemsControl.ItemsSource = _recipients;
        }

        private void ForMyselfCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedRecipientId = null;
            IsForMyself = true;
            DialogResult = true;
            Close();
        }

        private void RecipientCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RecipientItem item)
            {
                SelectedRecipientId = item.Recipient.PkIdПолучателя;
                IsForMyself = false;
                DialogResult = true;
                Close();
            }
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
            public BitmapImage? AvatarImage => ChangeRecipient.GetRecipientAvatarImage(Recipient);

            public RecipientItem(ПолучателиУхода recipient)
            {
                Recipient = recipient;
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