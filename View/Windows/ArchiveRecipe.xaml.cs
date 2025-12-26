using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class ArchiveRecipe : Window
    {
        private bool _isEditMode = false;
        private string _archiveFolderPath;
        private int _userId;
        private HealthcareManagementContext _context;

        private _NotificationManager _notificationManager;

        public ArchiveRecipe(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();

            _archiveFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Sourse", "Images", "ArchiveRecipe_Image");
            if (!Directory.Exists(_archiveFolderPath))
            {
                Directory.CreateDirectory(_archiveFolderPath);
            }

            LoadExistingImages();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Home homeWindow = new Home(_userId);
            homeWindow.Show();
            this.Close();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp;*.gif)|*.png;*.jpeg;*.jpg;*.bmp;*.gif",
                Multiselect = true,
                Title = "Выберите фотографии для загрузки"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string imagePath in openFileDialog.FileNames)
                {
                    SaveImageToDatabase(imagePath);
                }

                LoadExistingImages();
                MessageBox.Show("Изображения успешно загружены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveImageToDatabase(string sourcePath)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(sourcePath)}";
            var destPath = Path.Combine(_archiveFolderPath, fileName);

            File.Copy(sourcePath, destPath, true);

            _context.АрхивРецептовs.Add(new АрхивРецептов
            {
                Изображение = destPath,
                ДатаЗагрузки = DateTime.Now,
                FkIdПользователя = _userId
            });

            _context.SaveChanges();
        }

        private void LoadExistingImages()
        {
            ImagesPanel.Children.Clear();

            var userRecipes = _context.АрхивРецептовs.Where(r => r.FkIdПользователя == _userId).OrderByDescending(r => r.ДатаЗагрузки).ToList();

            foreach (var recipe in userRecipes)
            {
                AddImageThumbnail(recipe);
            }
        }

        private void AddImageThumbnail(АрхивРецептов recipe)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = recipe.PkIdАрхива
            };

            string fullImagePath = Path.Combine(Directory.GetCurrentDirectory(), recipe.Изображение);

            if (!File.Exists(fullImagePath))
            {
                ShowImageNotFoundThumbnail(stackPanel, recipe);
                return;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fullImagePath);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 500;
            image.EndInit();
            image.Freeze();

            var border = new Border
            {
                Style = (Style)FindResource("ImageThumbnailStyle")
            };

            var imageControl = new Image
            {
                Source = image,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = recipe.PkIdАрхива
            };

            border.Clip = new RectangleGeometry
            {
                RadiusX = 30,
                RadiusY = 30,
                Rect = new Rect(0, 0, 300, 300)
            };

            border.Child = imageControl;

            border.MouseDown += (s, e) =>
            {
                if (!_isEditMode)
                {
                    ShowFullSizeImage(recipe);
                }
            };

            var deleteButton = new Button
            {
                Content = "Удалить",
                Style = (Style)FindResource("BtnActive2style"),
                Margin = new Thickness(0, 5, 0, 0),
                Tag = recipe.PkIdАрхива,
                Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            deleteButton.Click += DeleteButton_Click;

            stackPanel.Children.Add(border);
            stackPanel.Children.Add(deleteButton);

            ImagesPanel.Children.Add(stackPanel);
        }

        private void ShowImageNotFoundThumbnail(StackPanel stackPanel, АрхивРецептов recipe)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ImageThumbnailStyle"),
                Background = Brushes.LightGray,
                Width = 300,
                Height = 300
            };

            var textBlock = new TextBlock
            {
                Text = "Изображение\nне найдено",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray,
                FontSize = 16
            };

            border.Child = textBlock;

            var deleteButton = new Button
            {
                Content = "Удалить запись",
                Style = (Style)FindResource("BtnActive2style"),
                Margin = new Thickness(0, 5, 0, 0),
                Tag = recipe.PkIdАрхива,
                Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            deleteButton.Click += DeleteButton_Click;

            stackPanel.Children.Add(border);
            stackPanel.Children.Add(deleteButton);

            ImagesPanel.Children.Add(stackPanel);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is int recipeId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот рецепт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new HealthcareManagementContext())
                    {
                        var recipeToDelete = context.АрхивРецептовs.FirstOrDefault(r => r.PkIdАрхива == recipeId);

                        if (recipeToDelete != null)
                        {
                            string fullImagePath = Path.Combine(Directory.GetCurrentDirectory(), recipeToDelete.Изображение);
                            if (File.Exists(fullImagePath))
                            {
                                File.Delete(fullImagePath);
                            }

                            context.АрхивРецептовs.Remove(recipeToDelete);
                            context.SaveChanges();

                            LoadExistingImages();

                            MessageBox.Show("Рецепт успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void ShowFullSizeImage(АрхивРецептов recipe)
        {
            string fullImagePath = Path.Combine(Directory.GetCurrentDirectory(), recipe.Изображение);

            if (!File.Exists(fullImagePath))
            {
                MessageBox.Show("Изображение не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fullImageWindow = new Window
            {
                Title = "Просмотр изображения - " + Path.GetFileName(recipe.Изображение),
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black
            };

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fullImagePath);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            var imageControl = new Image
            {
                Source = image,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var closeButton = new Button
            {
                Content = "X",
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

            fullImageWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    fullImageWindow.Close();
                }
            };

            fullImageWindow.ShowDialog();
        }

        private void BtnСhange_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;

            if (_isEditMode)
            {
                BtnChange.Content = "Готово";
                ShowDeleteButtons(true);
            }
            else
            {
                BtnChange.Content = "Изменить";
                ShowDeleteButtons(false);
            }
        }

        private void ShowDeleteButtons(bool show)
        {
            foreach (var child in ImagesPanel.Children)
            {
                if (child is StackPanel stackPanel && stackPanel.Children.Count > 1)
                {
                    if (stackPanel.Children[1] is Button deleteButton)
                    {
                        deleteButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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