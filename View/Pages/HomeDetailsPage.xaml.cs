using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Pages
{
    public partial class HomeDetailsPage : Page
    {
        public event EventHandler DeleteRequested;
        public event EventHandler ChangeRecipientRequested;
        public event EventHandler ReminderTimeChanged;
        public event EventHandler StockSettingsChanged;

        private HealthcareManagementContext _context;

        private object _selectedRecord;
        private string _selectedRecordType;
        private object _selectedReminder;

        public HomeDetailsPage()
        {
            InitializeComponent();
        }

        public void Initialize(int userId, HealthcareManagementContext context)
        {
            _context = context;
            ClearDetails();
        }

        public void ShowDetails(object record, object reminder, string recordType)
        {
            _selectedRecord = record;
            _selectedRecordType = recordType;
            _selectedReminder = reminder;

            DetailsStackPanel.Children.Clear();

            switch (recordType)
            {
                case "Medicine":
                    ShowMedicineDetails((Лекарства)record, (НапоминаниеЛекарства)reminder);
                    break;
                case "Measurement":
                    ShowMeasurementDetails((Измерение)record, (НапоминаниеИзмерения)reminder);
                    break;
                case "Symptom":
                    ShowSymptomDetails((Симптомы)record, (НапоминаниеСимптомы)reminder);
                    break;
                case "CompletedMedicine":
                    ShowCompletedMedicineDetails((ФиксацияПриёма)record);
                    break;
                case "CompletedMeasurement":
                    ShowCompletedMeasurementDetails((ЗначенияИзмерения)record);
                    break;
                case "CompletedSymptom":
                    ShowCompletedSymptomDetails((ЗафиксированныеСимптомы)record);
                    break;
            }

            UpdateButtonsVisibility();
        }

        private void ShowMedicineDetails(Лекарства medicine, НапоминаниеЛекарства reminder)
        {
            var panel = CreateDetailsPanel();
            var owner = GetOwnerName(medicine.FkIdПозицииNavigation);

            AddTitle(panel, medicine.Название, 18, true);
            AddDetail(panel, $"Получатель: {owner}", 14, true);

            if (medicine.FkIdИконкиNavigation != null)
            {
                AddIcon(panel, medicine.FkIdИконкиNavigation);
            }

            var grid = CreateGrid();
            int row = 0;

            if (!string.IsNullOrEmpty(medicine.Назначение))
            {
                AddGridRow(grid, "Назначение:", medicine.Назначение, row++);
            }

            if (medicine.Дозировка.HasValue)
            {
                AddGridRow(grid, "Дозировка:", $"{medicine.Дозировка} шт.", row++);
            }

            if (medicine.FkIdСпособаПриёмаNavigation != null)
            {
                AddGridRow(grid, "Способ приема:", medicine.FkIdСпособаПриёмаNavigation.Тип, row++);
            }

            if (reminder != null)
            {
                AddGridRow(grid, "Время приема:", $"{reminder.Часы:00}:{reminder.Минуты:00}", row++);
            }

            AddGridRow(grid, "Напоминания о запасе:", medicine.НапоминанияЗапас ? "Включены" : "Выключены", row++);
            AddGridRow(grid, "Текущий запас:", medicine.ТекущийЗапас.ToString(), row++);
            AddGridRow(grid, "Минимальный запас:", medicine.МинЗапас.ToString(), row++);

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            if (!string.IsNullOrWhiteSpace(medicine.Фото))
            {
                AddImage(panel, medicine.Фото);
            }

            if (!string.IsNullOrWhiteSpace(medicine.Комментарий))
            {
                AddComment(panel, medicine.Комментарий);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void ShowMeasurementDetails(Измерение measurement, НапоминаниеИзмерения reminder)
        {
            var panel = CreateDetailsPanel();
            var owner = GetOwnerName(measurement.FkIdПозицииNavigation);
            var type = measurement.FkIdТипИзмеренияNavigation;

            AddTitle(panel, type?.Название ?? "Измерение", 18, true);
            AddDetail(panel, $"Получатель: {owner}", 14, true);

            var grid = CreateGrid();
            int row = 0;

            if (type != null)
            {
                AddGridRow(grid, "Тип измерения:", type.Название, row++);
                if (!string.IsNullOrEmpty(type.ЕдИзмерения))
                {
                    AddGridRow(grid, "Единица:", type.ЕдИзмерения, row++);
                }
            }

            if (reminder != null)
            {
                AddGridRow(grid, "Время напоминания:", $"{reminder.Часы:00}:{reminder.Минуты:00}", row++);
            }

            AddGridRow(grid, "Записей в день:", measurement.ЗаписейВДень.ToString(), row++);

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void ShowSymptomDetails(Симптомы symptom, НапоминаниеСимптомы reminder)
        {
            var panel = CreateDetailsPanel();
            var owner = GetOwnerName(symptom.FkIdПозицииNavigation);

            AddTitle(panel, symptom.Название, 18, true);
            AddDetail(panel, $"Получатель: {owner}", 14, true);

            var grid = CreateGrid();
            int row = 0;

            if (reminder != null)
            {
                AddGridRow(grid, "Время напоминания:", $"{reminder.Часы:00}:{reminder.Минуты:00}", row++);
            }

            AddGridRow(grid, "Записей в день:", symptom.ЗаписейВДень.ToString(), row++);

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void ShowCompletedMedicineDetails(ФиксацияПриёма intake)
        {
            var panel = CreateDetailsPanel();

            var medicine = _context.ФиксацияПриёмаs.Include(f => f.FkIdНапоминанияЛекарстваNavigation.FkIdЛекарстваNavigation.FkIdПозицииNavigation)
                .FirstOrDefault(f => f.PkIdФиксПриём == intake.PkIdФиксПриём) ?.FkIdНапоминанияЛекарстваNavigation?.FkIdЛекарстваNavigation;

            AddTitle(panel, "Выполненный прием", 16, true);

            var grid = CreateGrid();
            int row = 0;

            if (medicine != null)
            {
                AddGridRow(grid, "Лекарство:", medicine.Название, row++);
                AddGridRow(grid, "Получатель:", GetOwnerName(medicine.FkIdПозицииNavigation), row++);
            }

            AddGridRow(grid, "Время приема:", intake.ДатаПриёма.ToString("dd.MM.yyyy HH:mm"), row++);

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void ShowCompletedMeasurementDetails(ЗначенияИзмерения measurement)
        {
            var panel = CreateDetailsPanel();

            var measurementEntity = _context.ЗначенияИзмеренияs.Include(z => z.FkIdНапоминанияИзмеренияNavigation.FkIdИзмеренияNavigation.FkIdТипИзмеренияNavigation)
                .FirstOrDefault(z => z.PkIdЗначениеИзмерения == measurement.PkIdЗначениеИзмерения) ?.FkIdНапоминанияИзмеренияNavigation?.FkIdИзмеренияNavigation;

            var type = measurementEntity?.FkIdТипИзмеренияNavigation;

            AddTitle(panel, "Выполненное измерение", 16, true);

            var grid = CreateGrid();
            int row = 0;

            AddGridRow(grid, "Значение:", $"{measurement.Значение:F1} {type?.ЕдИзмерения ?? "ед."}", row++);
            AddGridRow(grid, "Время:", measurement.ДатаЗаписи.ToString("dd.MM.yyyy HH:mm"), row++);

            if (!string.IsNullOrWhiteSpace(measurement.Заметка))
            {
                AddGridRow(grid, "Заметка:", measurement.Заметка, row++);
            }

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void ShowCompletedSymptomDetails(ЗафиксированныеСимптомы symptom)
        {
            var panel = CreateDetailsPanel();

            AddTitle(panel, "Зафиксированный симптом", 16, true);

            var grid = CreateGrid();
            int row = 0;

            AddGridRow(grid, "Оценка:", $"{symptom.ОценкаСамочувствия}/10", row++);
            AddGridRow(grid, "Время:", symptom.ДатаЗаписи.ToString("dd.MM.yyyy HH:mm"), row++);

            if (!string.IsNullOrWhiteSpace(symptom.Заметка))
            {
                AddGridRow(grid, "Заметка:", symptom.Заметка, row++);
            }

            if (row > 0)
            {
                panel.Children.Add(grid);
            }

            DetailsStackPanel.Children.Add(panel);
        }

        private void UpdateButtonsVisibility()
        {
            var isCompleted = _selectedRecordType?.Contains("Completed") == true;
            var isMedicine = _selectedRecordType == "Medicine";

            EditReminderButton.Visibility = isCompleted ? Visibility.Collapsed : Visibility.Visible;
            ManageStockButton.Visibility = isMedicine ? Visibility.Visible : Visibility.Collapsed;
            DeleteReminderButton.Visibility = isCompleted ? Visibility.Collapsed : Visibility.Visible;
            DeleteAllButton.Visibility = Visibility.Visible;
            ChangeRecipientButton.Visibility = isCompleted ? Visibility.Collapsed : Visibility.Visible;

            if (isCompleted)
            {
                DeleteAllButton.Content = "Удалить запись";
            }
            else
            {
                DeleteReminderButton.Content = "Удалить напоминание";
                DeleteAllButton.Content = "Удалить все";
            }
        }

        private StackPanel CreateDetailsPanel()
        {
            return new StackPanel { Margin = new Thickness(15) };
        }

        private void AddTitle(StackPanel panel, string text, int fontSize, bool bold = false)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(textBlock);
        }

        private void AddDetail(StackPanel panel, string text, int fontSize, bool bold = false)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 5)
            };
            panel.Children.Add(textBlock);
        }

        private void AddIcon(StackPanel panel, ИконкиЛекарств icon)
        {
            if (icon?.FkIdВектораNavigation?.Вектор == null)
            {
                return;
            }

            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(icon.FkIdВектораNavigation.Вектор),
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(icon.FkIdЦветИконкиNavigation?.Цвет ?? "#9A00D7")),
                Width = 40,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(path);
        }

        private Grid CreateGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.Margin = new Thickness(0, 10, 0, 15);
            return grid;
        }

        private void AddGridRow(Grid grid, string label, string value, int row)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 10, 5)
            };
            Grid.SetRow(labelText, row);
            Grid.SetColumn(labelText, 0);
            grid.Children.Add(labelText);

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(valueText, row);
            Grid.SetColumn(valueText, 1);
            grid.Children.Add(valueText);
        }

        private void AddImage(StackPanel panel, string imagePath)
        {
            var fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return;
            }

            var image = new Image
            {
                Source = new BitmapImage(new Uri(fullPath)),
                MaxWidth = 300,
                MaxHeight = 200,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 10, 0, 10),
                Cursor = Cursors.Hand
            };

            image.MouseDown += (s, e) => ShowFullImage(fullPath);
            panel.Children.Add(image);
        }

        private void AddComment(StackPanel panel, string comment)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 154, 0, 215)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 10, 0, 10)
            };

            var textBlock = new TextBlock
            {
                Text = comment,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            panel.Children.Add(border);
        }

        private string GetOwnerName(ПозицияЗаписи position)
        {
            if (position == null)
            {
                return "Неизвестно";
            }
            if (position.FkIdПолучателя.HasValue)
            {
                var recipient = _context.ПолучателиУходаs.FirstOrDefault(r => r.PkIdПолучателя == position.FkIdПолучателя.Value);
                return recipient?.Имя ?? "Подопечный";
            }
            return "Вы";
        }

        private void EditReminderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReminder == null)
            {
                return;
            }

            if (_selectedReminder is НапоминаниеЛекарства medicineReminder)
            {
                HoursTextBox.Text = medicineReminder.Часы.ToString();
                MinutesTextBox.Text = medicineReminder.Минуты.ToString();
            }
            else if (_selectedReminder is НапоминаниеИзмерения measurementReminder)
            {
                HoursTextBox.Text = measurementReminder.Часы.ToString();
                MinutesTextBox.Text = measurementReminder.Минуты.ToString();
            }
            else if (_selectedReminder is НапоминаниеСимптомы symptomReminder)
            {
                HoursTextBox.Text = symptomReminder.Часы.ToString();
                MinutesTextBox.Text = symptomReminder.Минуты.ToString();
            }

            EditReminderPanel.Visibility = Visibility.Visible;
            MainScrollViewer.ScrollToEnd();
        }

        private void ManageStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(_selectedRecord is Лекарства medicine))
            {
                return;
            }

            CurrentStockTextBox.Text = medicine.ТекущийЗапас.ToString();
            MinStockTextBox.Text = medicine.МинЗапас.ToString();
            StockNotificationsCheckBox.IsChecked = medicine.НапоминанияЗапас;

            ManageStockPanel.Visibility = Visibility.Visible;
            MainScrollViewer.ScrollToEnd();
        }

        private void SaveReminderTime_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HoursTextBox.Text, out int hours) || hours < 0 || hours > 23)
            {
                MessageBox.Show("Часы: 0-23", "Ошибка");
                return;
            }

            if (!int.TryParse(MinutesTextBox.Text, out int minutes) || minutes < 0 || minutes > 59)
            {
                MessageBox.Show("Минуты: 0-59", "Ошибка");
                return;
            }

            if (_selectedReminder is НапоминаниеЛекарства medicineReminder)
            {
                medicineReminder.Часы = hours;
                medicineReminder.Минуты = minutes;
            }
            else if (_selectedReminder is НапоминаниеИзмерения measurementReminder)
            {
                measurementReminder.Часы = hours;
                measurementReminder.Минуты = minutes;
            }
            else if (_selectedReminder is НапоминаниеСимптомы symptomReminder)
            {
                symptomReminder.Часы = hours;
                symptomReminder.Минуты = minutes;
            }

            _context.SaveChanges();
            EditReminderPanel.Visibility = Visibility.Collapsed;
            ReminderTimeChanged?.Invoke(this, EventArgs.Empty);
            ShowDetails(_selectedRecord, _selectedReminder, _selectedRecordType);
        }

        private void SaveStockSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!(_selectedRecord is Лекарства medicine))
            {
                return;
            }

            if (!int.TryParse(CurrentStockTextBox.Text, out int currentStock) || currentStock < 0)
            {
                MessageBox.Show("Текущий запас должен быть ≥ 0", "Ошибка");
                return;
            }

            if (!int.TryParse(MinStockTextBox.Text, out int minStock) || minStock < 0)
            {
                MessageBox.Show("Минимальный запас должен быть ≥ 0", "Ошибка");
                return;
            }

            medicine.ТекущийЗапас = currentStock;
            medicine.МинЗапас = minStock;
            medicine.НапоминанияЗапас = StockNotificationsCheckBox.IsChecked ?? false;

            _context.SaveChanges();
            ManageStockPanel.Visibility = Visibility.Collapsed;
            StockSettingsChanged?.Invoke(this, EventArgs.Empty);
            ShowDetails(_selectedRecord, _selectedReminder, _selectedRecordType);
        }

        private void CancelReminderEdit_Click(object sender, RoutedEventArgs e)
        {
            EditReminderPanel.Visibility = Visibility.Collapsed;
        }

        private void CancelStockEdit_Click(object sender, RoutedEventArgs e)
        {
            ManageStockPanel.Visibility = Visibility.Collapsed;
        }

        private void DeleteReminderButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Удалить это напоминание?\n(Запись останется)", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            string message = _selectedRecordType?.Contains("Completed") == true ? "Удалить эту запись?" : "Удалить ВСЮ запись целиком?";

            var result = MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ChangeRecipientButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeRecipientRequested?.Invoke(this, EventArgs.Empty);
        }

        private void HoursTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void MinutesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void StockTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        public void ClearDetails()
        {
            DetailsStackPanel.Children.Clear();
            _selectedRecord = null;
            _selectedRecordType = null;
            _selectedReminder = null;

            EditReminderButton.Visibility = Visibility.Collapsed;
            ManageStockButton.Visibility = Visibility.Collapsed;
            DeleteReminderButton.Visibility = Visibility.Collapsed;
            DeleteAllButton.Visibility = Visibility.Collapsed;
            ChangeRecipientButton.Visibility = Visibility.Collapsed;
        }

        private void ShowFullImage(string imagePath)
        {
            var window = new Window
            {
                Title = "Фото",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black
            };

            var image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath)),
                Stretch = Stretch.Uniform
            };

            image.MouseDown += (s, e) => window.Close();

            var closeButton = new Button
            {
                Content = "✕",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 20, 0)
            };

            closeButton.Click += (s, e) => window.Close();

            var grid = new Grid();
            grid.Children.Add(image);
            grid.Children.Add(closeButton);
            window.Content = grid;
            window.ShowDialog();
        }
    }
}