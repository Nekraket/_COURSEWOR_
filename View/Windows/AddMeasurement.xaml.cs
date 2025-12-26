using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class AddMeasurement : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private ПолучателиУхода? _currentRecipient;
        private string _measurementName;
        private ТипИзмерения? _measurementType;
        private Периодичность? _selectedPeriodicity;
        private int _timeCardIdCounter = 0;

        private _NotificationManager _notificationManager;

        private class TimeCardInfo
        {
            public int Id { get; set; }
            public TimeSpan Time { get; set; }
            public string EditHours { get; set; } = "";
            public string EditMinutes { get; set; } = "";
            public bool IsEditMode { get; set; }
        }

        private ObservableCollection<TimeCardInfo> _timeCards = new ObservableCollection<TimeCardInfo>();

        public static readonly DependencyProperty FrequencyHeaderTextProperty = DependencyProperty.Register("FrequencyHeaderText", typeof(string), typeof(AddMeasurement), new PropertyMetadata("Периодичность"));

        public static readonly DependencyProperty MeasurementUnitsProperty = DependencyProperty.Register("MeasurementUnits", typeof(string), typeof(AddMeasurement), new PropertyMetadata(""));

        public static readonly DependencyProperty VectorPathDataProperty = DependencyProperty.Register("VectorPathData", typeof(string), typeof(AddMeasurement), new PropertyMetadata(""));

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register("PageTitle", typeof(string), typeof(AddMeasurement), new PropertyMetadata(""));

        public string FrequencyHeaderText
        {
            get => (string)GetValue(FrequencyHeaderTextProperty);
            set => SetValue(FrequencyHeaderTextProperty, value);
        }

        public string MeasurementUnits
        {
            get => (string)GetValue(MeasurementUnitsProperty);
            set => SetValue(MeasurementUnitsProperty, value);
        }

        public string VectorPathData
        {
            get => (string)GetValue(VectorPathDataProperty);
            set => SetValue(VectorPathDataProperty, value);
        }

        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public AddMeasurement(int userId, ПолучателиУхода? recipient, string measurementName)
        {
            InitializeComponent();

            _userId = userId;
            _context = new HealthcareManagementContext();
            _currentRecipient = recipient;
            _measurementName = measurementName;

            DataContext = this;

            InitializePageTitle();
            LoadDataFromDatabase();

            InitializeComponents();

            _notificationManager = new _NotificationManager(_userId);
            _notificationManager.OnNotification += (title, message) =>
            {
                Console.WriteLine($"Уведомление: {title} - {message}");
            };
        }

        private void InitializePageTitle()
        {
            if (_currentRecipient == null)
            {
                PageTitle = $"Добавление для себя: {_measurementName}";
                BtnReturn.Content = $"Для себя: {_measurementName}";
            }
            else
            {
                PageTitle = $"Добавление для {_currentRecipient.Имя}: {_measurementName}";
                BtnReturn.Content = $"Для {_currentRecipient.Имя}: {_measurementName}";
            }
        }

        private void LoadDataFromDatabase()
        {
            if (_context == null)
            {
                return;
            }

            _measurementType = _context.ТипИзмеренияs.FirstOrDefault(t => t.Название == _measurementName);

            if (_measurementType == null)
            {
                MessageBox.Show($"Тип измерения '{_measurementName}' не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            MeasurementUnits = _measurementType.ЕдИзмерения;

            var vector = _context.ВектораИзображенийs.FirstOrDefault(v => v.Название == _measurementName);

            if (vector != null)
            {
                VectorPathData = vector.Вектор;
            }
            else
            {
                VectorPathData = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z";
            }

            _selectedPeriodicity = _context.Периодичностьs.FirstOrDefault(p => p.Период == 1);
        }

        private void InitializeComponents()
        {
            DailyRadio.IsChecked = true;
            UpdateFrequencyHeader();
            InitializeTimeCards();
        }

        private void InitializeTimeCards()
        {
            _timeCards.Clear();
            AddTimeCard(new TimeSpan(9, 0, 0));
            TimeCardsItemsControl.ItemsSource = _timeCards;
        }

        private void AddTimeCard(TimeSpan time)
        {
            var timeCard = new TimeCardInfo
            {
                Id = _timeCardIdCounter++,
                Time = time,
                EditHours = time.Hours.ToString("00"),
                EditMinutes = time.Minutes.ToString("00"),
                IsEditMode = false
            };
            _timeCards.Add(timeCard);
            SortTimeCards();
        }

        private void SortTimeCards()
        {
            var sortedCards = _timeCards.OrderBy(tc => tc.Time).ToList();
            _timeCards.Clear();
            foreach (var card in sortedCards)
            {
                _timeCards.Add(card);
            }
        }

        private void UpdateFrequencyHeader()
        {
            if (_selectedPeriodicity != null)
            {
                FrequencyHeaderText = $"Периодичность: {_selectedPeriodicity.Название}";
            }
        }

        private void FrequencyRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                switch (radioButton.Tag?.ToString())
                {
                    case "daily":
                        _selectedPeriodicity = _context.Периодичностьs.FirstOrDefault(p => p.Название == "Ежедневно");
                        break;
                    case "weekly":
                        _selectedPeriodicity = _context.Периодичностьs.FirstOrDefault(p => p.Название == "Еженедельно");
                        break;
                    case "monthly":
                        _selectedPeriodicity = _context.Периодичностьs.FirstOrDefault(p => p.Название == "Ежемесячно");
                        break;
                    case "custom":
                        CustomFrequencyPanel.Visibility = Visibility.Visible;
                        FrequencyHeaderText = "Периодичность: Пользовательская";
                        return;
                }

                CustomFrequencyPanel.Visibility = Visibility.Collapsed;
                UpdateFrequencyHeader();
            }
        }

        private void CustomDaysTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(CustomDaysTextBox?.Text, out int days) && days > 0)
            {
                FrequencyHeaderText = $"Периодичность: Раз в {days} дней";
            }
        }

        private void HoursTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void MinutesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void EditTimeCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is int id)
            {
                var timeCard = _timeCards.FirstOrDefault(tc => tc.Id == id);
                if (timeCard != null)
                {
                    timeCard.EditHours = timeCard.Time.Hours.ToString("00");
                    timeCard.EditMinutes = timeCard.Time.Minutes.ToString("00");
                    timeCard.IsEditMode = true;

                    UpdateTimeCardVisibility(id, true);
                }
            }
        }

        private void SaveTimeCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is int id)
            {
                var timeCard = _timeCards.FirstOrDefault(tc => tc.Id == id);
                if (timeCard != null)
                {
                    if (ValidateAndUpdateTime(timeCard))
                    {
                        timeCard.IsEditMode = false;
                        SortTimeCards();

                        UpdateTimeCardVisibility(id, false);
                    }
                }
            }
        }

        private void UpdateTimeCardVisibility(int id, bool isEditMode)
        {
            foreach (var item in TimeCardsItemsControl.Items)
            {
                if (item is TimeCardInfo timeCard && timeCard.Id == id)
                {
                    var container = TimeCardsItemsControl.ItemContainerGenerator.ContainerFromItem(item);
                    if (container != null)
                    {
                        var border = FindVisualChild<Border>(container, null);
                        if (border != null)
                        {
                            var viewModePanel = FindVisualChild<StackPanel>(border, "ViewModePanel");
                            var editModePanel = FindVisualChild<StackPanel>(border, "EditModePanel");

                            if (viewModePanel != null)
                            {
                                viewModePanel.Visibility = isEditMode ? Visibility.Collapsed : Visibility.Visible;
                            }

                            if (editModePanel != null)
                            {
                                editModePanel.Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                    }
                    break;
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                {
                    if (childName == null || (child is FrameworkElement fe && fe.Name == childName))
                    {
                        return result;
                    }
                }

                var descendant = FindVisualChild<T>(child, childName);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        private bool ValidateAndUpdateTime(TimeCardInfo timeCard)
        {
            if (int.TryParse(timeCard.EditHours, out int hours) &&
                int.TryParse(timeCard.EditMinutes, out int minutes))
            {
                if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59)
                {
                    timeCard.Time = new TimeSpan(hours, minutes, 0);
                    return true;
                }
            }

            MessageBox.Show("Введите корректное время (часы: 0-23, минуты: 0-59)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void DeleteTimeCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                var timeCardToRemove = _timeCards.FirstOrDefault(tc => tc.Id == id);
                if (timeCardToRemove != null)
                {
                    _timeCards.Remove(timeCardToRemove);
                }
            }
        }

        private void AddTimeCard_Click(object sender, RoutedEventArgs e)
        {
            var maxTime = _timeCards.Any() ? _timeCards.Max(tc => tc.Time) : new TimeSpan(8, 0, 0);
            var newTime = maxTime.Add(TimeSpan.FromHours(1));

            if (newTime >= TimeSpan.FromHours(24))
            {
                newTime = new TimeSpan(8, 0, 0);
            }
            AddTimeCard(newTime);
        }

        private void SaveMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveMeasurementData())
            {
                new Home(_userId).Show();
                Close();
            }
        }

        private bool SaveMeasurementData()
        {
            if (_context == null)
            {
                MessageBox.Show("Ошибка подключения к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_measurementType == null)
            {
                MessageBox.Show("Тип измерения не определен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_selectedPeriodicity == null)
            {
                MessageBox.Show("Периодичность не определена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Периодичность periodicityToUse = _selectedPeriodicity;

            if (CustomFrequencyPanel.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(CustomDaysTextBox.Text) || !int.TryParse(CustomDaysTextBox.Text, out int customDays) || customDays <= 0)
                {
                    MessageBox.Show("Введите корректное количество дней", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                periodicityToUse = _context.Периодичностьs.FirstOrDefault(p => p.Период == customDays);

                if (periodicityToUse == null)
                {
                    periodicityToUse = new Периодичность
                    {
                        Название = $"Каждые {customDays} дней",
                        Период = customDays
                    };
                    _context.Периодичностьs.Add(periodicityToUse);
                    _context.SaveChanges();
                }
            }

            var category = _context.КатегорииОтслеживанияs.FirstOrDefault(c => c.Тип == "Измерения");

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

            _context.ПозицияЗаписиs.Add(position);
            _context.SaveChanges();

            var vector = _context.ВектораИзображенийs.FirstOrDefault(v => v.Название == _measurementName);

            if (vector == null)
            {
                vector = new ВектораИзображений
                {
                    Название = _measurementName,
                    Вектор = string.IsNullOrEmpty(VectorPathData) ? "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z" : VectorPathData,
                    FkIdКатегорииОтслеж = category.PkIdКатегорииОтслеж
                };
                _context.ВектораИзображенийs.Add(vector);
                _context.SaveChanges();
            }

            var measurement = new Измерение
            {
                FkIdПозиции = position.PkIdПозиции,
                FkIdТипИзмерения = _measurementType.PkIdТипИзмерения,
                FkIdПериодичности = periodicityToUse.PkIdПериодичности,
                ЗаписейВДень = _timeCards.Count
            };
            _context.Измерениеs.Add(measurement);
            _context.SaveChanges();

            foreach (var timeCard in _timeCards)
            {
                var reminder = new НапоминаниеИзмерения
                {
                    FkIdИзмерения = measurement.PkIdИзмерения,
                    Часы = timeCard.Time.Hours,
                    Минуты = timeCard.Time.Minutes
                };
                _context.НапоминаниеИзмеренияs.Add(reminder);
            }

            _context.SaveChanges();

            string successMessage = _currentRecipient != null
                ? $"Напоминания для '{_measurementName}' настроены для {_currentRecipient.Имя} успешно!" : $"Напоминания для '{_measurementName}' настроены для вас успешно!";

            MessageBox.Show(successMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Measurement m = new Measurement(_userId);
            m.Show();
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