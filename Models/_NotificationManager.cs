using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;

namespace Курсовая.Models
{
    public class _NotificationManager
    {
        private readonly int _userId;
        private System.Windows.Threading.DispatcherTimer _checkTimer;
        private ConcurrentDictionary<int, DateTime> _notificationHistory = new();

        private const int NOTIFICATION_COOLDOWN_MINUTES = 30;
        private const int CHECK_INTERVAL_SECONDS = 30;

        public event Action<string, string> OnNotification;

        public _NotificationManager(int userId)
        {
            _userId = userId;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _checkTimer = new System.Windows.Threading.DispatcherTimer();
            _checkTimer.Interval = TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS);
            _checkTimer.Tick += (s, e) => CheckAllNotifications();
            _checkTimer.Start();

            CheckAllNotifications();
        }

        public void CheckAllNotifications()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            using var context = new HealthcareManagementContext();

            var recipientIds = context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).Select(r => r.PkIdПолучателя).ToList();

            CheckIntakeNotifications(context, now, currentTime, recipientIds);
            CheckStockNotifications(context, now, recipientIds);
            CheckMeasurementNotifications(context, now, currentTime, recipientIds);
            CheckSymptomNotifications(context, now, currentTime, recipientIds);
        }

        private void CheckIntakeNotifications(HealthcareManagementContext context, DateTime now, TimeSpan currentTime, List<int> recipientIds)
        {
            var medicines = GetActiveItems<Лекарства>(context, recipientIds);

            foreach (var medicine in medicines.Where(m => ShouldShowToday(m, now)))
            {
                foreach (var reminder in medicine.НапоминаниеЛекарстваs)
                {
                    var reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);

                    if (IsTimeMatch(currentTime, reminderTime))
                    {
                        ShowNotificationIfNeeded(
                            medicine.PkIdЛекарства,
                            "medicine",
                            now,
                            () => $"Время принять лекарство!\n" +
                                  $"Название: {medicine.Название}\n" +
                                  $"Дозировка: {medicine.Дозировка} шт.\n" +
                                  $"Способ: {medicine.FkIdСпособаПриёмаNavigation?.Тип}\n" +
                                  $"Для: {GetOwnerName(medicine.FkIdПозицииNavigation)}\n" +
                                  $"Время: {reminder.Часы:00}:{reminder.Минуты:00}",
                            () => MarkAsCompleted(context, "medicine", medicine.PkIdЛекарства, now)
                        );
                    }
                }
            }
        }

        private void CheckStockNotifications(HealthcareManagementContext context, DateTime now, List<int> recipientIds)
        {
            var medicines = GetActiveItems<Лекарства>(context, recipientIds);

            foreach (var medicine in medicines.Where(m => m.НапоминанияЗапас && m.ТекущийЗапас <= m.МинЗапас))
            {
                if (!_notificationHistory.TryGetValue(medicine.PkIdЛекарства, out var lastTime) || (now - lastTime).TotalMinutes >= NOTIFICATION_COOLDOWN_MINUTES)
                {
                    _notificationHistory[medicine.PkIdЛекарства] = now;
                    ShowStockNotification(medicine);
                }
            }
        }

        private void CheckMeasurementNotifications(HealthcareManagementContext context, DateTime now, TimeSpan currentTime, List<int> recipientIds)
        {
            var measurements = GetActiveItems<Измерение>(context, recipientIds);

            foreach (var measurement in measurements.Where(m => ShouldShowToday(m, now)))
            {
                foreach (var reminder in measurement.НапоминаниеИзмеренияs)
                {
                    var reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);

                    if (IsTimeMatch(currentTime, reminderTime))
                    {
                        ShowNotificationIfNeeded(
                            measurement.PkIdИзмерения,
                            "measurement",
                            now,
                            () => $"Время сделать измерение!\n" +
                                  $"Тип: {measurement.FkIdТипИзмеренияNavigation?.Название}\n" +
                                  $"Единица: {measurement.FkIdТипИзмеренияNavigation?.ЕдИзмерения}\n" +
                                  $"Для: {GetOwnerName(measurement.FkIdПозицииNavigation)}\n" +
                                  $"Время: {reminder.Часы:00}:{reminder.Минуты:00}",
                            () => MarkAsCompleted(context, "measurement", measurement.PkIdИзмерения, now)
                        );
                    }
                }
            }
        }

        private void CheckSymptomNotifications(HealthcareManagementContext context, DateTime now, TimeSpan currentTime, List<int> recipientIds)
        {
            var symptoms = GetActiveItems<Симптомы>(context, recipientIds);

            foreach (var symptom in symptoms.Where(s => ShouldShowToday(s, now)))
            {
                foreach (var reminder in symptom.НапоминаниеСимптомыs)
                {
                    var reminderTime = new TimeSpan(reminder.Часы, reminder.Минуты, 0);

                    if (IsTimeMatch(currentTime, reminderTime))
                    {
                        ShowNotificationIfNeeded(
                            symptom.PkIdСимптомы,
                            "symptom",
                            now,
                            () => $"Время отметить симптом!\n" +
                                  $"Симптом: {symptom.Название}\n" +
                                  $"Для: {GetOwnerName(symptom.FkIdПозицииNavigation)}\n" +
                                  $"Время: {reminder.Часы:00}:{reminder.Минуты:00}",
                            () => MarkAsCompleted(context, "symptom", symptom.PkIdСимптомы, now)
                        );
                    }
                }
            }
        }

        private bool IsTimeMatch(TimeSpan currentTime, TimeSpan reminderTime)
        {
            return currentTime.Hours == reminderTime.Hours && currentTime.Minutes == reminderTime.Minutes;
        }

        private List<T> GetActiveItems<T>(HealthcareManagementContext context, List<int> recipientIds) where T : class
        {
            var query = context.Set<T>().Include("FkIdПозицииNavigation").Where(item => EF.Property<ПозицияЗаписи>(item, "FkIdПозицииNavigation").Активность);

            query = query.Where(item => EF.Property<ПозицияЗаписи>(item, "FkIdПозицииNavigation").FkIdПользователя == _userId || (EF.Property<ПозицияЗаписи>(item, "FkIdПозицииNavigation").FkIdПолучателя.HasValue &&
                 recipientIds.Contains(EF.Property<ПозицияЗаписи>(item, "FkIdПозицииNavigation").FkIdПолучателя.Value)));

            if (typeof(T) == typeof(Лекарства))
            {
                query = query.Include("НапоминаниеЛекарстваs").Include("FkIdСпособаПриёмаNavigation");
            }
            else if (typeof(T) == typeof(Измерение))
            {
                query = query.Include("НапоминаниеИзмеренияs").Include("FkIdТипИзмеренияNavigation");
            }
            else if (typeof(T) == typeof(Симптомы))
            {
                query = query.Include("НапоминаниеСимптомыs");
            }

            return query.ToList();
        }

        private void ShowNotificationIfNeeded(int itemId, string type, DateTime now, Func<string> getMessage, Action onConfirm)
        {
            var notificationKey = $"{type}_{itemId}".GetHashCode();

            if (_notificationHistory.TryGetValue(notificationKey, out var lastTime) &&
                (now - lastTime).TotalMinutes < NOTIFICATION_COOLDOWN_MINUTES)
            {
                return;
            }

            using var context = new HealthcareManagementContext();
            bool alreadyCompleted = false;

            switch (type)
            {
                case "medicine":
                    alreadyCompleted = context.ФиксацияПриёмаs.Any(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарства == itemId && f.ДатаПриёма.Date == now.Date);
                    break;
                case "measurement":
                    alreadyCompleted = context.ЗначенияИзмеренияs.Any(f => f.FkIdНапоминанияИзмеренияNavigation.FkIdИзмерения == itemId && f.ДатаЗаписи.Date == now.Date);
                    break;
                case "symptom":
                    alreadyCompleted = context.ЗафиксированныеСимптомыs.Any(f => f.FkIdНапоминанияСимптомыNavigation.FkIdСимптомы == itemId && f.ДатаЗаписи.Date == now.Date);
                    break;
            }

            if (alreadyCompleted)
            {
                return;
            }

            _notificationHistory[notificationKey] = now;
            ShowNotificationWithChoice(type, getMessage(), onConfirm);
        }

        private void ShowNotificationWithChoice(string type, string message, Action onConfirm)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var title = type switch
                {
                    "medicine" => "Напоминание о приеме лекарства",
                    "measurement" => "Напоминание об измерении",
                    "symptom" => "Напоминание о симптоме",
                    _ => "Напоминание"
                };

                OnNotification?.Invoke(title, message);

                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    onConfirm();
                }
            });
        }

        private void ShowStockNotification(Лекарства medicine)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ownerName = GetOwnerName(medicine.FkIdПозицииNavigation);
                var message = $"Пора пополнить запас лекарства!\n" +
                             $"Название: {medicine.Название}\n" +
                             $"Текущий запас: {medicine.ТекущийЗапас} шт.\n" +
                             $"Минимальный: {medicine.МинЗапас} шт.\n" +
                             $"Для: {ownerName}\n\n" +
                             $"Заказать можно на сайте: https://lekvapteke.ru/\n\n" +
                             $"Хотите открыть сайт для заказа?";

                OnNotification?.Invoke("Пополните запас лекарства", message);

                var result = MessageBox.Show(message, "Пополните запас лекарства", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://lekvapteke.ru/",
                        UseShellExecute = true
                    });
                }
            });
        }

        private void MarkAsCompleted(HealthcareManagementContext context, string type, int itemId, DateTime now)
        {
            switch (type)
            {
                case "medicine":
                    var medicineReminder = context.НапоминаниеЛекарстваs.FirstOrDefault(r => r.FkIdЛекарства == itemId);
                    if (medicineReminder != null)
                    {
                        context.ФиксацияПриёмаs.Add(new ФиксацияПриёма
                        {
                            FkIdНапоминанияЛекарства = medicineReminder.PkIdНапоминанияЛекарства,
                            ДатаПриёма = now
                        });
                    }
                    break;

                case "measurement":
                    var measurementReminder = context.НапоминаниеИзмеренияs.FirstOrDefault(r => r.FkIdИзмерения == itemId);
                    if (measurementReminder != null)
                    {
                        context.ЗначенияИзмеренияs.Add(new ЗначенияИзмерения
                        {
                            FkIdНапоминанияИзмерения = measurementReminder.PkIdНапоминанияИзмерения,
                            ДатаЗаписи = now
                        });
                    }
                    break;

                case "symptom":
                    var symptomReminder = context.НапоминаниеСимптомыs.FirstOrDefault(r => r.FkIdСимптомы == itemId);
                    if (symptomReminder != null)
                    {
                        context.ЗафиксированныеСимптомыs.Add(new ЗафиксированныеСимптомы
                        {
                            FkIdНапоминанияСимптомы = symptomReminder.PkIdНапоминанияСимптомы,
                            ДатаЗаписи = now
                        });
                    }
                    break;
            }

            context.SaveChanges();
        }

        private bool ShouldShowToday<T>(T item, DateTime today) where T : class
        {
            ПозицияЗаписи position = null;
            int? periodicityId = null;

            if (item is Лекарства medicine)
            {
                position = medicine.FkIdПозицииNavigation;
                periodicityId = medicine.FkIdПериодичности;
            }
            else if (item is Измерение measurement)
            {
                position = measurement.FkIdПозицииNavigation;
                periodicityId = measurement.FkIdПериодичности;
            }
            else if (item is Симптомы symptom)
            {
                position = symptom.FkIdПозицииNavigation;
                periodicityId = symptom.FkIdПериодичности;
            }

            if (position?.ДатаСоздания == null) return true;

            var periodDays = GetPeriodDays(periodicityId);
            var daysFromStart = (today.Date - position.ДатаСоздания.Date).Days;
            return daysFromStart >= 0 && daysFromStart % periodDays == 0;
        }

        private int GetPeriodDays(int? periodicityId)
        {
            if (!periodicityId.HasValue)
            {
                return 1;
            }

            using var context = new HealthcareManagementContext();
            return context.Периодичностьs.FirstOrDefault(p => p.PkIdПериодичности == periodicityId.Value)?.Период ?? 1;
        }

        private string GetOwnerName(ПозицияЗаписи position)
        {
            if (position?.FkIdПолучателя == null)
            {
                return "Вы";
            }

            using var context = new HealthcareManagementContext();
            return context.ПолучателиУходаs.FirstOrDefault(r => r.PkIdПолучателя == position.FkIdПолучателя)?.Имя ?? "Подопечный";
        }

        public void Dispose()
        {
            _checkTimer?.Stop();
            _checkTimer = null;
            _notificationHistory.Clear();
        }
    }
}