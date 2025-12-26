using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public partial class Medicines : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private ПолучателиУхода? _currentRecipient;
        private string _selectedIcon = "таблетка";
        private Color _selectedColor = (Color)ColorConverter.ConvertFromString("#9A00D7");
        private string _selectedPhotoPath = null;
        private string _selectedFrequency = "daily";
        private int _customDays = 1;
        private bool _regularRemindersEnabled = true;
        private bool _stockRemindersEnabled = false;
        private string _medicineComment = string.Empty;
        private ObservableCollection<string> _compositionItems = new ObservableCollection<string>();
        private System.Collections.Generic.List<TimeSpan> _selectedTimes = new System.Collections.Generic.List<TimeSpan>();

        private _NotificationManager _notificationManager;

        private List<string> _userAllergens = new List<string>();
        private bool _allergensLoaded = false;

        public static readonly DependencyProperty FrequencyHeaderTextProperty = DependencyProperty.Register("FrequencyHeaderText", typeof(string), typeof(Medicines), new PropertyMetadata("Периодичность приема"));

        public static readonly DependencyProperty TimesPerDayHeaderTextProperty = DependencyProperty.Register("TimesPerDayHeaderText", typeof(string), typeof(Medicines), new PropertyMetadata("Сколько раз в день"));

        public static readonly DependencyProperty DosageHeaderTextProperty = DependencyProperty.Register("DosageHeaderText", typeof(string), typeof(Medicines), new PropertyMetadata("Дозировка"));

        public static readonly DependencyProperty StartDateHeaderTextProperty = DependencyProperty.Register("StartDateHeaderText", typeof(string), typeof(Medicines), new PropertyMetadata("Дата начала приёма"));

        public static readonly DependencyProperty EndDateHeaderTextProperty = DependencyProperty.Register("EndDateHeaderText", typeof(string), typeof(Medicines), new PropertyMetadata("Дата окончания приёма"));

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register("PageTitle", typeof(string), typeof(Medicines), new PropertyMetadata("Добавление лекарства"));

        public static readonly DependencyProperty IconDataMapProperty = DependencyProperty.Register("IconDataMap", typeof(Dictionary<string, string>), typeof(Medicines),  new PropertyMetadata(new Dictionary<string, string>()));

        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors", typeof(List<ColorItem>), typeof(Medicines), new PropertyMetadata(new List<ColorItem>()));

        public static readonly DependencyProperty SelectedIconPathProperty = DependencyProperty.Register("SelectedIconPath", typeof(string), typeof(Medicines), new PropertyMetadata(string.Empty));

        public string FrequencyHeaderText
        {
            get => (string)GetValue(FrequencyHeaderTextProperty);
            set => SetValue(FrequencyHeaderTextProperty, value);
        }

        public string TimesPerDayHeaderText
        {
            get => (string)GetValue(TimesPerDayHeaderTextProperty);
            set => SetValue(TimesPerDayHeaderTextProperty, value);
        }

        public string DosageHeaderText
        {
            get => (string)GetValue(DosageHeaderTextProperty);
            set => SetValue(DosageHeaderTextProperty, value);
        }

        public string StartDateHeaderText
        {
            get => (string)GetValue(StartDateHeaderTextProperty);
            set => SetValue(StartDateHeaderTextProperty, value);
        }

        public string EndDateHeaderText
        {
            get => (string)GetValue(EndDateHeaderTextProperty);
            set => SetValue(EndDateHeaderTextProperty, value);
        }

        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public Dictionary<string, string> IconDataMap
        {
            get => (Dictionary<string, string>)GetValue(IconDataMapProperty);
            private set => SetValue(IconDataMapProperty, value);
        }

        public List<ColorItem> AvailableColors
        {
            get => (List<ColorItem>)GetValue(AvailableColorsProperty);
            private set => SetValue(AvailableColorsProperty, value);
        }

        public string SelectedIconPath
        {
            get => (string)GetValue(SelectedIconPathProperty);
            private set => SetValue(SelectedIconPathProperty, value);
        }

        public class ColorItem
        {
            public SolidColorBrush Color { get; set; }
        }

        public Dictionary<string, string> FrequencyTexts => new Dictionary<string, string>
        {
            { "daily", "Каждый день" },
            { "weekly", "Каждую неделю" },
            { "monthly", "Каждый месяц" },
            { "custom", "Настраиваемый" }
        };

        public Dictionary<string, int> FrequencyDays => new Dictionary<string, int>
        {
            { "daily", 1 },
            { "weekly", 7 },
            { "monthly", 30 },
            { "custom", 1 }
        };

        public Dictionary<string, string> TimesPerDayTexts => new Dictionary<string, string>
        {
            { "OncePerDay", "1 раз в день" },
            { "TwicePerDay", "2 раза в день" },
            { "ThreeTimesPerDay", "3 раза в день" },
            { "FourTimesPerDay", "4 раза в день" },
            { "AsNeeded", "По необходимости" }
        };

        public Dictionary<string, int> TimesPerDayCount => new Dictionary<string, int>
        {
            { "OncePerDay", 1 },
            { "TwicePerDay", 2 },
            { "ThreeTimesPerDay", 3 },
            { "FourTimesPerDay", 4 },
            { "AsNeeded", 1 }
        };

        public Dictionary<string, string> IntakeMethods => new Dictionary<string, string>
        {
            { "BeforeMeal", "Перед едой" },
            { "AfterMeal", "После еды" },
            { "DuringMeal", "Во время еды" },
            { "NoMatter", "Не важно" }
        };

        public Medicines(int userId, ПолучателиУхода? recipient = null)
        {
            InitializeComponent();

            _userId = userId;
            _context = new HealthcareManagementContext();
            _currentRecipient = recipient;

            if (_currentRecipient == null)
            {
                PageTitle = "Добавление лекарства для себя";
                BtnReturn.Content = "Для себя: Добавление лекарства";
            }
            else
            {
                PageTitle = $"Добавление лекарства для {_currentRecipient.Имя}";
                BtnReturn.Content = $"Для {_currentRecipient.Имя}: Добавление лекарства";
            }

            CompositionListBox.ItemsSource = _compositionItems;
            InitializeComponents();

            this.DataContext = this;

            UpdateAllHeaders();

            LoadUserAllergens();

            _notificationManager = new _NotificationManager(_userId);
        }

        private async void LoadUserAllergens()
        {
            using (var db = new HealthcareManagementContext())
            {
                _userAllergens.Clear();

                List<string> allergens;

                if (_currentRecipient == null)
                {
                    allergens = await db.Аллергииs.Where(a => a.FkIdПользователя == _userId && a.FkIdПолучателя == null).Select(a => a.Аллерген).Where(a => !string.IsNullOrWhiteSpace(a)).ToListAsync();
                }
                else
                {
                    allergens = await db.Аллергииs.Where(a => a.FkIdПолучателя == _currentRecipient.PkIdПолучателя).Select(a => a.Аллерген).Where(a => !string.IsNullOrWhiteSpace(a)).ToListAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var allergenEntry in allergens)
                    {
                        var allergenName = ExtractAllergenName(allergenEntry);
                        if (!string.IsNullOrWhiteSpace(allergenName) && !_userAllergens.Contains(allergenName))
                        {
                            _userAllergens.Add(allergenName);
                        }
                    }

                    _allergensLoaded = true;
                });
            }
        }

        private string ExtractAllergenName(string allergenEntry)
        {
            if (string.IsNullOrWhiteSpace(allergenEntry))
            {
                return string.Empty;
            }

            allergenEntry = allergenEntry.Trim();

            int dashIndex = allergenEntry.IndexOf('-');
            if (dashIndex > 0)
            {
                string potentialAllergen = allergenEntry.Substring(0, dashIndex).Trim();

                if (dashIndex < allergenEntry.Length - 1)
                {
                    string afterDash = allergenEntry.Substring(dashIndex + 1).Trim();

                    if (afterDash.Length <= 15)
                    {
                        return potentialAllergen.ToLower();
                    }
                }
            }

            return allergenEntry.ToLower();
        }

        private string NormalizeTextForComparison(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.ToLower().Trim();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\w\sа-яё]", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        private List<string> FindAllergenMatches(string componentName)
        {
            var matches = new List<string>();

            if (string.IsNullOrWhiteSpace(componentName) || _userAllergens.Count == 0)
            {
                return matches;
            }

            var componentNormalized = NormalizeTextForComparison(componentName);

            var componentWords = componentNormalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 3).ToList();

            foreach (var allergen in _userAllergens)
            {
                if (string.IsNullOrWhiteSpace(allergen))
                {
                    continue;
                }

                var allergenNormalized = NormalizeTextForComparison(allergen);

                if (componentNormalized == allergenNormalized)
                {
                    matches.Add($"точное совпадение: {allergen}");
                    continue;
                }

                var allergenWords = allergenNormalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 3).ToList();

                foreach (var allergenWord in allergenWords)
                {
                    foreach (var componentWord in componentWords)
                    {
                        if (componentWord == allergenWord)
                        {
                            matches.Add($"совпадает слово: {allergen}");
                            break;
                        }
                    }
                }

                if (allergenNormalized.Length >= 5 && componentNormalized.Contains(allergenNormalized))
                {
                    if (IsWordBoundaryMatch(componentNormalized, allergenNormalized))
                    {
                        matches.Add($"компонент содержит аллерген: {allergen}");
                    }
                }
                else if (componentNormalized.Length >= 5 && allergenNormalized.Contains(componentNormalized))
                {
                    if (IsWordBoundaryMatch(allergenNormalized, componentNormalized))
                    {
                        matches.Add($"компонент является частью аллергена: {allergen}");
                    }
                }
            }

            return matches.Distinct().ToList();
        }

        private bool IsWordBoundaryMatch(string text, string substring)
        {
            int index = text.IndexOf(substring);
            if (index < 0)
            {
                return false;
            }

            bool validStart = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
            bool validEnd = index + substring.Length == text.Length || !char.IsLetterOrDigit(text[index + substring.Length]);

            return validStart && validEnd;
        }

        private void ComponentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var componentName = textBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(componentName) || componentName.Length < 3)
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB"));
                textBox.ToolTip = null;
                return;
            }

            var matches = FindAllergenMatches(componentName);

            var importantMatches = matches.Where(m =>
            {
                if (m.Contains("совпадает слово:"))
                {
                    var parts = m.Split(':');
                    if (parts.Length > 1)
                    {
                        var allergen = parts[1].Trim();
                        var allergenLower = allergen.ToLower();

                        if (allergenLower.Length < 4)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }).ToList();

            if (importantMatches.Any())
            {
                var isExactMatch = importantMatches.Any(m => m.Contains("точное совпадение"));
                textBox.BorderBrush = isExactMatch ? Brushes.Red : Brushes.Orange;

                textBox.ToolTip = $"ВНИМАНИЕ! Обнаружен возможный аллерген!\n\n" +
                                 $"Компонент: '{componentName}'\n" +
                                 $"Совпадения:\n{string.Join("\n", importantMatches.Take(3).Select(m => $"• {m}"))}";

                if (importantMatches.Count > 3)
                {
                    textBox.ToolTip += $"\n... и ещё {importantMatches.Count - 3}";
                }
            }
            else
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB"));
                textBox.ToolTip = null;
            }
        }

        private void InitializeComponents()
        {
            LoadIconData();
            LoadAvailableColors();
            InitializeDefaultSelection();
            UpdatePreviewIcon();
            InitializeDefaultValues();
            InitializeTimeInputs();
            UpdateSelectedIconPath();
        }

        private void LoadIconData()
        {
            if (_context == null)
            {
                IconDataMap = new Dictionary<string, string>();
                return;
            }

            var dict = new Dictionary<string, string>();

            var medicineIcons = _context.ВектораИзображенийs.Where(v => v.FkIdКатегорииОтслеж == 1).ToList();

            foreach (var icon in medicineIcons)
            {
                if (!string.IsNullOrEmpty(icon.Вектор) && !string.IsNullOrEmpty(icon.Название))
                {
                    dict[icon.Название] = icon.Вектор;
                }
            }

            IconDataMap = dict;
        }

        private void LoadAvailableColors()
        {
            if (_context == null)
            {
                AvailableColors = new List<ColorItem>();
                return;
            }

            var colors = new List<ColorItem>();
            var dbColors = _context.ЦветИконкиЛекарствs.ToList();
            foreach (var color in dbColors)
            {
                colors.Add(new ColorItem
                {
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color.Цвет))
                });
            }
            AvailableColors = colors;
        }

        private void UpdateSelectedIconPath()
        {
            if (_context == null)
            {
                SelectedIconPath = string.Empty;
                return;
            }

            var icon = _context.ВектораИзображенийs.FirstOrDefault(i => i.Название == _selectedIcon);
            SelectedIconPath = icon?.Вектор ?? string.Empty;
        }

        private void InitializeDefaultSelection()
        {
            if (AvailableColors.Count > 0)
            {
                _selectedColor = AvailableColors[0].Color.Color;
                UpdatePreviewIcon();
            }
        }

        private void InitializeDefaultValues()
        {
            if (StartDatePicker != null)
            {
                StartDatePicker.SelectedDate = DateTime.Now;
            }

            if (EndDatePicker != null)
            {
                EndDatePicker.SelectedDate = DateTime.Now.AddDays(30);
                EndDatePicker.IsEnabled = true;
            }

            if (StockRemindersToggle != null)
            {
                StockRemindersToggle.IsChecked = false;
                _stockRemindersEnabled = false;

                if (CurrentStockTextBox != null)
                {
                    CurrentStockTextBox.IsEnabled = false;
                    CurrentStockTextBox.Text = "0";
                }

                if (RemindWhenTextBox != null)
                {
                    RemindWhenTextBox.IsEnabled = false;
                    RemindWhenTextBox.Text = "0";
                }
            }

            if (DailyRadio != null)
            {
                DailyRadio.IsChecked = true;
            }

            if (OncePerDay != null)
            {
                OncePerDay.IsChecked = true;
            }

            if (BeforeMeal != null)
            {
                BeforeMeal.IsChecked = true;
            }
        }

        private void InitializeTimeInputs()
        {
            UpdateTimeInputs();
        }

        private void UpdateTimeInputs()
        {
            if (TimeInputsPanel == null)
            {
                return;
            }

            TimeInputsPanel.Children.Clear();
            _selectedTimes.Clear();

            int timesCount = GetTimesCount();

            for (int i = 0; i < timesCount; i++)
            {
                CreateTimeInput(i, timesCount);
            }

            UpdateTimesPerDayHeader();
        }

        private void CreateTimeInput(int index, int totalCount)
        {
            var timeStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var timeLabel = new TextBlock
            {
                Text = $"Время приема {index + 1}:",
                Style = (Style)FindResource("TextDouwn"),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 120
            };

            var timeTextBox = new TextBox
            {
                Name = $"TimeTextBox_{index}",
                Text = GetDefaultTime(index, totalCount),
                Style = (Style)FindResource("RoundedTextBoxStyle"),
                Width = 80,
                Tag = index
            };

            timeTextBox.TextChanged += TimeTextBox_TextChanged;
            timeTextBox.PreviewTextInput += TimeTextBox_PreviewTextInput;

            timeStackPanel.Children.Add(timeLabel);
            timeStackPanel.Children.Add(timeTextBox);
            TimeInputsPanel.Children.Add(timeStackPanel);

            if (TimeSpan.TryParse(timeTextBox.Text, out TimeSpan defaultTime))
            {
                _selectedTimes.Add(defaultTime);
            }
        }

        private int GetTimesCount()
        {
            var radioButtons = new[] { OncePerDay, TwicePerDay, ThreeTimesPerDay, FourTimesPerDay, AsNeeded };
            var checkedRadio = radioButtons.FirstOrDefault(r => r?.IsChecked == true);

            return checkedRadio != null ? TimesPerDayCount.GetValueOrDefault(checkedRadio.Name, 1) : 1;
        }

        private string GetDefaultTime(int index, int totalCount)
        {
            int baseHour = 8;
            int interval = 12 / Math.Max(totalCount, 1);
            int hour = (baseHour + (index * interval)) % 24;
            return $"{hour:00}:00";
        }

        private void UpdateAllHeaders()
        {
            UpdateFrequencyHeader();
            UpdateTimesPerDayHeader();
            UpdateDosageHeader();
            UpdateStartDateHeader();
            UpdateEndDateHeader();
        }

        private void UpdateFrequencyHeader()
        {
            string frequencyText = GetFrequencyText();
            FrequencyHeaderText = $"Периодичность приема: {frequencyText}";
        }

        private void UpdateTimesPerDayHeader()
        {
            string timesText = GetTimesPerDayText();

            if (_selectedTimes.Any() && AsNeeded?.IsChecked != true)
            {
                var timeStrings = _selectedTimes.Select(t => t.ToString(@"hh\:mm"));
                timesText += $" ({string.Join(", ", timeStrings)})";
            }

            TimesPerDayHeaderText = $"Сколько раз в день: {timesText}";
        }

        private void UpdateDosageHeader()
        {
            string dosage = DosageTextBox?.Text ?? "1";
            string method = GetIntakeMethodText();
            DosageHeaderText = $"Дозировка: {dosage} шт., {method}";
        }

        private void UpdateStartDateHeader()
        {
            string dateText = StartDatePicker?.SelectedDate?.ToString("dd.MM.yyyy") ?? "не указана";
            StartDateHeaderText = $"Дата начала: {dateText}";
        }

        private void UpdateEndDateHeader()
        {
            string dateText = NoEndDateCheckBox?.IsChecked == true ? "не указана" : EndDatePicker?.SelectedDate?.ToString("dd.MM.yyyy") ?? "не указана";
            EndDateHeaderText = $"Дата окончания: {dateText}";
        }

        private string GetFrequencyText()
        {
            if (_selectedFrequency == "custom")
            {
                return $"Раз в {_customDays} дней";
            }

            return FrequencyTexts.GetValueOrDefault(_selectedFrequency, "Каждый день");
        }

        private string GetTimesPerDayText()
        {
            var radioButtons = new[] { OncePerDay, TwicePerDay, ThreeTimesPerDay, FourTimesPerDay, AsNeeded };
            var checkedRadio = radioButtons.FirstOrDefault(r => r?.IsChecked == true);

            return checkedRadio != null ? TimesPerDayTexts.GetValueOrDefault(checkedRadio.Name, "1 раз в день") : "1 раз в день";
        }

        private string GetIntakeMethodText()
        {
            var radioButtons = new[] { BeforeMeal, AfterMeal, DuringMeal, NoMatter };
            var checkedRadio = radioButtons.FirstOrDefault(r => r?.IsChecked == true);

            return checkedRadio != null ? IntakeMethods.GetValueOrDefault(checkedRadio.Name, "Перед едой") : "Перед едой";
        }

        private void FrequencyRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                _selectedFrequency = radioButton.Tag?.ToString() ?? "daily";
                if (CustomFrequencyPanel != null)
                    CustomFrequencyPanel.Visibility = _selectedFrequency == "custom" ? Visibility.Visible : Visibility.Collapsed;
                UpdateFrequencyHeader();
            }
        }

        private void TimesPerDayRadio_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTimeInputs();
            if (TimeSelectionPanel != null)
            {
                TimeSelectionPanel.Visibility = AsNeeded?.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void IntakeMethodRadio_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDosageHeader();
        }

        private void CustomDaysTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(CustomDaysTextBox?.Text, out int days) && days > 0)
            {
                _customDays = days;
                if (_selectedFrequency == "custom")
                {
                    UpdateFrequencyHeader();
                }
            }
        }

        private void DosageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDosageHeader();
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            if (newText.Length > 5)
            {
                e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.Text[0]) && e.Text != ":")
            {
                e.Handled = true;
                return;
            }

            if (textBox.Text.Length == 2 && e.Text != ":")
            {
                textBox.Text += ":";
                textBox.CaretIndex = 3;
            }
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && int.TryParse(textBox.Tag?.ToString(), out int index))
            {
                bool isValid = TimeSpan.TryParse(textBox.Text, out TimeSpan time) && time >= TimeSpan.Zero && time < TimeSpan.FromHours(24);

                textBox.BorderBrush = isValid ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB")) : Brushes.Red;

                if (isValid)
                {
                    if (_selectedTimes.Count > index)
                    {
                        _selectedTimes[index] = time;
                    }
                    else
                    {
                        _selectedTimes.Add(time);
                    }
                }

                UpdateTimesPerDayHeader();
            }
        }

        private void NoEndDateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (EndDatePicker != null)
            {
                EndDatePicker.IsEnabled = false;
                EndDatePicker.SelectedDate = null;
            }
            UpdateEndDateHeader();
        }

        private void NoEndDateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (EndDatePicker != null)
            {
                EndDatePicker.IsEnabled = true;
                if (EndDatePicker.SelectedDate == null)
                {
                    EndDatePicker.SelectedDate = DateTime.Now.AddDays(30);
                }
            }
            UpdateEndDateHeader();
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStartDateHeader();
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEndDateHeader();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return text.All(c => char.IsDigit(c));
        }

        private async void SaveMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            if (!await ValidateAllComponentsForAllergiesAsync())
            {
                return;
            }

            await SaveMedicineDataAsync();
            NavigateToHome();
        }


        private async Task<bool> ValidateAllComponentsForAllergiesAsync()
        {
            if (_compositionItems.Count == 0 || _userAllergens.Count == 0)
            {
                return true;
            }

            var problematicComponents = new List<string>();

            foreach (var component in _compositionItems)
            {
                var matches = FindAllergenMatches(component);
                if (matches.Any())
                {
                    problematicComponents.Add($"{component} ({string.Join(", ", matches)})");
                }
            }

            if (problematicComponents.Any())
            {
                string recipientInfo = _currentRecipient != null ? $" для {_currentRecipient.Имя}" : " для себя";

                var message = $"ВНИМАНИЕ! В составе лекарства обнаружены возможные аллергены{recipientInfo}:\n\n" +
                              $"{string.Join("\n", problematicComponents.Select(c => $"• {c}"))}\n\n" +
                              $"Вы уверены, что хотите продолжить сохранение?";

                var tcs = new TaskCompletionSource<MessageBoxResult>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show(message, "Обнаружены аллергены", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    tcs.SetResult(result);
                });

                var result = await tcs.Task;
                return result == MessageBoxResult.Yes;
            }

            return true;
        }

        private bool ValidateInputs()
        {
            string medicineName = GetTextBoxValue(BoxNameMed);
            if (string.IsNullOrWhiteSpace(medicineName))
            {
                ShowError("Введите название лекарства");
                return false;
            }

            string medicinePurpose = GetTextBoxValue(BoxForMed);
            if (string.IsNullOrWhiteSpace(medicinePurpose))
            {
                ShowError("Введите назначение лекарства");
                return false;
            }

            if (AsNeeded?.IsChecked != true && _selectedTimes.Any(time => time < TimeSpan.Zero || time >= TimeSpan.FromHours(24)))
            {
                ShowError("Пожалуйста, введите корректное время в формате HH:MM");
                return false;
            }

            if (!int.TryParse(DosageTextBox?.Text ?? "1", out int dosage) || dosage <= 0)
            {
                ShowError("Введите корректное количество");
                return false;
            }

            if (_stockRemindersEnabled)
            {
                if (!int.TryParse(CurrentStockTextBox?.Text ?? "0", out int currentStock) || currentStock < 0)
                {
                    ShowError("Введите корректное текущее количество");
                    return false;
                }

                if (!int.TryParse(RemindWhenTextBox?.Text ?? "0", out int remindWhen) || remindWhen < 0)
                {
                    ShowError("Введите корректное значение для напоминания");
                    return false;
                }

                if (remindWhen >= currentStock)
                {
                    ShowError("Значение напоминания должно быть меньше текущего запаса");
                    return false;
                }
            }

            string comment = GetTextBoxValue(CommentTextBox);
            if (comment.Length > 500)
            {
                ShowError("Комментарий не должен превышать 500 символов");
                return false;
            }

            return true;
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            return textBox?.Foreground == Brushes.Gray ? "" : textBox?.Text ?? "";
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private int GetFrequencyDays()
        {
            if (_selectedFrequency == "custom")
            {
                return CustomDaysTextBox != null && int.TryParse(CustomDaysTextBox.Text, out int days) ? days : 1;
            }

            return FrequencyDays.GetValueOrDefault(_selectedFrequency, 1);
        }
        private async Task<string> SaveMedicinePhotoAsync(string sourcePath, string medicineName)
        {
            string appPhotosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MedicinePhotos");
            if (!Directory.Exists(appPhotosDir))
            {
                Directory.CreateDirectory(appPhotosDir);
            }

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"{medicineName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            string destinationPath = Path.Combine(appPhotosDir, fileName);

            using (FileStream sourceStream = File.Open(sourcePath, FileMode.Open))
            using (FileStream destinationStream = File.Create(destinationPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            return $"MedicinePhotos/{fileName}";
        }
        private async Task SaveMedicineDataAsync()
        {
            string medicineName = GetTextBoxValue(BoxNameMed);
            string medicinePurpose = GetTextBoxValue(BoxForMed);

            using (var db = new HealthcareManagementContext())
            {
                var vector = await db.ВектораИзображенийs.FirstOrDefaultAsync(v => v.Название == _selectedIcon);

                if (vector == null)
                {
                    vector = new ВектораИзображений
                    {
                        Название = _selectedIcon,
                        Вектор = SelectedIconPath
                    };
                    db.ВектораИзображенийs.Add(vector);
                    await db.SaveChangesAsync();
                }

                var colorHex = _selectedColor.ToString();
                var iconColor = await db.ЦветИконкиЛекарствs.FirstOrDefaultAsync(c => c.Цвет == colorHex);

                if (iconColor == null)
                {
                    iconColor = new ЦветИконкиЛекарств { Цвет = colorHex };
                    db.ЦветИконкиЛекарствs.Add(iconColor);
                    await db.SaveChangesAsync();
                }

                var icon = new ИконкиЛекарств
                {
                    FkIdВектора = vector.PkIdВектора,
                    FkIdЦветИконки = iconColor.PkIdЦветИконки
                };
                db.ИконкиЛекарствs.Add(icon);
                await db.SaveChangesAsync();

                var periodicityName = GetFrequencyText();
                var periodicity = await db.Периодичностьs.FirstOrDefaultAsync(p => p.Название == periodicityName);

                if (periodicity == null)
                {
                    periodicity = new Периодичность
                    {
                        Название = periodicityName,
                        Период = GetFrequencyDays()
                    };
                    db.Периодичностьs.Add(periodicity);
                    await db.SaveChangesAsync();
                }

                var intakeMethodName = GetIntakeMethodText();
                var intakeMethod = await db.СпособыПриёмаs.FirstOrDefaultAsync(m => m.Тип == intakeMethodName);

                if (intakeMethod == null)
                {
                    intakeMethod = new СпособыПриёма { Тип = intakeMethodName };
                    db.СпособыПриёмаs.Add(intakeMethod);
                    await db.SaveChangesAsync();
                }

                var category = await db.КатегорииОтслеживанияs.FirstOrDefaultAsync(c => c.Тип == "Лекарства");

                if (category == null)
                {
                    category = new КатегорииОтслеживания { Тип = "Лекарства" };
                    db.КатегорииОтслеживанияs.Add(category);
                    await db.SaveChangesAsync();
                }

                string photoPath = null;
                if (!string.IsNullOrEmpty(_selectedPhotoPath) && File.Exists(_selectedPhotoPath))
                {
                    photoPath = await SaveMedicinePhotoAsync(_selectedPhotoPath, medicineName);
                }

                var position = new ПозицияЗаписи
                {
                    FkIdПользователя = _userId,
                    FkIdКатегорииОтслеж = category.PkIdКатегорииОтслеж,
                    Активность = true,
                    ДатаСоздания = DateTime.Now
                };

                if (_currentRecipient != null)
                {
                    position.FkIdПолучателя = _currentRecipient.PkIdПолучателя;
                }

                db.ПозицияЗаписиs.Add(position);
                await db.SaveChangesAsync();

                var medicine = new Лекарства
                {
                    Название = medicineName,
                    Назначение = medicinePurpose,
                    Комментарий = _medicineComment,
                    Фото = photoPath,
                    ПриёмовВДень = _selectedTimes.Count,
                    Дозировка = int.Parse(DosageTextBox?.Text ?? "1"),
                    НапоминанияЗапас = _stockRemindersEnabled,
                    ТекущийЗапас = _stockRemindersEnabled ? int.Parse(CurrentStockTextBox?.Text ?? "0") : 0,
                    МинЗапас = _stockRemindersEnabled ? int.Parse(RemindWhenTextBox?.Text ?? "0") : 0,
                    FkIdСпособаПриёма = intakeMethod.PkIdСпособаПриёма,
                    FkIdПозиции = position.PkIdПозиции,
                    FkIdИконки = icon.PkIdИконки,
                    FkIdПериодичности = periodicity.PkIdПериодичности
                };
                db.Лекарстваs.Add(medicine);
                await db.SaveChangesAsync();

                foreach (var component in _compositionItems)
                {
                    var составЛекарства = await db.СоставЛекарстваs.FirstOrDefaultAsync(s => s.НазваниеСоставляющей == component);

                    if (составЛекарства == null)
                    {
                        составЛекарства = new СоставЛекарства
                        {
                            НазваниеСоставляющей = component
                        };
                        db.СоставЛекарстваs.Add(составЛекарства);
                        await db.SaveChangesAsync();
                    }

                    var составВЛекарстве = new СоставВЛекарстве
                    {
                        FkIdЛекарства = medicine.PkIdЛекарства,
                        FkIdСоставаЛекарства = составЛекарства.PkIdСостава
                    };
                    db.СоставВЛекарствеs.Add(составВЛекарстве);
                }

                if (_selectedTimes.Count > 0)
                {
                    foreach (var time in _selectedTimes)
                    {
                        var reminder = new НапоминаниеЛекарства
                        {
                            FkIdЛекарства = medicine.PkIdЛекарства,
                            Часы = time.Hours,
                            Минуты = time.Minutes
                        };
                        db.НапоминаниеЛекарстваs.Add(reminder);
                    }
                }

                await db.SaveChangesAsync();

                string recipientInfo = _currentRecipient != null ? $" для {_currentRecipient.Имя}" : " для вас";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Лекарство успешно сохранено{recipientInfo}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        private void CommentTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void CommentTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Text.Length > 500)
                {
                    textBox.BorderBrush = Brushes.Red;
                    textBox.ToolTip = "Превышена максимальная длина комментария (500 символов)";
                }
                else
                {
                    textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABABAB"));
                    textBox.ToolTip = null;
                }

                _medicineComment = textBox.Foreground == Brushes.Gray ? "" : textBox.Text;
            }
        }

        private void NavigateToHome()
        {
            new Home(_userId).Show();
            Close();
        }

        private void UpdatePreviewIcon()
        {
            if (PreviewIconPath == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SelectedIconPath))
            {
                PreviewIconPath.Data = Geometry.Parse(SelectedIconPath);
            }
            else
            {
                PreviewIconPath.Data = Geometry.Parse("M 20 50 L 80 50 M 50 20 L 50 80");
            }

            PreviewIconPath.Fill = new SolidColorBrush(_selectedColor);

            var colorAnimation = new ColorAnimation
            {
                To = _selectedColor,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (PreviewIconPath.Fill is SolidColorBrush brush)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border iconBorder = sender as Border;
            if (iconBorder?.Tag != null)
            {
                _selectedIcon = iconBorder.Tag.ToString();
                UpdateSelectedIconPath();
                UpdatePreviewIcon();
            }
        }

        private void Color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border colorBorder = sender as Border;
            if (colorBorder?.Tag is ColorItem colorItem)
            {
                _selectedColor = colorItem.Color.Color;
                UpdatePreviewIcon();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "BoxNameMed")
                {
                    textBox.Text = "Введите название лекарства";
                }
                else if (textBox.Name == "BoxForMed")
                {
                    textBox.Text = "От/Для чего лекарство: Например, от головной боли";
                }

                textBox.Foreground = Brushes.Gray;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp;*.gif)|*.png;*.jpeg;*.jpg;*.bmp;*.gif|All files (*.*)|*.*",
                Title = "Выберите фотографию"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedPhotoPath = openFileDialog.FileName;
                UpdateLargePreview();
                if (CancelPhotoButton != null)
                {
                    CancelPhotoButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void CancelPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            SetDefaultImage();
            if (CancelPhotoButton != null)
            {
                CancelPhotoButton.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateLargePreview()
        {
            if (!string.IsNullOrEmpty(_selectedPhotoPath) && File.Exists(_selectedPhotoPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedPhotoPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    if (LargePreviewImage != null)
                        LargePreviewImage.Source = bitmap;
                }
                catch
                {
                    SetDefaultImage();
                }
            }
            else
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

        private async void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            string componentName = ComponentNameTextBox?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(componentName))
            {
                MessageBox.Show("Введите название компонента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_compositionItems.Any(item => item.Equals(componentName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Такой компонент уже добавлен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allergenMatches = FindAllergenMatches(componentName);
            if (allergenMatches.Any())
            {
                bool shouldAdd = await ShowAllergyWarningAsync(componentName, allergenMatches);
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

        private async Task<bool> ShowAllergyWarningAsync(string componentName, List<string> matches)
        {
            string recipientInfo = _currentRecipient != null ? $" для {_currentRecipient.Имя}" : " для себя";

            var message = $"ВНИМАНИЕ! Обнаружен возможный аллерген{recipientInfo}!\n\n" +
                          $"Компонент: '{componentName}'\n" +
                          $"Причина: {string.Join("; ", matches)}\n\n" +
                          $"Вы уверены, что хотите добавить этот компонент?";

            bool result = false;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialogResult = MessageBox.Show(message, "Обнаружен аллерген", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                result = dialogResult == MessageBoxResult.Yes;
            });

            return result;
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

        private void ShowFullSizeImage()
        {
            var fullImageWindow = new Window
            {
                Title = "Просмотр изображения",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black
            };

            var imageControl = new Image
            {
                Source = LargePreviewImage?.Source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

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

            closeButton.Click += (s, e) => fullImageWindow.Close();

            var grid = new Grid();
            grid.Children.Add(imageControl);
            grid.Children.Add(closeButton);

            fullImageWindow.Content = grid;

            imageControl.MouseDown += (s, e) => fullImageWindow.Close();

            fullImageWindow.ShowDialog();
        }

        private void StockRemindersToggle_Changed(object sender, RoutedEventArgs e)
        {
            _stockRemindersEnabled = StockRemindersToggle.IsChecked == true;

            if (CurrentStockTextBox != null)
            {
                CurrentStockTextBox.IsEnabled = _stockRemindersEnabled;
                if (!_stockRemindersEnabled)
                {
                    CurrentStockTextBox.Text = "0";
                }
            }

            if (RemindWhenTextBox != null)
            {
                RemindWhenTextBox.IsEnabled = _stockRemindersEnabled;
                if (!_stockRemindersEnabled)
                {
                    RemindWhenTextBox.Text = "0";
                }
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Home h = new Home(_userId);
            h.Show();
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