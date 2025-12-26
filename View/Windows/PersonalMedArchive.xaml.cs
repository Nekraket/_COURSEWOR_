using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class PersonalMedArchive : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private Лекарства _selectedMedicine;

        private _NotificationManager _notificationManager;

        public PersonalMedArchive(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();

            Loaded += PersonalMedArchive_Loaded;

            _notificationManager = new _NotificationManager(_userId);
        }

        private void PersonalMedArchive_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            InitializeDefaultDetails();
        }

        private void InitializeDefaultDetails()
        {
            ClearSelection();
        }

        private void LoadCategories()
        {
            CategoriesPanel.Children.Clear();

            var categories = _context.КатегорияАрхиваs
                .Include(c => c.ЛичныйАрхивs)
                .ThenInclude(la => la.Лекарстваs).ThenInclude(m => m.FkIdИконкиNavigation).ThenInclude(i => i.FkIdВектораNavigation)
                .Include(c => c.ЛичныйАрхивs).ThenInclude(la => la.Лекарстваs).ThenInclude(m => m.FkIdИконкиNavigation).ThenInclude(i => i.FkIdЦветИконкиNavigation)
                .Include(c => c.ЛичныйАрхивs).ThenInclude(la => la.Лекарстваs).ThenInclude(m => m.FkIdСпособаПриёмаNavigation)
                .Include(c => c.ЛичныйАрхивs).ThenInclude(la => la.Лекарстваs).ThenInclude(m => m.СоставВЛекарствеs).ThenInclude(s => s.FkIdСоставаЛекарстваNavigation)
                .Include(c => c.ЛичныйАрхивs).ThenInclude(la => la.Лекарстваs).ThenInclude(m => m.FkIdПозицииNavigation)
                .Where(c => c.ЛичныйАрхивs.Any(la => la.FkIdПользователя == _userId))
                .ToList();

            if (!categories.Any())
            {
                var noCategoriesText = new TextBlock
                {
                    Text = "У вас пока нет категорий.\nНажмите 'Добавить новую группу' для создания первой категории.",
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20),
                    TextAlignment = TextAlignment.Center
                };
                CategoriesPanel.Children.Add(noCategoriesText);
            }
            else
            {
                foreach (var category in categories)
                {
                    CreateCategoryExpander(category);
                }
            }

            var addButton = new Button
            {
                Content = "Добавить новую группу",
                FontSize = 16,
                Height = 40,
                Margin = new Thickness(10, 20, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var buttonStyle = FindResource("BtnActivestyle") as Style;
            addButton.Style = buttonStyle;

            addButton.Click += AddNewCategory_Click;
            CategoriesPanel.Children.Add(addButton);
        }

        private void CreateCategoryExpander(КатегорияАрхива category)
        {
            var expander = new Expander
            {
                Header = CreateCategoryHeader(category),
                Margin = new Thickness(10, 5, 10, 5),
                IsExpanded = false
            };

            var expanderStyle = FindResource("ExpanderStyle") as Style;
            if (expanderStyle != null)
            {
                expander.Style = expanderStyle;
            }
            else
            {
                expander.BorderThickness = new Thickness(1);
                expander.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                expander.Background = Brushes.White;
            }

            var contentPanel = new StackPanel
            {
                Background = Brushes.Transparent
            };

            var personalArchive = category.ЛичныйАрхивs.FirstOrDefault(la => la.FkIdПользователя == _userId);

            if (personalArchive != null && personalArchive.Лекарстваs.Any())
            {
                foreach (var medicine in personalArchive.Лекарстваs)
                {
                    var medicineItem = CreateMedicineItem(medicine);
                    contentPanel.Children.Add(medicineItem);
                }
            }
            else
            {
                var noMedicinesText = new TextBlock
                {
                    Text = "В этой категории пока нет лекарств",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(5, 15, 5, 5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                contentPanel.Children.Add(noMedicinesText);
            }

            var addMedicineButton = new Button
            {
                Content = "+ Добавить лекарство",
                Margin = new Thickness(0, 10, 0, 10),
                Height = 35,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = category.PkIdКатегорииАрхива,
                Cursor = Cursors.Hand
            };

            var buttonStyle = FindResource("BtnActive2style") as Style;
            if (buttonStyle != null)
            {
                addMedicineButton.Style = buttonStyle;
            }
            else
            {
                addMedicineButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                addMedicineButton.Foreground = Brushes.White;
                addMedicineButton.BorderBrush = Brushes.Transparent;
            }

            addMedicineButton.Click += (s, e) => AddMedicineToCategory(category.PkIdКатегорииАрхива);
            contentPanel.Children.Add(addMedicineButton);

            expander.Content = contentPanel;

            int insertIndex = CategoriesPanel.Children.Count;
            CategoriesPanel.Children.Insert(insertIndex, expander);
        }

        private Grid CreateCategoryHeader(КатегорияАрхива category)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 10, 5, 10)
            };

            var personalArchive = category.ЛичныйАрхивs.FirstOrDefault(la => la.FkIdПользователя == _userId);
            int medicineCount = personalArchive?.Лекарстваs?.Count ?? 0;

            var categoryNameText = new TextBlock
            {
                Text = category.Название,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var countBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6F00")),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(6, 2, 6, 2),
                VerticalAlignment = VerticalAlignment.Center
            };

            var countText = new TextBlock
            {
                Text = medicineCount.ToString(),
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            countBadge.Child = countText;

            leftPanel.Children.Add(categoryNameText);
            leftPanel.Children.Add(countBadge);

            var deleteButton = new Button
            {
                Content = "Удалить",
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Tag = category,
                Cursor = Cursors.Hand,
                Height = 35
            };

            var buttonStyle = FindResource("BtnActive2style") as Style;
            if (buttonStyle != null)
            {
                deleteButton.Style = buttonStyle;
            }
            else
            {
                deleteButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4444"));
                deleteButton.Foreground = Brushes.White;
                deleteButton.BorderBrush = Brushes.Transparent;
            }

            deleteButton.Click += (s, e) => DeleteCategory(category);

            Grid.SetColumn(leftPanel, 0);
            Grid.SetColumn(deleteButton, 1);

            grid.Children.Add(leftPanel);
            grid.Children.Add(deleteButton);

            return grid;
        }

        private void DeleteCategory(КатегорияАрхива category)
        {
            var message = $"Вы уверены, что хотите удалить категорию '{category.Название}'?\nВсе лекарства в ней также будут удалены.";

            if (MessageBox.Show(message, "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using var transaction = _context.Database.BeginTransaction();

                var personalArchive = _context.ЛичныйАрхивs.Include(la => la.Лекарстваs).FirstOrDefault(la => la.FkIdКатегорииАрхива == category.PkIdКатегорииАрхива && la.FkIdПользователя == _userId);

                if (personalArchive != null)
                {
                    foreach (var medicine in personalArchive.Лекарстваs.ToList())
                    {
                        DeleteMedicineWithDependencies(medicine.PkIdЛекарства);
                    }

                    _context.ЛичныйАрхивs.Remove(personalArchive);
                    _context.SaveChanges();
                }

                var otherArchives = _context.ЛичныйАрхивs.Any(la => la.FkIdКатегорииАрхива == category.PkIdКатегорииАрхива);

                if (!otherArchives)
                {
                    _context.КатегорияАрхиваs.Remove(category);
                    _context.SaveChanges();
                }

                transaction.Commit();

                LoadCategories();
                ClearSelection();
                MessageBox.Show("Категория удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Border CreateMedicineItem(Лекарства medicine)
        {
            var border = new Border
            {
                Margin = new Thickness(10, 5, 10, 5),
                Tag = medicine,
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(5),
                Cursor = Cursors.Hand,
                Padding = new Thickness(10)
            };

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            border.MouseEnter += (s, e) =>
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
            };

            border.MouseLeave += (s, e) =>
            {
                border.Background = Brushes.Transparent;
            };

            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconPath = new Path();

            var iconData = medicine.FkIdИконкиNavigation?.FkIdВектораNavigation?.Вектор;
            if (!string.IsNullOrEmpty(iconData))
            {
                iconPath.Data = Geometry.Parse(iconData);

                var colorHex = medicine.FkIdИконкиNavigation?.FkIdЦветИконкиNavigation?.Цвет;
                if (!string.IsNullOrEmpty(colorHex))
                {
                    try
                    {
                        iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
                    }
                    catch
                    {
                        iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A00D7"));
                    }
                }
                else
                {
                    iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A00D7"));
                }

                iconPath.Stretch = Stretch.Uniform;
                iconPath.Width = 25;
                iconPath.Height = 25;
            }
            else
            {
                iconPath.Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A00D7"));
                iconPath.Stretch = Stretch.Uniform;
                iconPath.Width = 25;
                iconPath.Height = 25;
            }

            iconPath.HorizontalAlignment = HorizontalAlignment.Center;
            iconPath.VerticalAlignment = VerticalAlignment.Center;
            iconBorder.Child = iconPath;

            var textPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var nameText = new TextBlock
            {
                Text = medicine.Название,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.Black,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            textPanel.Children.Add(nameText);

            if (medicine.Дозировка > 0)
            {
                var dosageText = new TextBlock
                {
                    Text = $"Дозировка: {medicine.Дозировка} мг",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                textPanel.Children.Add(dosageText);
            }

            var ownerText = new TextBlock
            {
                Text = GetOwnerName(medicine.FkIdПозицииNavigation?.FkIdПользователя ?? _userId, medicine.FkIdПозицииNavigation?.FkIdПолучателя),
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0),
                FontStyle = FontStyles.Italic
            };
            textPanel.Children.Add(ownerText);

            panel.Children.Add(iconBorder);
            panel.Children.Add(textPanel);
            border.Child = panel;

            border.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    ShowMedicineDetails(medicine);
                }
            };

            return border;
        }

        private void ShowMedicineDetails(Лекарства medicine)
        {
            _selectedMedicine = medicine;
            DetailsStackPanel.Children.Clear();

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var iconBorder = new Border
            {
                Width = 60,
                Height = 60,
                CornerRadius = new CornerRadius(30),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconPath = new Path();

            var iconData = medicine.FkIdИконкиNavigation?.FkIdВектораNavigation?.Вектор;
            if (!string.IsNullOrEmpty(iconData))
            {
                iconPath.Data = Geometry.Parse(iconData);

                var colorHex = medicine.FkIdИконкиNavigation?.FkIdЦветИконкиNavigation?.Цвет;
                if (!string.IsNullOrEmpty(colorHex))
                {
                    iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
                }
                else
                {
                    iconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A00D7"));
                }

                iconPath.Stretch = Stretch.Uniform;
                iconPath.Width = 35;
                iconPath.Height = 35;
            }

            iconPath.HorizontalAlignment = HorizontalAlignment.Center;
            iconPath.VerticalAlignment = VerticalAlignment.Center;
            iconBorder.Child = iconPath;

            var nameStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var nameText = new TextBlock
            {
                Text = medicine.Название,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };

            nameStack.Children.Add(nameText);

            var currentOwner = GetOwnerName(medicine.FkIdПозицииNavigation?.FkIdПользователя ?? _userId, medicine.FkIdПозицииNavigation?.FkIdПолучателя);
            var ownerText = new TextBlock
            {
                Text = $"Текущий получатель: {currentOwner}",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DarkGray,
                Margin = new Thickness(0, 5, 0, 0)
            };
            nameStack.Children.Add(ownerText);

            if (medicine.Дозировка > 0)
            {
                var dosageText = new TextBlock
                {
                    Text = $"Дозировка: {medicine.Дозировка} мг",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                nameStack.Children.Add(dosageText);
            }

            headerPanel.Children.Add(iconBorder);
            headerPanel.Children.Add(nameStack);

            stackPanel.Children.Add(headerPanel);

            var separator = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(separator);

            var detailsPanel = new StackPanel();

            var category = _context.КатегорияАрхиваs.Include(c => c.ЛичныйАрхивs).FirstOrDefault(c => c.ЛичныйАрхивs.Any(la => la.Лекарстваs.Any(m => m.PkIdЛекарства == medicine.PkIdЛекарства)));

            if (category != null)
            {
                AddDetail(detailsPanel, "Категория архива:", category.Название);
            }

            if (!string.IsNullOrEmpty(medicine.Назначение))
            {
                AddDetail(detailsPanel, "Назначение:", medicine.Назначение);
            }

            if (medicine.ПриёмовВДень > 0)
            {
                AddDetail(detailsPanel, "Приемов в день:", medicine.ПриёмовВДень.ToString());
            }

            if (medicine.Дозировка > 0)
            {
                AddDetail(detailsPanel, "Дозировка:", $"{medicine.Дозировка} мг");
            }

            var способПриема = medicine.FkIdСпособаПриёмаNavigation?.Тип;
            if (!string.IsNullOrEmpty(способПриема))
            {
                AddDetail(detailsPanel, "Способ приема:", способПриема);
            }

            var состав = medicine.СоставВЛекарствеs?.Select(s => s.FkIdСоставаЛекарстваNavigation?.НазваниеСоставляющей).Where(s => !string.IsNullOrEmpty(s)).ToList();

            if (состав != null && состав.Any())
            {
                var составText = string.Join(", ", состав);
                AddDetail(detailsPanel, "Состав:", составText);
            }

            if (!string.IsNullOrEmpty(medicine.Комментарий))
            {
                AddDetail(detailsPanel, "Комментарий:", medicine.Комментарий);
            }

            if (!string.IsNullOrEmpty(medicine.Фото))
            {
                var фотоStack = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 15)
                };

                var фотоLabel = new TextBlock
                {
                    Text = "Фото:",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkGray,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                фотоStack.Children.Add(фотоLabel);

                string путьФото = medicine.Фото;
                if (!System.IO.Path.IsPathRooted(путьФото))
                {
                    путьФото = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, путьФото);
                }

                if (System.IO.File.Exists(путьФото))
                {
                    var фотоImage = new Image
                    {
                        Source = new BitmapImage(new Uri(путьФото)),
                        Width = 150,
                        Height = 150,
                        Stretch = Stretch.Uniform,
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    фотоImage.MouseDown += (s, e) => ShowFullSizeImage(путьФото);
                    фотоStack.Children.Add(фотоImage);
                }

                detailsPanel.Children.Add(фотоStack);
            }

            stackPanel.Children.Add(detailsPanel);

            DetailsStackPanel.Children.Add(stackPanel);
            DeleteButton.Visibility = Visibility.Visible;
            ChangeRecipientButton.Visibility = Visibility.Visible;
        }

        private void ShowFullSizeImage(string imagePath)
        {
            var window = new Window
            {
                Title = "Просмотр фото",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath)),
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            window.Content = image;
            window.ShowDialog();
        }

        private string GetOwnerName(int positionUserId, int? recipientId)
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

            var user = _context.Пользовательs.FirstOrDefault(u => u.PkIdПользователя == positionUserId);
            return user?.Логин ?? "Владелец";
        }

        private void AddDetail(StackPanel panel, string label, string value)
        {
            var detailStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkGray,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            detailStack.Children.Add(labelText);
            detailStack.Children.Add(valueText);
            panel.Children.Add(detailStack);
        }

        private void AddNewCategory_Click(object sender, RoutedEventArgs e)
        {
            CategoryInputDialog inputDialog = new CategoryInputDialog();
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.CategoryName))
            {
                var category = new КатегорияАрхива
                {
                    Название = inputDialog.CategoryName
                };

                _context.КатегорияАрхиваs.Add(category);
                _context.SaveChanges();

                var personalArchive = new ЛичныйАрхив
                {
                    FkIdКатегорииАрхива = category.PkIdКатегорииАрхива,
                    FkIdПользователя = _userId
                };

                _context.ЛичныйАрхивs.Add(personalArchive);
                _context.SaveChanges();

                LoadCategories();

                MessageBox.Show("Категория успешно добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddMedicineToCategory(int categoryId)
        {
            var dialog = new ChangeRecipient(_userId)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.ShowDialog() == true)
            {
                ПолучателиУхода selectedRecipient = null;

                if (dialog.IsForMyselfSelected)
                {
                    OpenAddMedicineWindow(categoryId, null);
                }
                else if (dialog.SelectedRecipient != null)
                {
                    OpenAddMedicineWindow(categoryId, dialog.SelectedRecipient);
                }
            }
        }

        private void OpenAddMedicineWindow(int categoryId, ПолучателиУхода recipient)
        {
            var addMedicineWindow = new AddPersonalMed(_userId, categoryId, recipient);

            addMedicineWindow.MedicineSaved += (medicine) =>
            {
                LoadCategories();
            };

            addMedicineWindow.ShowDialog();
        }

        private void ChangeRecipientButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedicine == null)
            {
                MessageBox.Show("Сначала выберите лекарство для изменения получателя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new ChangeEditRecipient(_userId, "Medicine");
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (dialog.ShowDialog() == true)
            {
                var position = _context.ПозицияЗаписиs.FirstOrDefault(p => p.PkIdПозиции == _selectedMedicine.FkIdПозиции);
                if (position != null)
                {
                    position.FkIdПолучателя = dialog.SelectedRecipientId;
                    position.FkIdПользователя = _userId;

                    _context.SaveChanges();

                    ShowMedicineDetails(_selectedMedicine);
                    LoadCategories();

                    MessageBox.Show("Получатель успешно изменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось найти позицию лекарства", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedicine != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить лекарство '{_selectedMedicine.Название}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteMedicineWithDependencies(_selectedMedicine.PkIdЛекарства);

                    LoadCategories();
                    ClearSelection();

                    MessageBox.Show("Лекарство удалено из архива", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteMedicineWithDependencies(int medicineId)
        {
            var medicineToDelete = _context.Лекарстваs.Include(m => m.СоставВЛекарствеs).Include(m => m.НапоминаниеЛекарстваs).ThenInclude(r => r.ФиксацияПриёмаs).FirstOrDefault(m => m.PkIdЛекарства == medicineId);

            if (medicineToDelete == null)
            {
                return;
            }

            foreach (var reminder in medicineToDelete.НапоминаниеЛекарстваs.ToList())
            {
                _context.ФиксацияПриёмаs.RemoveRange(reminder.ФиксацияПриёмаs);
                _context.НапоминаниеЛекарстваs.Remove(reminder);
            }

            _context.СоставВЛекарствеs.RemoveRange(medicineToDelete.СоставВЛекарствеs);

            var iconId = medicineToDelete.FkIdИконки;
            var positionId = medicineToDelete.FkIdПозиции;

            _context.Лекарстваs.Remove(medicineToDelete);
            _context.SaveChanges();

            if (iconId > 0)
            {
                var iconInUse = _context.Лекарстваs.Any(m => m.FkIdИконки == iconId);
                if (!iconInUse)
                {
                    var iconToDelete = _context.ИконкиЛекарствs.Find(iconId);
                    if (iconToDelete != null)
                    {
                        _context.ИконкиЛекарствs.Remove(iconToDelete);
                    }
                }
            }

            if (positionId > 0)
            {
                var positionInUse = _context.Лекарстваs.Any(m => m.FkIdПозиции == positionId);
                if (!positionInUse)
                {
                    var positionToDelete = _context.ПозицияЗаписиs.Find(positionId);
                    if (positionToDelete != null)
                    {
                        _context.ПозицияЗаписиs.Remove(positionToDelete);
                    }
                }
            }

            _context.SaveChanges();
        }

        private void ClearSelection()
        {
            _selectedMedicine = null;
            DeleteButton.Visibility = Visibility.Collapsed;
            ChangeRecipientButton.Visibility = Visibility.Collapsed;
            DetailsStackPanel.Children.Clear();

            var defaultText = new TextBlock
            {
                Text = "Выберите запись для просмотра деталей",
                Margin = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            DetailsStackPanel.Children.Add(defaultText);
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            new Home(_userId).Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            _notificationManager?.Dispose();
            base.OnClosed(e);
        }
    }

    public class CategoryInputDialog : Window
    {
        public string CategoryName { get; set; }

        public CategoryInputDialog()
        {
            Title = "Добавить новую группу";
            Width = 400;
            Height = 250;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Введите название новой группы (симптом/заболевание):",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15),
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            var textBox = new TextBox
            {
                Height = 35,
                Margin = new Thickness(0, 0, 0, 20),
                FontSize = 14,
                Padding = new Thickness(5)
            };

            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "ОК",
                Width = 80,
                Height = 35,
                Margin = new Thickness(5, 0, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6F00")),
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                Cursor = Cursors.Hand
            };

            okButton.Click += (sender, e) =>
            {
                CategoryName = textBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(CategoryName))
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Введите название группы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 35,
                Margin = new Thickness(5, 0, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Transparent,
                Cursor = Cursors.Hand
            };

            cancelButton.Click += (sender, e) =>
            {
                DialogResult = false;
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);

            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;

            Loaded += (s, e) => textBox.Focus();
        }
    }
}