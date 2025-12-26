using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class AddRecipient : Window
    {
        private readonly int _userId;
        private readonly HealthcareManagementContext _context;
        private readonly ПолучателиУхода? _recipientToEdit;
        private string _selectedAvatarPath = string.Empty;
        private string? _originalAvatarPath;
        private _NotificationManager _notificationManager;

        public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register("WindowTitle", typeof(string), typeof(AddRecipient), new PropertyMetadata("Добавление получателя ухода"));

        public static readonly DependencyProperty SaveButtonTextProperty = DependencyProperty.Register("SaveButtonText", typeof(string), typeof(AddRecipient), new PropertyMetadata("Добавить получателя"));

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }

        public string SaveButtonText
        {
            get => (string)GetValue(SaveButtonTextProperty);
            set => SetValue(SaveButtonTextProperty, value);
        }

        public AddRecipient(int userId, ПолучателиУхода? recipientToEdit = null)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();
            _recipientToEdit = recipientToEdit;

            DataContext = this;
            LoadRecipientData();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void LoadRecipientData()
        {
            if (_recipientToEdit == null)
            {
                return;
            }

            WindowTitle = "Редактирование получателя ухода";
            SaveButtonText = "Сохранить изменения";

            RecipientNameTextBox.Text = _recipientToEdit.Имя;
            _originalAvatarPath = _recipientToEdit.Аватар;

            if (!string.IsNullOrEmpty(_recipientToEdit.Аватар))
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), _recipientToEdit.Аватар);
                if (File.Exists(absolutePath))
                {
                    _selectedAvatarPath = absolutePath;
                    LoadAvatarImage(absolutePath);
                }
            }
        }

        private void LoadAvatarImage(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                bitmap.Freeze();
                AvatarPreviewImage.Source = bitmap;
            }
            catch
            {
                AvatarPreviewImage.Source = null;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string recipientName = RecipientNameTextBox.Text?.Trim() ?? string.Empty;

            if (!ValidateInput(recipientName))
            {
                return;
            }

            if (_recipientToEdit != null)
            {
                UpdateRecipient(recipientName);
            }
            else
            {
                AddNewRecipient(recipientName);
            }

            string successMessage = _recipientToEdit != null ? "Получатель ухода успешно обновлен!" : "Получатель ухода успешно добавлен!";

            MessageBox.Show(successMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private bool ValidateInput(string recipientName)
        {
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                MessageBox.Show("Введите имя получателя ухода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                RecipientNameTextBox.Focus();
                return false;
            }

            if (recipientName.ToLower() == "вы")
            {
                MessageBox.Show("Имя \"Вы\" зарезервировано системой. Пожалуйста, выберите другое имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                RecipientNameTextBox.Focus();
                return false;
            }

            var existingRecipient = _context.ПолучателиУходаs.FirstOrDefault(r => r.Имя.Trim() == recipientName && r.FkIdПользователя == _userId &&
                (_recipientToEdit == null || r.PkIdПолучателя != _recipientToEdit.PkIdПолучателя));

            if (existingRecipient != null)
            {
                MessageBox.Show($"Получатель с именем \"{recipientName}\" уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                RecipientNameTextBox.Focus();
                return false;
            }

            return true;
        }

        private void AddNewRecipient(string recipientName)
        {
            string? avatarPath = SaveRecipientAvatar();

            var recipient = new ПолучателиУхода
            {
                Имя = recipientName,
                Аватар = avatarPath,
                FkIdПользователя = _userId
            };

            _context.ПолучателиУходаs.Add(recipient);
            _context.SaveChanges();
        }

        private void UpdateRecipient(string recipientName)
        {
            var recipient = _context.ПолучателиУходаs.Find(_recipientToEdit!.PkIdПолучателя);
            if (recipient == null)
            {
                return;
            }

            string? newAvatarPath = SaveRecipientAvatar();

            string? oldAvatarPath = recipient.Аватар;

            recipient.Имя = recipientName;
            recipient.Аватар = newAvatarPath;

            _context.SaveChanges();

            if (!string.IsNullOrEmpty(oldAvatarPath) && oldAvatarPath != newAvatarPath)
            {
                DeleteOldAvatar(oldAvatarPath);
            }
        }

        private string? SaveRecipientAvatar()
        {
            if (string.IsNullOrEmpty(_selectedAvatarPath))
            {
                return _recipientToEdit?.Аватар;
            }

            if (!File.Exists(_selectedAvatarPath))
            {
                return _recipientToEdit?.Аватар;
            }

            string avatarsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Sourse", "Images", "Recipients");
            Directory.CreateDirectory(avatarsFolder);

            string fileName = $"{Guid.NewGuid():N}{Path.GetExtension(_selectedAvatarPath)}";
            string destinationPath = Path.Combine(avatarsFolder, fileName);

            File.Copy(_selectedAvatarPath, destinationPath, true);
            return Path.Combine("Sourse", "Images", "Recipients", fileName);
        }

        private void DeleteOldAvatar(string? avatarPath)
        {
            if (string.IsNullOrEmpty(avatarPath))
            {
                return;
            }

            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), avatarPath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpeg;*.jpg;*.bmp;*.gif",
                Title = "Выберите аватар для получателя ухода"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedAvatarPath = openFileDialog.FileName;
                LoadAvatarImage(_selectedAvatarPath);
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            _notificationManager?.Dispose();
            base.OnClosed(e);
        }
    }
}