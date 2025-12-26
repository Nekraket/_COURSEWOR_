using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class AddPersonalMed : Window
    {
        private int _userId;
        private int _categoryId;
        private ПолучателиУхода? _currentRecipient;
        private string _selectedIcon = "таблетка";
        private Color _selectedColor = (Color)ColorConverter.ConvertFromString("#9A00D7");
        private string _selectedPhotoPath = null;
        private ObservableCollection<string> _compositionItems = new ObservableCollection<string>();
        private Лекарства _savedMedicine;

        private HealthcareManagementContext _context;

        private _NotificationManager _notificationManager;

        private System.Collections.Generic.List<string> _userAllergens = new System.Collections.Generic.List<string>();

        private System.Collections.Generic.Dictionary<string, string> _iconDataCache;
        private System.Collections.Generic.List<ColorItem> _colorsCache;

        public event Action<Лекарства> MedicineSaved;

        public class ColorItem
        {
            public SolidColorBrush Color { get; set; }
            public string HexColor { get; set; }
        }

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register(nameof(PageTitle), typeof(string), typeof(AddPersonalMed), new PropertyMetadata("Добавление средства в категорию архива"));

        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static readonly DependencyProperty IconDataMapProperty = DependencyProperty.Register(nameof(IconDataMap), typeof(System.Collections.Generic.Dictionary<string, string>), typeof(AddPersonalMed), new PropertyMetadata(null));

        public System.Collections.Generic.Dictionary<string, string> IconDataMap
        {
            get => (System.Collections.Generic.Dictionary<string, string>)GetValue(IconDataMapProperty);
            set => SetValue(IconDataMapProperty, value);
        }

        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register(nameof(AvailableColors), typeof(System.Collections.Generic.List<ColorItem>), typeof(AddPersonalMed), new PropertyMetadata(null));

        public System.Collections.Generic.List<ColorItem> AvailableColors
        {
            get => (System.Collections.Generic.List<ColorItem>)GetValue(AvailableColorsProperty);
            set => SetValue(AvailableColorsProperty, value);
        }

        public AddPersonalMed(int userId, int categoryId, ПолучателиУхода? recipient = null)
        {
            InitializeComponent();

            _context = new HealthcareManagementContext();
            _userId = userId;
            _categoryId = categoryId;
            _currentRecipient = recipient;

            if (_currentRecipient != null)
            {
                PageTitle = $"Добавление средства для {_currentRecipient.Имя} в категорию архива";
            }

            if (CompositionListBox != null)
            {
                CompositionListBox.ItemsSource = _compositionItems;
            }

            DataContext = this;
            Loaded += AddPersonalMed_Loaded;

            _notificationManager = new _NotificationManager(_userId);
        }

        private void AddPersonalMed_Loaded(object sender, RoutedEventArgs e)
        {
            HideRemindersForArchive();
            LoadInitialData();
            InitializeDefaults();
        }

        private void LoadInitialData()
        {
            LoadIcons();
            LoadColors();
            LoadUserAllergens();
        }

        private void LoadIcons()
        {
            _iconDataCache = new System.Collections.Generic.Dictionary<string, string>();

            var icons = _context.ВектораИзображенийs.Where(v => v.FkIdКатегорииОтслеж == 1).ToList();

            foreach (var icon in icons)
            {
                if (!string.IsNullOrEmpty(icon.Название) && !string.IsNullOrEmpty(icon.Вектор))
                {
                    _iconDataCache[icon.Название] = icon.Вектор;
                }
            }

            Dispatcher.Invoke(() =>
            {
                IconDataMap = _iconDataCache;
            });
        }

        private void LoadColors()
        {
            using (var db = new HealthcareManagementContext())
            {
                var dbColors = db.ЦветИконкиЛекарствs.ToList();
                _colorsCache = dbColors.Select(color => new ColorItem
                {
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color.Цвет)),
                    HexColor = color.Цвет
                }).ToList();
            }

            Dispatcher.Invoke(() =>
            {
                AvailableColors = _colorsCache;
                if (_colorsCache != null && _colorsCache.Any())
                {
                    try
                    {
                        _selectedColor = (Color)ColorConverter.ConvertFromString(_colorsCache[0].HexColor);
                        UpdatePreviewIcon();
                    }
                    catch
                    {
                        _selectedColor = Colors.Purple;
                        UpdatePreviewIcon();
                    }
                }
            });
        }

        private void LoadUserAllergens()
        {
            using (var db = new HealthcareManagementContext())
            {
                _userAllergens.Clear();

                if (_currentRecipient == null)
                {
                    var userAllergens = db.Аллергииs
                        .Where(a => a.FkIdПользователя == _userId && a.FkIdПолучателя == null)
                        .Select(a => a.Аллерген)
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Select(a => a.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    _userAllergens.AddRange(userAllergens);
                }
                else
                {
                    var recipientAllergens = db.Аллергииs
                        .Where(a => a.FkIdПолучателя == _currentRecipient.PkIdПолучателя)
                        .Select(a => a.Аллерген)
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Select(a => a.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    _userAllergens.AddRange(recipientAllergens);
                }
            }
        }

        private void HideRemindersForArchive()
        {
            var elementsToHide = new[]
            {
                FindName("RemindersToggle") as FrameworkElement,
                FindName("StockRemindersToggle") as FrameworkElement,
                FindName("TimeSelectionPanel") as FrameworkElement
            };

            foreach (var element in elementsToHide)
            {
                if (element != null)
                {
                    element.Visibility = Visibility.Collapsed;
                }
            }

            var parentStackPanel = FindName("SettingsStackPanel") as StackPanel;
            if (parentStackPanel != null)
            {
                foreach (var child in parentStackPanel.Children)
                {
                    if (child is Border border)
                    {
                        var content = border.Child as FrameworkElement;
                        if (content != null)
                        {
                            if (content is StackPanel stackPanel)
                            {
                                foreach (var stackChild in stackPanel.Children)
                                {
                                    if (stackChild is Grid grid)
                                    {
                                        foreach (var gridChild in grid.Children)
                                        {
                                            if (gridChild is TextBlock textBlock)
                                            {
                                                if (textBlock.Text.Contains("Уведомления") ||
                                                    textBlock.Text.Contains("напоминания"))
                                                {
                                                    border.Visibility = Visibility.Collapsed;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeDefaults()
        {
            if (OncePerDay != null)
            {
                OncePerDay.IsChecked = true;
            }
            if (BeforeMeal != null)
            {
                BeforeMeal.IsChecked = true;
            }

            if (_colorsCache == null || !_colorsCache.Any())
            {
                _selectedColor = Colors.Purple;
            }

            UpdatePreviewIcon();
        }

        private void CommentsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void CommentsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Добавьте ваши комментарии и рекомендации по применению средства...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private string GetIntakeMethodText()
        {
            var radioButtons = new[] { BeforeMeal, AfterMeal, DuringMeal, NoMatter };
            var checkedRadio = radioButtons.FirstOrDefault(r => r?.IsChecked == true);
            return checkedRadio?.Name switch
            {
                "BeforeMeal" => "Перед едой",
                "AfterMeal" => "После еды",
                "DuringMeal" => "Во время еды",
                "NoMatter" => "Не имеет значения",
                _ => "Перед едой"
            };
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        private void SaveToGuideButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            SaveMedicineData();

            MedicineSaved?.Invoke(_savedMedicine);
            MessageBox.Show("Средство успешно добавлено в категорию архива!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private bool ValidateInputs()
        {
            string medicineName = GetTextBoxValue(BoxNameMed);
            if (string.IsNullOrWhiteSpace(medicineName))
            {
                ShowError("Введите название средства");
                return false;
            }

            if (!int.TryParse(DosageTextBox?.Text ?? "1", out int dosage) || dosage <= 0)
            {
                ShowError("Введите корректное количество");
                return false;
            }

            return true;
        }

        private List<string> FindAllergenMatches(string componentName)
        {
            var matches = new List<string>();

            if (string.IsNullOrWhiteSpace(componentName) || _userAllergens.Count == 0)
            {
                return matches;
            }

            var componentLower = componentName.Trim().ToLower();

            foreach (var allergen in _userAllergens)
            {
                if (componentLower == allergen)
                {
                    matches.Add($"точное совпадение: {allergen}");
                }
                else if (componentLower.Contains(allergen))
                {
                    matches.Add($"содержит: {allergen}");
                }
                else if (allergen.Contains(componentLower))
                {
                    matches.Add($"является частью: {allergen}");
                }
            }

            return matches.Distinct().ToList();
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            return textBox?.Foreground == Brushes.Gray ? "" : textBox?.Text ?? "";
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SaveMedicineData()
        {
            string medicineName = GetTextBoxValue(BoxNameMed);
            string комментарии = GetTextBoxValue(CommentsTextBox);

            using (var db = new HealthcareManagementContext())
            {
                var векторИзображения = db.ВектораИзображенийs.FirstOrDefault(v => v.Название == _selectedIcon);

                if (векторИзображения == null)
                {
                    векторИзображения = db.ВектораИзображенийs.FirstOrDefault(v => v.Название == "таблетка");
                }

                string цветHex = _selectedColor.ToString();
                var цветИконки = db.ЦветИконкиЛекарствs.FirstOrDefault(c => c.Цвет == цветHex);
                if (цветИконки == null)
                {
                    цветИконки = new ЦветИконкиЛекарств { Цвет = цветHex };
                    db.ЦветИконкиЛекарствs.Add(цветИконки);
                    db.SaveChanges();
                }

                var иконка = new ИконкиЛекарств
                {
                    FkIdВектора = векторИзображения.PkIdВектора,
                    FkIdЦветИконки = цветИконки.PkIdЦветИконки
                };
                db.ИконкиЛекарствs.Add(иконка);
                db.SaveChanges();

                var способПриемаТекст = GetIntakeMethodText();
                var способПриема = db.СпособыПриёмаs.FirstOrDefault(m => m.Тип == способПриемаТекст);
                if (способПриема == null)
                {
                    способПриема = new СпособыПриёма { Тип = способПриемаТекст };
                    db.СпособыПриёмаs.Add(способПриема);
                    db.SaveChanges();
                }

                string путьФото = null;
                if (!string.IsNullOrEmpty(_selectedPhotoPath) && File.Exists(_selectedPhotoPath))
                {
                    путьФото = SaveMedicinePhoto(_selectedPhotoPath, medicineName);
                }

                var лекарство = new Лекарства
                {
                    Название = medicineName,
                    Комментарий = комментарии,
                    Дозировка = int.Parse(DosageTextBox?.Text ?? "1"),
                    FkIdИконки = иконка.PkIdИконки,
                    FkIdСпособаПриёма = способПриема.PkIdСпособаПриёма,
                    Фото = путьФото,
                    НапоминанияЗапас = false,
                    ТекущийЗапас = 0,
                    МинЗапас = 0
                };

                db.Лекарстваs.Add(лекарство);
                db.SaveChanges();

                foreach (var component in _compositionItems)
                {
                    var составЛекарства = db.СоставЛекарстваs.FirstOrDefault(s => s.НазваниеСоставляющей == component);

                    if (составЛекарства == null)
                    {
                        составЛекарства = new СоставЛекарства
                        {
                            НазваниеСоставляющей = component
                        };
                        db.СоставЛекарстваs.Add(составЛекарства);
                        db.SaveChanges();
                    }

                    var составВЛекарстве = new СоставВЛекарстве
                    {
                        FkIdЛекарства = лекарство.PkIdЛекарства,
                        FkIdСоставаЛекарства = составЛекарства.PkIdСостава
                    };
                    db.СоставВЛекарствеs.Add(составВЛекарстве);
                }
                db.SaveChanges();

                var личныйАрхив = db.ЛичныйАрхивs.FirstOrDefault(la => la.FkIdКатегорииАрхива == _categoryId && la.FkIdПользователя == _userId);

                if (личныйАрхив == null)
                {
                    личныйАрхив = new ЛичныйАрхив
                    {
                        FkIdКатегорииАрхива = _categoryId,
                        FkIdПользователя = _userId
                    };
                    db.ЛичныйАрхивs.Add(личныйАрхив);
                    db.SaveChanges();
                }

                лекарство.FkIdЛичныйАрхив = личныйАрхив.PkIdЛичногоАрхива;

                var категорияОтслеживания = db.КатегорииОтслеживанияs.FirstOrDefault(c => c.Тип == "Лекарства");
                if (категорияОтслеживания == null)
                {
                    категорияОтслеживания = new КатегорииОтслеживания { Тип = "Лекарства" };
                    db.КатегорииОтслеживанияs.Add(категорияОтслеживания);
                    db.SaveChanges();
                }

                var позиция = new ПозицияЗаписи
                {
                    FkIdКатегорииОтслеж = категорияОтслеживания.PkIdКатегорииОтслеж,
                    FkIdПользователя = _userId,
                    FkIdПолучателя = _currentRecipient?.PkIdПолучателя,
                    Активность = true,
                    ДатаСоздания = DateTime.Now
                };
                db.ПозицияЗаписиs.Add(позиция);
                db.SaveChanges();

                лекарство.FkIdПозиции = позиция.PkIdПозиции;
                db.SaveChanges();

                _savedMedicine = лекарство;
            }
        }

        private string SaveMedicinePhoto(string sourcePath, string medicineName)
        {
            string appPhotosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MedicinePhotos");
            if (!Directory.Exists(appPhotosDir))
            {
                Directory.CreateDirectory(appPhotosDir);
            }

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"{medicineName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            string destinationPath = Path.Combine(appPhotosDir, fileName);

            File.Copy(sourcePath, destinationPath, true);

            return $"MedicinePhotos/{fileName}";
        }

        private void UpdatePreviewIcon()
        {
            if (PreviewIconPath == null)
            {
                return;
            }

            string selectedIconVector = _iconDataCache?.GetValueOrDefault(_selectedIcon) ?? "";
            if (!string.IsNullOrEmpty(selectedIconVector))
            {
                PreviewIconPath.Data = Geometry.Parse(selectedIconVector);
            }
            else
            {
                PreviewIconPath.Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
            }

            var colorAnimation = new ColorAnimation
            {
                To = _selectedColor,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            PreviewIconPath.Fill = new SolidColorBrush(_selectedColor);

            if (PreviewIconPath.Fill is SolidColorBrush brush)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border iconBorder && iconBorder.Tag is string iconName)
            {
                _selectedIcon = iconName;
                UpdatePreviewIcon();
            }
        }

        private void Color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border colorBorder && colorBorder.Tag is ColorItem colorItem)
            {
                try
                {
                    _selectedColor = (Color)ColorConverter.ConvertFromString(colorItem.HexColor);
                    UpdatePreviewIcon();
                }
                catch
                {
                    _selectedColor = Colors.Black;
                    UpdatePreviewIcon();
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "BoxNameMed")
                    textBox.Text = "Введите название средства";

                textBox.Foreground = Brushes.Gray;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp;*.gif)|*.png;*.jpeg;*.jpg;*.bmp;*.gif|All files (*.*)|*.*",
                Title = "Выберите фотографию"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedPhotoPath = dialog.FileName;
                UpdateLargePreview();
                CancelPhotoButton.Visibility = Visibility.Visible;
            }
        }

        private void CancelPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            SetDefaultImage();
            CancelPhotoButton.Visibility = Visibility.Collapsed;
        }

        private void UpdateLargePreview()
        {
            if (string.IsNullOrEmpty(_selectedPhotoPath) || !File.Exists(_selectedPhotoPath))
            {
                SetDefaultImage();
                return;
            }

            try
            {
                var bitmap = new BitmapImage(new Uri(_selectedPhotoPath));
                if (LargePreviewImage != null)
                    LargePreviewImage.Source = bitmap;
            }
            catch
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            if (LargePreviewImage != null)
            {
                LargePreviewImage.Source = new BitmapImage(new Uri("pack://application:,,,/Sourse/Images/Default_Image.png"));
            }
            _selectedPhotoPath = null;
        }

        private void LargePreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPhotoPath) || !File.Exists(_selectedPhotoPath))
            {
                return;
            }

            ShowFullSizeImage();
        }

        private void ShowFullSizeImage()
        {
            var window = new Window
            {
                Title = "Просмотр изображения",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Content = CreateFullScreenContent()
            };
            window.ShowDialog();
        }

        private Grid CreateFullScreenContent()
        {
            var grid = new Grid();

            var image = new Image
            {
                Source = LargePreviewImage?.Source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.MouseDown += (s, e) => ((Window)((Grid)image.Parent).Parent).Close();

            var closeButton = new Button
            {
                Content = "✕",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("BtnActive2style"),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 20, 0),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => ((Window)((Grid)closeButton.Parent).Parent).Close();

            grid.Children.Add(image);
            grid.Children.Add(closeButton);
            return grid;
        }

        private void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            string componentName = ComponentNameTextBox?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(componentName))
            {
                MessageBox.Show("Введите название компонента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_compositionItems.Any(item =>
                item.Equals(componentName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Такой компонент уже добавлен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allergenMatches = FindAllergenMatches(componentName);
            if (allergenMatches.Any())
            {
                bool shouldAdd = ShowAllergyWarning(componentName, allergenMatches);
                if (!shouldAdd)
                {
                    ComponentNameTextBox.Focus();
                    ComponentNameTextBox.SelectAll();
                    return;
                }
            }

            _compositionItems.Add(componentName);
            ComponentNameTextBox.Text = "";
            ComponentNameTextBox.Focus();
        }

        private bool ShowAllergyWarning(string componentName, List<string> matches)
        {
            string recipientInfo = _currentRecipient != null ? $" для {_currentRecipient.Имя}" : " для себя";

            var message = $"ВНИМАНИЕ! Обнаружен возможный аллерген{recipientInfo}!\n\n" +
                          $"Компонент: '{componentName}'\n" +
                          $"Причина: {string.Join("; ", matches)}\n\n" +
                          $"Вы уверены, что хотите добавить этот компонент?";

            var dialogResult = MessageBox.Show(message, "Обнаружен аллерген", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            return dialogResult == MessageBoxResult.Yes;
        }

        private void RemoveComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string componentName)
            {
                var result = MessageBox.Show($"Удалить компонент '{componentName}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _compositionItems.Remove(componentName);
                }
            }
        }

        private void ComponentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            var componentName = textBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(componentName))
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB"));
                textBox.ToolTip = null;
                return;
            }

            var matches = FindAllergenMatches(componentName);
            if (matches.Any())
            {
                textBox.BorderBrush = Brushes.Red;
                textBox.ToolTip = $"Возможный аллерген! {string.Join(", ", matches)}";
            }
            else
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB"));
                textBox.ToolTip = null;
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            PersonalMedArchive medArchive = new PersonalMedArchive(_userId);
            medArchive.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _notificationManager?.Dispose();
            base.OnClosed(e);
        }
    }
}