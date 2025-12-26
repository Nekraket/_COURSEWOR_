using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;
using Курсовая.Models;
using Курсовая.View.Pages;
using Курсовая.View.UserControls;

namespace Курсовая.View.Windows
{
    public partial class Home : Window
    {
        private readonly int _userId;
        private HealthcareManagementContext _context;
        private _NotificationManager _notificationManager;

        private object _selectedRecord;
        private object _selectedReminder;
        private string _selectedRecordType;
        private HomeItemCheck _selectedCard;

        public Home(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();

            InitializeDetailsPage();
            LoadUserData();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void InitializeDetailsPage()
        {
            var detailsPage = new HomeDetailsPage();
            DetailsFrame.Content = detailsPage;

            detailsPage.DeleteRequested += DetailsPage_DeleteRequested;
            detailsPage.ChangeRecipientRequested += DetailsPage_ChangeRecipientRequested;
            detailsPage.ReminderTimeChanged += DetailsPage_ReminderTimeChanged;
            detailsPage.StockSettingsChanged += DetailsPage_StockSettingsChanged;

            detailsPage.Initialize(_userId, _context);
        }

        private void LoadUserData()
        {
            LoadActiveTasks();
            LoadCompletedTasks();
        }

        private void LoadActiveTasks()
        {
            ClearPanels();

            var recipientIds = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).Select(r => r.PkIdПолучателя).ToList();

            var today = DateTime.Today;

            var medicines = _context.Лекарстваs
                .Include(l => l.FkIdПозицииNavigation)
                .Include(l => l.FkIdИконкиNavigation).ThenInclude(i => i.FkIdЦветИконкиNavigation)
                .Include(l => l.FkIdИконкиNavigation).ThenInclude(i => i.FkIdВектораNavigation)
                .Include(l => l.НапоминаниеЛекарстваs)
                .Include(l => l.FkIdСпособаПриёмаNavigation)
                .Where(l => l.FkIdПозицииNavigation.Активность)
                .ToList()
                .Where(l => l.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          (l.FkIdПозицииNavigation.FkIdПолучателя.HasValue &&
                           recipientIds.Contains(l.FkIdПозицииNavigation.FkIdПолучателя.Value)))
                .ToList();

            foreach (var medicine in medicines)
            {
                if (!ShouldShowToday(medicine))
                {
                    continue;
                }

                var completedIds = _context.ФиксацияПриёмаs.Where(f => f.ДатаПриёма.Date == today && f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарства == medicine.PkIdЛекарства)
                    .Select(f => f.FkIdНапоминанияЛекарстваNavigation.PkIdНапоминанияЛекарства).ToList();

                foreach (var reminder in medicine.НапоминаниеЛекарстваs)
                {
                    if (!completedIds.Contains(reminder.PkIdНапоминанияЛекарства))
                    {
                        CreateMedicineCard(medicine, reminder);
                    }
                }
            }

            var measurements = _context.Измерениеs
                .Include(m => m.FkIdПозицииNavigation)
                .Include(m => m.FkIdТипИзмеренияNavigation).ThenInclude(t => t.FkIdВекторNavigation)
                .Include(m => m.НапоминаниеИзмеренияs)
                .Where(m => m.FkIdПозицииNavigation.Активность)
                .ToList()
                .Where(m => m.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          (m.FkIdПозицииNavigation.FkIdПолучателя.HasValue &&
                           recipientIds.Contains(m.FkIdПозицииNavigation.FkIdПолучателя.Value)))
                .ToList();

            foreach (var measurement in measurements)
            {
                if (!ShouldShowToday(measurement))
                {
                    continue;
                }

                var completedIds = _context.ЗначенияИзмеренияs.Where(z => z.ДатаЗаписи.Date == today && z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмерения == measurement.PkIdИзмерения)
                    .Select(z => z.FkIdНапоминанияИзмеренияNavigation.PkIdНапоминанияИзмерения).ToList();

                foreach (var reminder in measurement.НапоминаниеИзмеренияs)
                {
                    if (!completedIds.Contains(reminder.PkIdНапоминанияИзмерения))
                    {
                        CreateMeasurementCard(measurement, reminder);
                    }
                }
            }

            var symptoms = _context.Симптомыs
                .Include(s => s.FkIdПозицииNavigation)
                .Include(s => s.НапоминаниеСимптомыs)
                .Where(s => s.FkIdПозицииNavigation.Активность)
                .ToList()
                .Where(s => s.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          (s.FkIdПозицииNavigation.FkIdПолучателя.HasValue &&
                           recipientIds.Contains(s.FkIdПозицииNavigation.FkIdПолучателя.Value)))
                .ToList();

            foreach (var symptom in symptoms)
            {
                if (!ShouldShowToday(symptom))
                {
                    continue;
                }

                var completedIds = _context.ЗафиксированныеСимптомыs.Where(z => z.ДатаЗаписи.Date == today && z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомы == symptom.PkIdСимптомы)
                    .Select(z => z.FkIdНапоминанияСимптомыNavigation.PkIdНапоминанияСимптомы).ToList();

                foreach (var reminder in symptom.НапоминаниеСимптомыs)
                {
                    if (!completedIds.Contains(reminder.PkIdНапоминанияСимптомы))
                    {
                        CreateSymptomCard(symptom, reminder);
                    }
                }
            }
        }

