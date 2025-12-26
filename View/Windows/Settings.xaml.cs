using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Settings : Window
    {
        private readonly int _userId;
        private readonly HealthcareManagementContext _context;
        private Пользователь _currentUser;
        private _NotificationManager _notificationManager;

        public static readonly DependencyProperty SelectedSexTextProperty =  DependencyProperty.Register(nameof(SelectedSexText), typeof(string), typeof(Settings), new PropertyMetadata("  Выбрать..."));

        public string SelectedSexText
        {
            get => (string)GetValue(SelectedSexTextProperty);
            set => SetValue(SelectedSexTextProperty, value);
        }

        public static readonly DependencyProperty GendersProperty = DependencyProperty.Register(nameof(Genders), typeof(List<string>), typeof(Settings), new PropertyMetadata(new List<string> { "Мужской", "Женский" }));

        public List<string> Genders
        {
            get => (List<string>)GetValue(GendersProperty);
            set => SetValue(GendersProperty, value);
        }

        public static readonly DependencyProperty RecipientsProperty = DependencyProperty.Register(nameof(Recipients), typeof(ObservableCollection<ПолучателиУхода>), typeof(Settings), new PropertyMetadata(null));

        public ObservableCollection<ПолучателиУхода> Recipients
        {
            get => (ObservableCollection<ПолучателиУхода>)GetValue(RecipientsProperty);
            set => SetValue(RecipientsProperty, value);
        }

        public Settings(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();

            this.DataContext = this;

            InitializeUserData();
            LoadRecipients();
            _notificationManager = new _NotificationManager(_userId);
        }

        private void InitializeUserData()
        {
            _currentUser = _context.Пользовательs.FirstOrDefault(u => u.PkIdПользователя == _userId);

            if (_currentUser == null)
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            UserNameTextBox.Text = _currentUser.Логин ?? string.Empty;
            UserEmailTextBox.Text = _currentUser.Email ?? string.Empty;

            if (_currentUser.ДатаРождения.HasValue)
            {
                BirthDatePicker.SelectedDate = new DateTime(_currentUser.ДатаРождения.Value.Year,
                    _currentUser.ДатаРождения.Value.Month, _currentUser.ДатаРождения.Value.Day);
            }

            if (!string.IsNullOrEmpty(_currentUser.Пол))
            {
                SelectedSexText = $"  {_currentUser.Пол}";

                var gender = Genders.FirstOrDefault(g => g == _currentUser.Пол);
                if (gender != null)
                {
                    SexListBox.SelectedItem = gender;
                }
            }

            LoadUserAvatar(_currentUser);
            UpdateGreeting(_currentUser);
        }

        private void LoadUserAvatar(Пользователь user)
        {
            if (!string.IsNullOrEmpty(user.Аватар))
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), user.Аватар);
                if (File.Exists(absolutePath))
                {
                    SetAvatarImage(absolutePath, AvatarPreviewImage);
                    return;
                }
            }
            AvatarPreviewImage.Source = null;
        }

        private void SetAvatarImage(string imagePath, Image imageControl)
        {
            if (File.Exists(imagePath))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                imageControl.Source = bitmapImage;
            }
            else
            {
                imageControl.Source = null;
            }
        }

        public static BitmapImage? GetRecipientAvatarImage(ПолучателиУхода recipient)
        {
            string avatarPath = GetRecipientAvatarPath(recipient);
            if (string.IsNullOrEmpty(avatarPath) || !File.Exists(avatarPath))
            {
                return null;
            }
            return LoadImage(avatarPath);
        }

        public static string GetRecipientAvatarPath(ПолучателиУхода recipient)
        {
            if (recipient == null) return null;

            if (!string.IsNullOrEmpty(recipient.Аватар))
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), recipient.Аватар);
                if (File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }
            return null;
        }

        private static BitmapImage? LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(path);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void LoadRecipients()
        {
            var recipients = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).ToList();
            Recipients = new ObservableCollection<ПолучателиУхода>(recipients);
            RecipientsItemsControl.ItemsSource = Recipients;
        }

        public void EditRecipient(ПолучателиУхода? recipient)
        {
            if (recipient == null)
            {
                return;
            }

            var editWindow = new AddRecipient(_userId, recipient);
            if (editWindow.ShowDialog() == true)
            {
                _context.Entry(recipient).Reload();

                int index = Recipients.IndexOf(recipient);
                if (index >= 0)
                {
                    Recipients.RemoveAt(index);
                    Recipients.Insert(index, recipient);

                    RecipientsItemsControl.ItemsSource = null;
                    RecipientsItemsControl.ItemsSource = Recipients;
                }
            }
        }

        private void AddRecipient_Click(object sender, RoutedEventArgs e)
        {
            var addRecipientWindow = new AddRecipient(_userId);
            if (addRecipientWindow.ShowDialog() == true)
            {
                LoadRecipients();
            }
        }

        private void BtnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите аватар"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            string sourcePath = openFileDialog.FileName;
            if (!File.Exists(sourcePath))
            {
                return;
            }

            string avatarsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Sourse", "Images", "Users");
            Directory.CreateDirectory(avatarsFolder);

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"user_{_userId}_{Guid.NewGuid()}{extension}";
            string destinationPath = Path.Combine(avatarsFolder, fileName);
            string relativePath = Path.Combine("Sourse", "Images", "Users", fileName);

            if (!string.IsNullOrEmpty(_currentUser.Аватар))
            {
                string oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), _currentUser.Аватар);
                if (File.Exists(oldAvatarPath))
                {
                    File.Delete(oldAvatarPath);
                }
            }

            File.Copy(sourcePath, destinationPath, true);

            _currentUser.Аватар = relativePath;
            _context.SaveChanges();

            SetAvatarImage(destinationPath, AvatarPreviewImage);
            MessageBox.Show("Аватар успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SexListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SexListBox.SelectedItem is string selectedSex)
            {
                SelectedSexText = $"  {selectedSex}";
                SexExpander.IsExpanded = false;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                MessageBox.Show("Имя пользователя не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(UserEmailTextBox.Text))
            {
                MessageBox.Show("Email не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingUser = _context.Пользовательs.FirstOrDefault(u => u.Email == UserEmailTextBox.Text.Trim() && u.PkIdПользователя != _userId);

            if (existingUser != null)
            {
                UserEmailTextBox.Text = _currentUser.Email ?? string.Empty;
                MessageBox.Show("Этот email уже используется другим пользователем", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser.Логин = UserNameTextBox.Text.Trim();
            _currentUser.Email = UserEmailTextBox.Text.Trim();

            if (BirthDatePicker.SelectedDate.HasValue)
            {
                var date = BirthDatePicker.SelectedDate.Value;
                _currentUser.ДатаРождения = new DateOnly(date.Year, date.Month, date.Day);
            }
            else
            {
                _currentUser.ДатаРождения = null;
            }

            if (SexListBox.SelectedItem is string selectedGender)
            {
                _currentUser.Пол = selectedGender;
            }
            else
            {
                _currentUser.Пол = null;
            }

            _context.SaveChanges();

            UpdateGreeting(_currentUser);
            MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateGreeting(Пользователь user)
        {
            if (GreetingText != null)
            {
                GreetingText.Text = $"Привет, {user.Логин ?? "Пользователь"}!";
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Home home = new Home(_userId);
            home.Show();
            Close();
        }

        private void BtnExitProfile_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из профиля?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new Authorization();
                loginWindow.Show();
                Close();
            }
        }

        private void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "ВНИМАНИЕ!\n\n" +
                "Удаление профиля приведет к:\n" +
                "• Безвозвратному удалению всех ваших данных\n" +
                "• Удалению всех получателей ухода\n" +
                "• Удалению всех лекарств, измерений и напоминаний\n" +
                "• Удалению личного архива и рецептов\n\n" +
                "Это действие нельзя отменить!\n\n" +
                "Вы уверены, что хотите удалить профиль?",
                "УДАЛЕНИЕ ПРОФИЛЯ",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteUserProfile();
            }
        }

        public void DeleteRecipient(ПолучателиУхода? recipient)
        {
            if (recipient == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить получателя \"{recipient.Имя}\"?\n\n" +
                "Это действие приведет к удалению всех связанных данных (лекарств, измерений, симптомов и т.д.)",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            using var transaction = _context.Database.BeginTransaction();

            var recipientToDelete = _context.ПолучателиУходаs
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.НапоминаниеЛекарстваs).ThenInclude(nl => nl.ФиксацияПриёмаs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.СоставВЛекарствеs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.FkIdЛичныйАрхивNavigation)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Измерениеs).ThenInclude(m => m.НапоминаниеИзмеренияs).ThenInclude(ni => ni.ЗначенияИзмеренияs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Симптомыs).ThenInclude(s => s.НапоминаниеСимптомыs).ThenInclude(ns => ns.ЗафиксированныеСимптомыs)
                .Include(r => r.Аллергииs)
                .Include(r => r.ЛичныйАрхивs)
                .FirstOrDefault(r => r.PkIdПолучателя == recipient.PkIdПолучателя);

            if (recipientToDelete == null)
            {
                return;
            }

            _context.SaveChanges();
            transaction.Commit();

            Recipients.Remove(recipient);
            RecipientsItemsControl.ItemsSource = null;
            RecipientsItemsControl.ItemsSource = Recipients;

            MessageBox.Show("Получатель успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteUserProfile()
        {
            using var transaction = _context.Database.BeginTransaction();

            var userToDelete = _context.Пользовательs
                .Include(u => u.ПолучателиУходаs)
                .Include(u => u.АрхивРецептовs)
                .Include(u => u.Аллергииs.Where(a => a.FkIdПолучателя == null))
                .Include(u => u.ЛичныйАрхивs.Where(la => la.FkIdПолучателя == null))
                .FirstOrDefault(u => u.PkIdПользователя == _userId);

            if (userToDelete == null)
            {
                return;
            }

            foreach (var recipient in userToDelete.ПолучателиУходаs.ToList())
            {
                DeleteRecipientWithAllData(recipient);
            }

            var userPositions = _context.ПозицияЗаписиs
                .Where(p => p.FkIdПользователя == _userId && p.FkIdПолучателя == null)
                .Include(p => p.Лекарстваs).ThenInclude(l => l.НапоминаниеЛекарстваs).ThenInclude(nl => nl.ФиксацияПриёмаs)
                .Include(p => p.Лекарстваs).ThenInclude(l => l.СоставВЛекарствеs)
                .Include(p => p.Лекарстваs).ThenInclude(l => l.FkIdЛичныйАрхивNavigation)
                .Include(p => p.Измерениеs).ThenInclude(m => m.НапоминаниеИзмеренияs).ThenInclude(ni => ni.ЗначенияИзмеренияs)
                .Include(p => p.Симптомыs).ThenInclude(s => s.НапоминаниеСимптомыs).ThenInclude(ns => ns.ЗафиксированныеСимптомыs)
                .ToList();

            foreach (var position in userPositions)
            {
                foreach (var medicine in position.Лекарстваs.ToList())
                {
                    medicine.FkIdЛичныйАрхив = null;

                    foreach (var reminder in medicine.НапоминаниеЛекарстваs.ToList())
                    {
                        _context.ФиксацияПриёмаs.RemoveRange(reminder.ФиксацияПриёмаs);
                        _context.НапоминаниеЛекарстваs.Remove(reminder);
                    }

                    _context.СоставВЛекарствеs.RemoveRange(medicine.СоставВЛекарствеs);
                    _context.Лекарстваs.Remove(medicine);
                }

                foreach (var measurement in position.Измерениеs.ToList())
                {
                    foreach (var reminder in measurement.НапоминаниеИзмеренияs.ToList())
                    {
                        _context.ЗначенияИзмеренияs.RemoveRange(reminder.ЗначенияИзмеренияs);
                        _context.НапоминаниеИзмеренияs.Remove(reminder);
                    }
                    _context.Измерениеs.Remove(measurement);
                }

                foreach (var symptom in position.Симптомыs.ToList())
                {
                    foreach (var reminder in symptom.НапоминаниеСимптомыs.ToList())
                    {
                        _context.ЗафиксированныеСимптомыs.RemoveRange(reminder.ЗафиксированныеСимптомыs);
                        _context.НапоминаниеСимптомыs.Remove(reminder);
                    }
                    _context.Симптомыs.Remove(symptom);
                }

                _context.ПозицияЗаписиs.Remove(position);
            }

            var userPersonalArchives = _context.ЛичныйАрхивs.Where(la => la.FkIdПользователя == _userId && la.FkIdПолучателя == null).Include(la => la.Лекарстваs).ToList();

            foreach (var personalArchive in userPersonalArchives)
            {
                foreach (var medicine in personalArchive.Лекарстваs.ToList())
                {
                    foreach (var reminder in medicine.НапоминаниеЛекарстваs.ToList())
                    {
                        _context.ФиксацияПриёмаs.RemoveRange(reminder.ФиксацияПриёмаs);
                        _context.НапоминаниеЛекарстваs.Remove(reminder);
                    }

                    _context.СоставВЛекарствеs.RemoveRange(medicine.СоставВЛекарствеs);
                    medicine.FkIdЛичныйАрхив = null;
                    _context.Лекарстваs.Remove(medicine);
                }

                _context.ЛичныйАрхивs.Remove(personalArchive);
            }

            foreach (var recipe in userToDelete.АрхивРецептовs.ToList())
            {
                if (!string.IsNullOrEmpty(recipe.Изображение))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), recipe.Изображение);
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
                _context.АрхивРецептовs.Remove(recipe);
            }

            var userAllergies = _context.Аллергииs.Where(a => a.FkIdПользователя == _userId && a.FkIdПолучателя == null).ToList();
            _context.Аллергииs.RemoveRange(userAllergies);

            DeleteUserFiles(userToDelete);

            _context.Пользовательs.Remove(userToDelete);

            _context.SaveChanges();
            transaction.Commit();

            MessageBox.Show("Профиль успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            var loginWindow = new Authorization();
            loginWindow.Show();
            Close();
        }

        private void DeleteRecipientWithAllData(ПолучателиУхода recipient)
        {
            var recipientToDelete = _context.ПолучателиУходаs
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.НапоминаниеЛекарстваs).ThenInclude(nl => nl.ФиксацияПриёмаs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.СоставВЛекарствеs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Лекарстваs).ThenInclude(l => l.FkIdЛичныйАрхивNavigation)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Измерениеs).ThenInclude(m => m.НапоминаниеИзмеренияs).ThenInclude(ni => ni.ЗначенияИзмеренияs)
                .Include(r => r.ПозицияЗаписиs).ThenInclude(p => p.Симптомыs).ThenInclude(s => s.НапоминаниеСимптомыs).ThenInclude(ns => ns.ЗафиксированныеСимптомыs)
                .Include(r => r.Аллергииs)
                .Include(r => r.ЛичныйАрхивs).ThenInclude(la => la.Лекарстваs)
                .FirstOrDefault(r => r.PkIdПолучателя == recipient.PkIdПолучателя);

            if (recipientToDelete == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(recipientToDelete.Аватар))
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), recipientToDelete.Аватар);
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }

            foreach (var position in recipientToDelete.ПозицияЗаписиs.ToList())
            {
                foreach (var medicine in position.Лекарстваs.ToList())
                {
                    medicine.FkIdЛичныйАрхив = null;

                    foreach (var reminder in medicine.НапоминаниеЛекарстваs.ToList())
                    {
                        _context.ФиксацияПриёмаs.RemoveRange(reminder.ФиксацияПриёмаs);
                        _context.НапоминаниеЛекарстваs.Remove(reminder);
                    }

                    _context.СоставВЛекарствеs.RemoveRange(medicine.СоставВЛекарствеs);
                    _context.Лекарстваs.Remove(medicine);
                }

                foreach (var measurement in position.Измерениеs.ToList())
                {
                    foreach (var reminder in measurement.НапоминаниеИзмеренияs.ToList())
                    {
                        _context.ЗначенияИзмеренияs.RemoveRange(reminder.ЗначенияИзмеренияs);
                        _context.НапоминаниеИзмеренияs.Remove(reminder);
                    }
                    _context.Измерениеs.Remove(measurement);
                }

                foreach (var symptom in position.Симптомыs.ToList())
                {
                    foreach (var reminder in symptom.НапоминаниеСимптомыs.ToList())
                    {
                        _context.ЗафиксированныеСимптомыs.RemoveRange(reminder.ЗафиксированныеСимптомыs);
                        _context.НапоминаниеСимптомыs.Remove(reminder);
                    }
                    _context.Симптомыs.Remove(symptom);
                }

                _context.ПозицияЗаписиs.Remove(position);
            }

            _context.Аллергииs.RemoveRange(recipientToDelete.Аллергииs);

            foreach (var личныйАрхив in recipientToDelete.ЛичныйАрхивs.ToList())
            {
                foreach (var medicine in личныйАрхив.Лекарстваs.ToList())
                {
                    foreach (var reminder in medicine.НапоминаниеЛекарстваs.ToList())
                    {
                        _context.ФиксацияПриёмаs.RemoveRange(reminder.ФиксацияПриёмаs);
                        _context.НапоминаниеЛекарстваs.Remove(reminder);
                    }

                    _context.СоставВЛекарствеs.RemoveRange(medicine.СоставВЛекарствеs);
                    medicine.FkIdЛичныйАрхив = null;
                    _context.Лекарстваs.Remove(medicine);
                }

                _context.ЛичныйАрхивs.Remove(личныйАрхив);
            }

            _context.ПолучателиУходаs.Remove(recipientToDelete);
        }

        private void DeleteUserFiles(Пользователь user)
        {
            if (!string.IsNullOrEmpty(user.Аватар))
            {
                string userAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), user.Аватар);
                if (File.Exists(userAvatarPath))
                {
                    File.Delete(userAvatarPath);
                }
            }

            var recipients = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).ToList();
            foreach (var recipient in recipients)
            {
                if (!string.IsNullOrEmpty(recipient.Аватар))
                {
                    string avatarPath = Path.Combine(Directory.GetCurrentDirectory(), recipient.Аватар);
                    if (File.Exists(avatarPath))
                    {
                        File.Delete(avatarPath);
                    }
                }
            }

            var recipes = _context.АрхивРецептовs.Where(r => r.FkIdПользователя == _userId).ToList();
            foreach (var recipe in recipes)
            {
                if (!string.IsNullOrEmpty(recipe.Изображение))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), recipe.Изображение);
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
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