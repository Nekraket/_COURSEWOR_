using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class MainCalendar : Window
    {
        private HealthcareManagementContext _context;
        private DateTime _selectedDate;
        private int _userId;
        private List<int> _careRecipientIds = new List<int>();

        private _NotificationManager _notificationManager;

        private List<Лекарства> _medicines = new List<Лекарства>();
        private List<Измерение> _measurements = new List<Измерение>();
        private List<Симптомы> _symptoms = new List<Симптомы>();
        private List<ФиксацияПриёма> _completedMedicines = new List<ФиксацияПриёма>();
        private List<ЗначенияИзмерения> _completedMeasurements = new List<ЗначенияИзмерения>();
        private List<ЗафиксированныеСимптомы> _completedSymptoms = new List<ЗафиксированныеСимптомы>();

        public MainCalendar(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _selectedDate = DateTime.Today;

            Loaded += MainCalendar_Loaded;

            _notificationManager = new _NotificationManager(_userId);
        }

        private void MainCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
            _context = new HealthcareManagementContext();
            LoadCareRecipients();
            LoadAllData();
            InitializeCalendar();

            CalendarControl.SelectedDate = DateTime.Today;
            UpdateSelectedDateInfo();
        }

        private void LoadCareRecipients()
        {
            _careRecipientIds = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).Select(r => r.PkIdПолучателя).ToList();
        }

        private void LoadAllData()
        {
            _medicines.Clear();
            _measurements.Clear();
            _symptoms.Clear();
            _completedMedicines.Clear();
            _completedMeasurements.Clear();
            _completedSymptoms.Clear();

            var startDate = DateTime.Today.AddMonths(-1);
            var endDate = DateTime.Today.AddMonths(2);

            _medicines = _context.Лекарстваs
                .Include(m => m.FkIdПериодичностиNavigation)
                .Include(m => m.НапоминаниеЛекарстваs)
                .Include(m => m.FkIdПозицииNavigation)
                .Include(m => m.FkIdСпособаПриёмаNavigation)
                .Where(m => m.FkIdПозицииNavigation != null &&
                           m.FkIdПозицииNavigation.Активность == true &&
                           (m.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (m.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(m.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();

            _completedMedicines = _context.ФиксацияПриёмаs
                .Include(f => f.FkIdНапоминанияЛекарстваNavigation).ThenInclude(r => r.FkIdЛекарстваNavigation)
                .Where(f => f.ДатаПриёма >= startDate &&
                           f.ДатаПриёма <= endDate &&
                           (f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();

            _measurements = _context.Измерениеs
                .Include(m => m.FkIdТипИзмеренияNavigation)
                .Include(m => m.НапоминаниеИзмеренияs)
                .Include(m => m.FkIdПозицииNavigation)
                .Include(m => m.FkIdПериодичностиNavigation)
                .Where(m => m.FkIdПозицииNavigation != null &&
                           m.FkIdПозицииNavigation.Активность == true &&
                           (m.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (m.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(m.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();

            _completedMeasurements = _context.ЗначенияИзмеренияs
                .Include(z => z.FkIdНапоминанияИзмеренияNavigation).ThenInclude(r => r.FkIdИзмеренияNavigation)
                .Where(z => z.ДатаЗаписи >= startDate &&
                           z.ДатаЗаписи <= endDate &&
                           (z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();

            _symptoms = _context.Симптомыs
                .Include(s => s.НапоминаниеСимптомыs)
                .Include(s => s.FkIdПозицииNavigation)
                .Include(s => s.FkIdПериодичностиNavigation)
                .Where(s => s.FkIdПозицииNavigation != null &&
                           s.FkIdПозицииNavigation.Активность == true &&
                           (s.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (s.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(s.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();

            _completedSymptoms = _context.ЗафиксированныеСимптомыs
                .Include(z => z.FkIdНапоминанияСимптомыNavigation).ThenInclude(r => r.FkIdСимптомыNavigation)
                .Where(z => z.ДатаЗаписи >= startDate &&
                           z.ДатаЗаписи <= endDate &&
                           (z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                            (z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation.FkIdПолучателя != null &&
                             _careRecipientIds.Contains(z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation.FkIdПолучателя.Value))))
                .ToList();
        }

        private int GetPeriodDays(int? periodicityId)
        {
            if (!periodicityId.HasValue)
            {
                return 1;
            }

            var period = _context.Периодичностьs.FirstOrDefault(p => p.PkIdПериодичности == periodicityId.Value);
            return period?.Период ?? 1;
        }

        private string GetOwnerName(int? positionUserId, int? recipientId)
        {
            if (recipientId.HasValue)
            {
                var recipient = _context.ПолучателиУходаs.FirstOrDefault(r => r.PkIdПолучателя == recipientId.Value);
                return recipient?.Имя ?? "Подопечный";
            }

            if (positionUserId == _userId)
            {
                return "Вы";
            }

            return "Неизвестно";
        }

        private void InitializeCalendar()
        {
            UpdateCalendarHeader();
            UpdateSelectedDateInfo();
        }

        private void UpdateCalendarHeader()
        {
            MonthYearText.Text = DateTime.Today.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
        }

        private void UpdateSelectedDateInfo()
        {
            _selectedDate = CalendarControl.SelectedDate ?? DateTime.Today;
            DayHeaderText.Text = _selectedDate.ToString("dddd, dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));

            DisplayEventsForSelectedDate();
        }

        private void DisplayEventsForSelectedDate()
        {
            DisplayPendingTasks();
            DisplayCompletedTasks();
        }

        private void DisplayPendingTasks()
        {
            PendingTasksPanel.Children.Clear();

            var pendingMedicines = GetPendingMedicinesForDate(_selectedDate);
            var pendingMeasurements = GetPendingMeasurementsForDate(_selectedDate);
            var pendingSymptoms = GetPendingSymptomsForDate(_selectedDate);

            if (!pendingMedicines.Any() && !pendingMeasurements.Any() && !pendingSymptoms.Any())
            {
                NoPendingTasksText.Text = "На выбранную дату нет ожидающих задач";
                NoPendingTasksText.Visibility = Visibility.Visible;
                return;
            }

            NoPendingTasksText.Visibility = Visibility.Collapsed;

            foreach (var medicine in pendingMedicines)
            {
                AddPendingTaskItem(
                    medicine,
                    "Medicine",
                    $"Приём лекарства: {medicine.Название}",
                    $"Дозировка: {medicine.Дозировка ?? 1}, Способ: {medicine.FkIdСпособаПриёмаNavigation?.Тип ?? "Не указан"}",
                    "#4CAF50",
                    medicine.FkIdПозицииNavigation
                );
            }

            foreach (var measurement in pendingMeasurements)
            {
                AddPendingTaskItem(
                    measurement,
                    "Measurement",
                    $"Измерение: {measurement.FkIdТипИзмеренияNavigation?.Название ?? "Неизвестно"}",
                    $"Единица измерения: {measurement.FkIdТипИзмеренияNavigation?.ЕдИзмерения ?? "Не указана"}",
                    "#2196F3",
                    measurement.FkIdПозицииNavigation
                );
            }

            foreach (var symptom in pendingSymptoms)
            {
                AddPendingTaskItem(
                    symptom,
                    "Symptom",
                    $"Отслеживание: {symptom.Название}",
                    "Оценка самочувствия и заметки",
                    "#FF9800",
                    symptom.FkIdПозицииNavigation
                );
            }
        }

        private void DisplayCompletedTasks()
        {
            CompletedTasksPanel.Children.Clear();

            var completedMedicines = GetCompletedMedicinesForDate(_selectedDate);
            var completedMeasurements = GetCompletedMeasurementsForDate(_selectedDate);
            var completedSymptoms = GetCompletedSymptomsForDate(_selectedDate);

            if (!completedMedicines.Any() && !completedMeasurements.Any() && !completedSymptoms.Any())
            {
                NoCompletedTasksText.Text = "На выбранную дату нет завершённых задач";
                NoCompletedTasksText.Visibility = Visibility.Visible;
                return;
            }

            NoCompletedTasksText.Visibility = Visibility.Collapsed;

            foreach (var medicineRecord in completedMedicines)
            {
                var medicine = medicineRecord.FkIdНапоминанияЛекарстваNavigation?.FkIdЛекарстваNavigation;
                if (medicine != null)
                {
                    AddCompletedTaskItem(
                        medicineRecord,
                        "Medicine",
                        $"Принято лекарство: {medicine.Название}",
                        $"Время приёма: {medicineRecord.ДатаПриёма:HH:mm}",
                        medicine.FkIdПозицииNavigation,
                        medicineRecord.ДатаПриёма
                    );
                }
            }

            foreach (var measurementRecord in completedMeasurements)
            {
                var measurement = measurementRecord.FkIdНапоминанияИзмеренияNavigation?.FkIdИзмеренияNavigation;
                if (measurement != null)
                {
                    AddCompletedTaskItem(
                        measurementRecord,
                        "Measurement",
                        $"Измерение выполнено: {measurement.FkIdТипИзмеренияNavigation?.Название ?? "Неизвестно"}",
                        $"Значение: {measurementRecord.Значение:F1} {measurement.FkIdТипИзмеренияNavigation?.ЕдИзмерения ?? ""}",
                        measurement.FkIdПозицииNavigation,
                        measurementRecord.ДатаЗаписи
                    );
                }
            }

            foreach (var symptomRecord in completedSymptoms)
            {
                var symptom = symptomRecord.FkIdНапоминанияСимптомыNavigation?.FkIdСимптомыNavigation;
                if (symptom != null)
                {
                    AddCompletedTaskItem(
                        symptomRecord,
                        "Symptom",
                        $"Симптом зафиксирован: {symptom.Название}",
                        $"Оценка самочувствия: {symptomRecord.ОценкаСамочувствия ?? 0}/10",
                        symptom.FkIdПозицииNavigation,
                        symptomRecord.ДатаЗаписи
                    );
                }
            }
        }

        private List<Лекарства> GetPendingMedicinesForDate(DateTime date)
        {
            var result = new List<Лекарства>();

            foreach (var medicine in _medicines)
            {
                var periodDays = GetPeriodDays(medicine.FkIdПериодичности);
                var startDate = medicine.FkIdПозицииNavigation?.ДатаСоздания ?? DateTime.Today;

                if (IsEventDate(date, startDate, periodDays, medicine.ДлительностьПриёма))
                {
                    var isCompleted = _completedMedicines.Any(c => c.FkIdНапоминанияЛекарстваNavigation?.FkIdЛекарстваNavigation?.PkIdЛекарства == medicine.PkIdЛекарства && c.ДатаПриёма.Date == date.Date);

                    if (!isCompleted)
                    {
                        result.Add(medicine);
                    }
                }
            }

            return result;
        }

        private List<Измерение> GetPendingMeasurementsForDate(DateTime date)
        {
            var result = new List<Измерение>();

            foreach (var measurement in _measurements)
            {
                var periodDays = GetPeriodDays(measurement.FkIdПериодичности);
                var startDate = measurement.FkIdПозицииNavigation?.ДатаСоздания ?? DateTime.Today;

                if (IsEventDate(date, startDate, periodDays, null))
                {
                    var isCompleted = _completedMeasurements.Any(c => c.FkIdНапоминанияИзмеренияNavigation?.FkIdИзмеренияNavigation?.PkIdИзмерения == measurement.PkIdИзмерения && c.ДатаЗаписи.Date == date.Date);

                    if (!isCompleted)
                    {
                        result.Add(measurement);
                    }
                }
            }

            return result;
        }

        private List<Симптомы> GetPendingSymptomsForDate(DateTime date)
        {
            var result = new List<Симптомы>();

            foreach (var symptom in _symptoms)
            {
                var periodDays = GetPeriodDays(symptom.FkIdПериодичности);
                var startDate = symptom.FkIdПозицииNavigation?.ДатаСоздания ?? DateTime.Today;

                if (IsEventDate(date, startDate, periodDays, null))
                {
                    var isCompleted = _completedSymptoms.Any(c =>
                        c.FkIdНапоминанияСимптомыNavigation?.FkIdСимптомыNavigation?.PkIdСимптомы == symptom.PkIdСимптомы &&
                        c.ДатаЗаписи.Date == date.Date);

                    if (!isCompleted)
                    {
                        result.Add(symptom);
                    }
                }
            }

            return result;
        }

        private List<ФиксацияПриёма> GetCompletedMedicinesForDate(DateTime date)
        {
            return _completedMedicines.Where(c => c.ДатаПриёма.Date == date.Date).ToList();
        }

        private List<ЗначенияИзмерения> GetCompletedMeasurementsForDate(DateTime date)
        {
            return _completedMeasurements.Where(c => c.ДатаЗаписи.Date == date.Date).ToList();
        }

        private List<ЗафиксированныеСимптомы> GetCompletedSymptomsForDate(DateTime date)
        {
            return _completedSymptoms.Where(c => c.ДатаЗаписи.Date == date.Date).ToList();
        }

        private bool IsEventDate(DateTime targetDate, DateTime startDate, int periodDays, int? duration)
        {
            if (targetDate < startDate.Date)
            {
                return false;
            }

            if (duration.HasValue && targetDate > startDate.AddDays(duration.Value).Date)
            {
                return false;
            }

            var daysDiff = (targetDate.Date - startDate.Date).Days;
            return daysDiff % periodDays == 0;
        }

        private void AddPendingTaskItem(object entity, string type, string title, string description, string color, ПозицияЗаписи position)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color + "20")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                Tag = new { Entity = entity, Type = type }
            };

            var stackPanel = new StackPanel();

            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stackPanel.Children.Add(titleTextBlock);

            var timeOwnerStack = new StackPanel { Orientation = Orientation.Horizontal };
            var ownerName = GetOwnerName(position?.FkIdПользователя, position?.FkIdПолучателя);

            TimeSpan reminderTime = TimeSpan.Zero;
            if (type == "Medicine" && entity is Лекарства medicine && medicine.НапоминаниеЛекарстваs.Any())
            {
                var reminder = medicine.НапоминаниеЛекарстваs.First();
                reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);
            }
            else if (type == "Measurement" && entity is Измерение measurement && measurement.НапоминаниеИзмеренияs.Any())
            {
                var reminder = measurement.НапоминаниеИзмеренияs.First();
                reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);
            }
            else if (type == "Symptom" && entity is Симптомы symptom && symptom.НапоминаниеСимптомыs.Any())
            {
                var reminder = symptom.НапоминаниеСимптомыs.First();
                reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);
            }

            timeOwnerStack.Children.Add(new TextBlock
            {
                Text = $"Время: {reminderTime.ToString(@"hh\:mm")}",
                FontSize = 12,
                Foreground = Brushes.Gray
            });

            timeOwnerStack.Children.Add(new TextBlock
            {
                Text = $" | Владелец: {ownerName}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(5, 0, 0, 0)
            });

            stackPanel.Children.Add(timeOwnerStack);

            if (!string.IsNullOrEmpty(description))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = description,
                    FontSize = 12,
                    Foreground = Brushes.DarkGray,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            border.Child = stackPanel;
            PendingTasksPanel.Children.Add(border);
        }

        private void AddCompletedTaskItem(object record, string type, string title, string description, ПозицияЗаписи position, DateTime completionTime)
        {
            var border = new Border
            {
                Background = Brushes.WhiteSmoke,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                Tag = new { Record = record, Type = type }
            };

            var stackPanel = new StackPanel();

            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DarkGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stackPanel.Children.Add(titleTextBlock);

            var timeOwnerStack = new StackPanel { Orientation = Orientation.Horizontal };
            var ownerName = GetOwnerName(position?.FkIdПользователя, position?.FkIdПолучателя);

            timeOwnerStack.Children.Add(new TextBlock
            {
                Text = $"Выполнено: {completionTime:HH:mm}",
                FontSize = 12,
                Foreground = Brushes.Gray
            });

            timeOwnerStack.Children.Add(new TextBlock
            {
                Text = $" | Владелец: {ownerName}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(5, 0, 0, 0)
            });

            stackPanel.Children.Add(timeOwnerStack);

            if (!string.IsNullOrEmpty(description))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = description,
                    FontSize = 12,
                    Foreground = Brushes.DarkGray,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            border.Child = stackPanel;
            CompletedTasksPanel.Children.Add(border);
        }

        private void TasksTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                UpdateSelectedDateInfo();
            }
        }

        private void CalendarControl_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedDateInfo();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Home h = new Home(_userId);
            h.Show();
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