        private bool ShouldShowToday(object item)
        {
            DateTime? startDate = null;
            int? periodDays = null;
            int? durationDays = null;

            if (item is Лекарства medicine)
            {
                startDate = medicine.FkIdПозицииNavigation?.ДатаСоздания;
                periodDays = GetPeriodDays(medicine.FkIdПериодичности);
                durationDays = medicine.ДлительностьПриёма;
            }
            else if (item is Измерение measurement)
            {
                startDate = measurement.FkIdПозицииNavigation?.ДатаСоздания;
                periodDays = GetPeriodDays(measurement.FkIdПериодичности);
            }
            else if (item is Симптомы symptom)
            {
                startDate = symptom.FkIdПозицииNavigation?.ДатаСоздания;
                periodDays = GetPeriodDays(symptom.FkIdПериодичности);
            }

            if (!startDate.HasValue || !periodDays.HasValue) return true;

            var daysFromStart = (DateTime.Today - startDate.Value.Date).Days;
            if (daysFromStart < 0)
            {
                return false;
            }
            if (daysFromStart % periodDays.Value != 0)
            {
                return false;
            }
            if (durationDays.HasValue && daysFromStart > durationDays.Value)
            {
                return false;
            }

            return true;
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

        private void CreateMedicineCard(Лекарства medicine, НапоминаниеЛекарства reminder)
        {
            var ownerName = GetOwnerName(medicine.FkIdПозицииNavigation);
            var iconInfo = GetMedicineIconInfo(medicine);

            var description = $"Дозировка: {medicine.Дозировка} мг | " + $"Способ: {medicine.FkIdСпособаПриёмаNavigation?.Тип} | " + $"Запас: {medicine.ТекущийЗапас}/{medicine.МинЗапас} шт.";

            var homeCheck = CreateHomeCheck(
                medicine.Название,
                description,
                $"{reminder.Часы:00}:{reminder.Минуты:00}",
                ownerName,
                iconInfo.pathData,
                iconInfo.iconColor,
                false);

            homeCheck.Tag = Tuple.Create<object, object>(medicine, reminder);
            homeCheck.MouseDown += (s, e) => SelectItem(medicine, reminder, homeCheck, "Medicine");
            homeCheck.CompleteTaskClicked += (sender, e) => CompleteMedicine(medicine, homeCheck, reminder);

            MedicinesStackPanel.Children.Add(homeCheck);
        }

        private void CreateMeasurementCard(Измерение measurement, НапоминаниеИзмерения reminder)
        {
            var ownerName = GetOwnerName(measurement.FkIdПозицииNavigation);
            var type = measurement.FkIdТипИзмеренияNavigation;

            var description = $"Тип: {type?.Название} | " + $"Ед. измерения: {type?.ЕдИзмерения} | " + $"Записей в день: {measurement.ЗаписейВДень}";

            string iconPath = null;
            if (type?.FkIdВекторNavigation != null)
            {
                iconPath = type.FkIdВекторNavigation.Вектор;
            }
            else
            {
                iconPath = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
            }

            var homeCheck = CreateHomeCheck(
                type?.Название ?? "Измерение",
                description,
                $"{reminder.Часы:00}:{reminder.Минуты:00}",
                ownerName,
                iconPath,
                "#000000",
                false);

            homeCheck.Tag = Tuple.Create<object, object>(measurement, reminder);
            homeCheck.MouseDown += (s, e) => SelectItem(measurement, reminder, homeCheck, "Measurement");
            homeCheck.CompleteTaskClicked += (sender, e) => CompleteMeasurement(measurement, homeCheck, reminder);

            MeasurementsStackPanel.Children.Add(homeCheck);
        }

        private void CreateSymptomCard(Симптомы symptom, НапоминаниеСимптомы reminder)
        {
            var ownerName = GetOwnerName(symptom.FkIdПозицииNavigation);
            var iconPath = GetSymptomIconPath();

            var homeCheck = CreateHomeCheck(
                symptom.Название,
                $"Записей в день: {symptom.ЗаписейВДень}",
                $"{reminder.Часы:00}:{reminder.Минуты:00}",
                ownerName,
                iconPath,
                "#000000",
                false);

            homeCheck.Tag = Tuple.Create<object, object>(symptom, reminder);
            homeCheck.MouseDown += (s, e) => SelectItem(symptom, reminder, homeCheck, "Symptom");
            homeCheck.CompleteTaskClicked += (sender, e) => CompleteSymptom(symptom, homeCheck, reminder);

            MoodStackPanel.Children.Add(homeCheck);
        }

        private string GetSymptomIconPath()
        {
            var symptomVector = _context.ВектораИзображенийs.FirstOrDefault(v => v.Название.ToLower().Contains("симптом") || v.Название.ToLower().Contains("symptom"));

            if (symptomVector != null && !string.IsNullOrEmpty(symptomVector.Вектор))
            {
                return symptomVector.Вектор;
            }

            return "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
        }

        private HomeItemCheck CreateHomeCheck(string title, string description, string time, string owner, string iconPath, string iconColor, bool isCompleted)
        {
            return new HomeItemCheck
            {
                TitleText = title,
                DescriptionText = description,
                TimeText = time,
                OwnerText = owner ?? "",
                IconPathData = iconPath,
                IconColor = iconColor,
                IsCompleted = isCompleted,
                IsCompletionEnabled = !isCompleted
            };
        }

        private void SelectItem(object record, object reminder, HomeItemCheck card, string recordType)
        {
            _selectedRecord = record;
            _selectedReminder = reminder;
            _selectedRecordType = recordType;
            _selectedCard = card;

            HomeDetailsPage detailsPage = DetailsFrame.Content as HomeDetailsPage;
            if (detailsPage != null)
            {
                detailsPage.ShowDetails(record, reminder, recordType);
            }
        }

        private void CompleteMedicine(Лекарства medicine, HomeItemCheck homeCheck, НапоминаниеЛекарства reminder)
        {
            var intake = new ФиксацияПриёма
            {
                ДатаПриёма = DateTime.Now,
                FkIdНапоминанияЛекарства = reminder?.PkIdНапоминанияЛекарства
            };
            _context.ФиксацияПриёмаs.Add(intake);

            medicine.ТекущийЗапас = Math.Max(0, medicine.ТекущийЗапас - (int)medicine.Дозировка);

            _context.SaveChanges();
            MedicinesStackPanel.Children.Remove(homeCheck);
            ClearDetails();
            LoadCompletedTasks();

            MessageBox.Show($"Приём лекарства '{medicine.Название}' выполнен", "Выполнено");
        }

        private void CompleteMeasurement(Измерение measurement, HomeItemCheck homeCheck, НапоминаниеИзмерения reminder)
        {
            var type = measurement.FkIdТипИзмеренияNavigation;
            ValueInputDialog dialog = new ValueInputDialog( title: "Ввод значения", description: $"Измерение: {type?.Название}", unit: type?.ЕдИзмерения ?? "ед.")
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.Result.HasValue)
            {
                var record = new ЗначенияИзмерения
                {
                    ДатаЗаписи = DateTime.Now,
                    Значение = dialog.Result.Value,
                    FkIdНапоминанияИзмерения = reminder?.PkIdНапоминанияИзмерения
                };

                _context.ЗначенияИзмеренияs.Add(record);
                _context.SaveChanges();

                MeasurementsStackPanel.Children.Remove(homeCheck);
                ClearDetails();
                LoadCompletedTasks();

                MessageBox.Show($"Измерение записано: {dialog.Result.Value} {type?.ЕдИзмерения}", "Записано");
            }
        }

