using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class MoodAndSymptoms : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private ПолучателиУхода? _currentRecipient;
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

        public static readonly DependencyProperty FrequencyHeaderTextProperty = DependencyProperty.Register("FrequencyHeaderText", typeof(string), typeof(MoodAndSymptoms), new PropertyMetadata("Периодичность"));

        public static readonly DependencyProperty VectorPathDataProperty = DependencyProperty.Register("VectorPathData", typeof(string), typeof(MoodAndSymptoms), new PropertyMetadata(""));

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register("PageTitle", typeof(string), typeof(MoodAndSymptoms), new PropertyMetadata(""));

        public string FrequencyHeaderText
        {
            get => (string)GetValue(FrequencyHeaderTextProperty);
            set => SetValue(FrequencyHeaderTextProperty, value);
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

        public MoodAndSymptoms(int userId, ПолучателиУхода? recipient = null)
        {
            InitializeComponent();

            _userId = userId;
            _context = new HealthcareManagementContext();
            _currentRecipient = recipient;

            DataContext = this;

            InitializePageTitle();
            LoadDataFromDatabase();

            TimeCardsItemsControl.ItemsSource = _timeCards;
            InitializeComponents();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void InitializePageTitle()
        {
            if (_currentRecipient == null)
            {
                PageTitle = $"Добавление настроения или симптома для себя";
                BtnReturn.Content = $"Для себя: Настроения и симптомы";
            }
            else
            {
                PageTitle = $"Добавление настроения или симптома для {_currentRecipient.Имя}";
                BtnReturn.Content = $"Для {_currentRecipient.Имя}: Настроения и симптомы";
            }
        }

        private void LoadDataFromDatabase()
        {
            if (_context == null)
            {
                return;
            }

            var vector = _context.ВектораИзображенийs.FirstOrDefault(v => v.Название == "СимптомыГлавное");
            if (vector == null)
            {
                MessageBox.Show("Векторное изображение 'СимптомыГлавное' не найдено в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            VectorPathData = vector.Вектор;

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

                    UpdateTimeCardVisualStates(id, true);
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

                        UpdateTimeCardVisualStates(id, false);
                    }
                }
            }
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

        private void UpdateTimeCardVisualStates(int id, bool isEditMode)
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSymptomsData())
            {
                new Home(_userId).Show();
                Close();
            }
        }

        private bool SaveSymptomsData()
        {
            if (_context == null)
            {
                MessageBox.Show("Ошибка подключения к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SymptomNameTextBox.Text))
            {
                MessageBox.Show("Введите название симптома/настроения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            var category = _context.КатегорииОтслеживанияs.FirstOrDefault(c => c.Тип == "Симптомы");

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

            var symptom = new Симптомы
            {
                FkIdПозиции = position.PkIdПозиции,
                FkIdПериодичности = periodicityToUse.PkIdПериодичности,
                Название = SymptomNameTextBox.Text.Trim(),
                ЗаписейВДень = _timeCards.Count
            };
            _context.Симптомыs.Add(symptom);
            _context.SaveChanges();

            foreach (var timeCard in _timeCards)
            {
                var reminder = new НапоминаниеСимптомы
                {
                    FkIdСимптомы = symptom.PkIdСимптомы,
                    Часы = timeCard.Time.Hours,
                    Минуты = timeCard.Time.Minutes
                };
                _context.НапоминаниеСимптомыs.Add(reminder);
            }

            _context.SaveChanges();

            string successMessage = _currentRecipient != null
                ? $"Напоминания для '{SymptomNameTextBox.Text}' настроены для {_currentRecipient.Имя} успешно!" : $"Напоминания для '{SymptomNameTextBox.Text}' настроены для вас успешно!";

            MessageBox.Show(successMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private void VectorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                var animation = Application.Current.TryFindResource("JellyAnimation") as System.Windows.Media.Animation.Storyboard;

                var animationCopy = animation.Clone();
                System.Windows.Media.Animation.Storyboard.SetTarget(animationCopy, border);
                animationCopy.Begin();
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