        private void CompleteSymptom(Симптомы symptom, HomeItemCheck homeCheck, НапоминаниеСимптомы reminder)
        {
            SymptomRatingDialog dialog = new SymptomRatingDialog(symptom.Название)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                var record = new ЗафиксированныеСимптомы
                {
                    ДатаЗаписи = DateTime.Now,
                    ОценкаСамочувствия = dialog.WellbeingRating,
                    Заметка = dialog.Notes,
                    FkIdНапоминанияСимптомы = reminder?.PkIdНапоминанияСимптомы
                };

                _context.ЗафиксированныеСимптомыs.Add(record);
                _context.SaveChanges();

                MoodStackPanel.Children.Remove(homeCheck);
                ClearDetails();
                LoadCompletedTasks();

                MessageBox.Show($"Симптом '{symptom.Название}' зафиксирован", "Зафиксировано");
            }
        }

        private void LoadCompletedTasks()
        {
            CompletedTasksStackPanel.Children.Clear();

            var recipientIds = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).Select(r => r.PkIdПолучателя).ToList();

            var today = DateTime.Today;

            var completedItems = new List<object>();

            var intakes = _context.ФиксацияПриёмаs
                .Include(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation)
                .Include(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdИконкиNavigation)
                .Where(f => f.ДатаПриёма.Date == today)
                .ToList()
                .Where(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          recipientIds.Contains(f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation.FkIdПолучателя ?? -1))
                .ToList();

            completedItems.AddRange(intakes);

            var measurements = _context.ЗначенияИзмеренияs
                .Include(z => z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdТипИзмеренияNavigation)
                .Include(z => z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation)
                .Where(z => z.ДатаЗаписи.Date == today)
                .ToList()
                .Where(z => z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          recipientIds.Contains(z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdПозицииNavigation.FkIdПолучателя ?? -1))
                .ToList();

            completedItems.AddRange(measurements);

            var symptoms = _context.ЗафиксированныеСимптомыs
                .Include(z => z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation)
                .Where(z => z.ДатаЗаписи.Date == today)
                .ToList()
                .Where(z => z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation.FkIdПользователя == _userId ||
                          recipientIds.Contains(z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомыNavigation.FkIdПозицииNavigation.FkIdПолучателя ?? -1))
                .ToList();

            completedItems.AddRange(symptoms);

            foreach (var item in completedItems.OrderByDescending(GetDateTime))
            {
                if (item is ФиксацияПриёма intake)
                {
                    AddCompletedMedicineCard(intake);
                }
                else if (item is ЗначенияИзмерения measurementValue)
                {
                    AddCompletedMeasurementCard(measurementValue);
                }
                else if (item is ЗафиксированныеСимптомы symptomRecord)
                {
                    AddCompletedSymptomCard(symptomRecord);
                }
            }
        }

        private DateTime GetDateTime(object item)
        {
            return item switch
            {
                ФиксацияПриёма intake => intake.ДатаПриёма,
                ЗначенияИзмерения measurement => measurement.ДатаЗаписи,
                ЗафиксированныеСимптомы symptom => symptom.ДатаЗаписи,
                _ => DateTime.MinValue
            };
        }

        private void AddCompletedMedicineCard(ФиксацияПриёма intake)
        {
            var medicine = intake.FkIdНапоминанияЛекарстваNavigation?.FkIdЛекарстваNavigation;
            var ownerName = GetOwnerName(medicine?.FkIdПозицииNavigation);
            var iconInfo = GetMedicineIconInfo(medicine);

            var homeCheck = CreateHomeCheck(
                medicine?.Название ?? "Лекарство",
                $"Время: {intake.ДатаПриёма:HH:mm} | Дозировка: {medicine?.Дозировка} шт.",
                "Выполнено",
                ownerName,
                iconInfo.pathData,
                iconInfo.iconColor,
                true);

            homeCheck.MouseDoubleClick += (s, e) => SelectCompletedItem(intake, homeCheck, "CompletedMedicine");
            CompletedTasksStackPanel.Children.Add(homeCheck);
        }

        private void AddCompletedMeasurementCard(ЗначенияИзмерения measurementValue)
        {
            var measurementEntity = measurementValue.FkIdНапоминанияИзмеренияNavigation?.FkIdИзмеренияNavigation;
            var type = measurementEntity?.FkIdТипИзмеренияNavigation;
            var ownerName = GetOwnerName(measurementEntity?.FkIdПозицииNavigation);

            string iconPath = null;
            if (type?.FkIdВекторNavigation != null)
            {
                iconPath = type.FkIdВекторNavigation.Вектор;
            }
            else
            {
                iconPath = GetSymptomIconPath();
            }

            var homeCheck = CreateHomeCheck(
                type?.Название ?? "Измерение",
                $"Значение: {measurementValue.Значение} {type?.ЕдИзмерения} | Время: {measurementValue.ДатаЗаписи:HH:mm}",
                "Выполнено",
                ownerName,
                iconPath,
                "#000000",
                true);

            homeCheck.MouseDoubleClick += (s, e) => SelectCompletedItem(measurementValue, homeCheck, "CompletedMeasurement");
            CompletedTasksStackPanel.Children.Add(homeCheck);
        }

        private void AddCompletedSymptomCard(ЗафиксированныеСимптомы symptomRecord)
        {
            var symptomEntity = symptomRecord.FkIdНапоминанияСимптомыNavigation?.FkIdСимптомыNavigation;
            var ownerName = GetOwnerName(symptomEntity?.FkIdПозицииNavigation);

            var homeCheck = CreateHomeCheck(
                symptomEntity?.Название ?? "Симптом",
                $"Оценка: {symptomRecord.ОценкаСамочувствия}/10 | Время: {symptomRecord.ДатаЗаписи:HH:mm}",
                "Зафиксировано",
                ownerName,
                GetSymptomIconPath(),
                "#000000",
                true);

            homeCheck.MouseDoubleClick += (s, e) => SelectCompletedItem(symptomRecord, homeCheck, "CompletedSymptom");
            CompletedTasksStackPanel.Children.Add(homeCheck);
        }

        private void SelectCompletedItem(object record, HomeItemCheck card, string recordType)
        {
            _selectedRecord = record;
            _selectedRecordType = recordType;
            _selectedCard = card;

            var detailsPage = DetailsFrame.Content as HomeDetailsPage;
            if (detailsPage != null)
            {
                detailsPage.ShowDetails(record, null, recordType);
            }
        }

        private void DetailsPage_DeleteRequested(object sender, EventArgs e)
        {
            if (_selectedRecord == null)
            {
                return;
            }

            var result = MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            if (_selectedRecordType == "CompletedMedicine" && _selectedRecord is ФиксацияПриёма intake)
            {
                _context.ФиксацияПриёмаs.Remove(intake);
            }
            else if (_selectedRecordType == "CompletedMeasurement" && _selectedRecord is ЗначенияИзмерения measurement)
            {
                _context.ЗначенияИзмеренияs.Remove(measurement);
            }
            else if (_selectedRecordType == "CompletedSymptom" && _selectedRecord is ЗафиксированныеСимптомы symptom)
            {
                _context.ЗафиксированныеСимптомыs.Remove(symptom);
            }
            else if (_selectedRecordType == "Medicine" && _selectedRecord is Лекарства medicine)
            {
                DeleteMedicineCompletely(medicine);
            }
            else if (_selectedRecordType == "Measurement" && _selectedRecord is Измерение measurementEntity)
            {
                DeleteMeasurementCompletely(measurementEntity);
            }
            else if (_selectedRecordType == "Symptom" && _selectedRecord is Симптомы symptomEntity)
            {
                DeleteSymptomCompletely(symptomEntity);
            }

            _context.SaveChanges();
            LoadUserData();
            ClearDetails();

            MessageBox.Show("Запись удалена", "Успех");
        }

        private void DeleteMedicineCompletely(Лекарства medicine)
        {
            var reminders = _context.НапоминаниеЛекарстваs.Where(r => r.FkIdЛекарства == medicine.PkIdЛекарства).ToList();
            if (reminders.Any())
            {
                _context.НапоминаниеЛекарстваs.RemoveRange(reminders);
            }

            var intakes = _context.ФиксацияПриёмаs.Include(f => f.FkIdНапоминанияЛекарстваNavigation).Where(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарства == medicine.PkIdЛекарства).ToList();
            if (intakes.Any())
            {
                _context.ФиксацияПриёмаs.RemoveRange(intakes);
            }

            _context.Лекарстваs.Remove(medicine);
        }

        private void DeleteMeasurementCompletely(Измерение measurementEntity)
        {
            var reminders = _context.НапоминаниеИзмеренияs.Where(r => r.FkIdИзмерения == measurementEntity.PkIdИзмерения).ToList();
            if (reminders.Any())
            {
                _context.НапоминаниеИзмеренияs.RemoveRange(reminders);
            }

            var values = _context.ЗначенияИзмеренияs.Include(z => z.FkIdНапоминанияИзмеренияNavigation).Where(z => z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмерения == measurementEntity.PkIdИзмерения).ToList();
            if (values.Any())
            {
                _context.ЗначенияИзмеренияs.RemoveRange(values);
            }

            _context.Измерениеs.Remove(measurementEntity);
        }

        private void DeleteSymptomCompletely(Симптомы symptomEntity)
        {
            var reminders = _context.НапоминаниеСимптомыs.Where(r => r.FkIdСимптомы == symptomEntity.PkIdСимптомы).ToList();
            if (reminders.Any())
            {
                _context.НапоминаниеСимптомыs.RemoveRange(reminders);
            }

            var recorded = _context.ЗафиксированныеСимптомыs.Include(z => z.FkIdНапоминанияСимптомыNavigation).Where(z => z.FkIdНапоминанияСимптомыNavigation.FkIdСимптомы == symptomEntity.PkIdСимптомы).ToList();
            if (recorded.Any())
            {
                _context.ЗафиксированныеСимптомыs.RemoveRange(recorded);
            }

            _context.Симптомыs.Remove(symptomEntity);
        }

        private void DetailsPage_ChangeRecipientRequested(object sender, EventArgs e)
        {
            if (_selectedRecord == null)
            {
                return;
            }

            ChangeEditRecipient dialog = new ChangeEditRecipient(_userId, _selectedRecordType)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                var position = GetPosition(_selectedRecord);
                if (position != null)
                {
                    position.FkIdПолучателя = dialog.SelectedRecipientId;
                    position.FkIdПользователя = _userId;
                    _context.SaveChanges();

                    LoadUserData();
                    MessageBox.Show("Получатель изменен", "Успех");
                }
            }
        }

        private ПозицияЗаписи GetPosition(object record)
        {
            return record switch
            {
                Лекарства medicine => medicine.FkIdПозицииNavigation,
                Измерение measurement => measurement.FkIdПозицииNavigation,
                Симптомы symptom => symptom.FkIdПозицииNavigation,
                _ => null
            };
        }

        private void DetailsPage_ReminderTimeChanged(object sender, EventArgs e)
        {
            if (_selectedCard != null && _selectedReminder != null)
            {
                if (_selectedReminder is НапоминаниеЛекарства medicineReminder)
                {
                    _selectedCard.TimeText = $"{medicineReminder.Часы:00}:{medicineReminder.Минуты:00}";
                }
                else if (_selectedReminder is НапоминаниеИзмерения measurementReminder)
                {
                    _selectedCard.TimeText = $"{measurementReminder.Часы:00}:{measurementReminder.Минуты:00}";
                }
                else if (_selectedReminder is НапоминаниеСимптомы symptomReminder)
                {
                    _selectedCard.TimeText = $"{symptomReminder.Часы:00}:{symptomReminder.Минуты:00}";
                }
            }
        }

        private void DetailsPage_StockSettingsChanged(object sender, EventArgs e)
        {
            if (_selectedCard != null && _selectedRecord is Лекарства medicine)
            {
                _selectedCard.DescriptionText = $"Дозировка: {medicine.Дозировка} мг | " + $"Способ: {medicine.FkIdСпособаПриёмаNavigation?.Тип} | " + $"Запас: {medicine.ТекущийЗапас}/{medicine.МинЗапас} шт.";
            }
        }

        private void ClearPanels()
        {
            MedicinesStackPanel.Children.Clear();
            MeasurementsStackPanel.Children.Clear();
            MoodStackPanel.Children.Clear();
        }

        private void ClearDetails()
        {
            _selectedRecord = null;
            _selectedReminder = null;
            _selectedRecordType = null;
            _selectedCard = null;

            HomeDetailsPage detailsPage = DetailsFrame.Content as HomeDetailsPage;
            detailsPage?.ClearDetails();
        }

        private string GetOwnerName(ПозицияЗаписи position)
        {
            if (position == null) return "Неизвестно";

            if (position.FkIdПолучателя.HasValue)
            {
                var recipient = _context.ПолучателиУходаs
                    .FirstOrDefault(r => r.PkIdПолучателя == position.FkIdПолучателя.Value);
                return recipient?.Имя ?? "Подопечный";
            }

            return "Вы";
        }

        private (string pathData, string iconColor) GetMedicineIconInfo(Лекарства medicine)
        {
            if (medicine?.FkIdИконкиNavigation == null)
            {
                return (null, "#9A00D7");
            }

            var icon = medicine.FkIdИконкиNavigation;
            var vector = icon.FkIdВектораNavigation?.Вектор;
            var color = icon.FkIdЦветИконкиNavigation?.Цвет ?? "#9A00D7";

            if (string.IsNullOrEmpty(vector))
            {
                vector = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
            }

            return (vector, color);
        }

        private void MedicinesCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeRecipient dialog = new ChangeRecipient(_userId) { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                Medicines medicinesWindow = new Medicines(_userId, dialog.SelectedRecipient);
                medicinesWindow.Show();
                Close();
            }
        }

        private void MeasurementsCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Measurement meas = new Measurement(_userId);
            meas.Show();
            Close();
        }

        private void MoodCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeRecipient dialog = new ChangeRecipient(_userId, true) { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                MoodAndSymptoms moodWindow = new MoodAndSymptoms(_userId, dialog.SelectedRecipient);
                moodWindow.Show();
                Close();
            }
        }

        private void BtnRecipe_Click(object sender, RoutedEventArgs e)
        {
            ArchiveRecipe archive = new ArchiveRecipe(_userId);
            archive.Show();
            this.Close(); ;
        }

        private void PersonalMedArchiveCard_Click(object sender, RoutedEventArgs e)
        {
            PersonalMedArchive personalMed = new PersonalMedArchive(_userId);
            personalMed.Show();
            this.Close();
        }

        private void BtnCalendar_Click(object sender, RoutedEventArgs e)
        {
            MainCalendar calendar = new MainCalendar(_userId);
            calendar.Show();
            this.Close();
        }

        private void AllergiesButton_Click(object sender, RoutedEventArgs e)
        {
            Allergies allergies = new Allergies(_userId);
            allergies.Show();
            this.Close();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(_userId);
            settings.Show();
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