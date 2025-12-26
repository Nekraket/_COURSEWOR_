using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Allergies : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;
        private ObservableCollection<Аллергии> _userAllergies;
        private Dictionary<int, ObservableCollection<Аллергии>> _recipientAllergies;
        private List<ПолучателиУхода> _recipients;
        private List<string> _allAllergens;

        private _NotificationManager _notificationManager;

        public Allergies(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();
            _userAllergies = new ObservableCollection<Аллергии>();
            _recipientAllergies = new Dictionary<int, ObservableCollection<Аллергии>>();
            InitializeAutoComplete();

            _notificationManager = new _NotificationManager(_userId);
        }

        private void InitializeAutoComplete()
        {
            _allAllergens = _context.Аллергииs.Where(a => a.FkIdПользователя == null && a.FkIdПолучателя == null).Select(a => a.Аллерген).Distinct().OrderBy(a => a).ToList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AllergiesTabControl.Items.Clear();
            _userAllergies.Clear();
            _recipientAllergies.Clear();

            var userAllergiesList = _context.Аллергииs.Where(a => a.FkIdПользователя == _userId).OrderBy(a => a.Аллерген).ToList();

            foreach (var allergy in userAllergiesList)
            {
                _userAllergies.Add(allergy);
            }

            var userTabItem = CreateUserTabItem();
            AllergiesTabControl.Items.Add(userTabItem);

            _recipients = _context.ПолучателиУходаs.Where(r => r.FkIdПользователя == _userId).OrderBy(r => r.Имя).ToList();

            foreach (var recipient in _recipients)
            {
                var recipientAllergies = new ObservableCollection<Аллергии>();
                var recipientAllergiesList = _context.Аллергииs.Where(a => a.FkIdПолучателя == recipient.PkIdПолучателя).OrderBy(a => a.Аллерген).ToList();

                foreach (var allergy in recipientAllergiesList)
                {
                    recipientAllergies.Add(allergy);
                }

                _recipientAllergies[recipient.PkIdПолучателя] = recipientAllergies;
                var recipientTabItem = CreateRecipientTabItem(recipient, recipientAllergies);
                AllergiesTabControl.Items.Add(recipientTabItem);
            }

            if (AllergiesTabControl.Items.Count > 0)
            {
                AllergiesTabControl.SelectedIndex = 0;
            }
        }

        private TabItem CreateUserTabItem()
        {
            var tabItem = new TabItem
            {
                Header = "Вы"
            };

            var ownerData = new object[] { "User", _userId };
            tabItem.Tag = ownerData;

            tabItem.Content = CreateTabContent("User", _userId, "Вы", _userAllergies);
            return tabItem;
        }

        private TabItem CreateRecipientTabItem(ПолучателиУхода recipient, ObservableCollection<Аллергии> allergies)
        {
            var tabItem = new TabItem
            {
                Header = recipient.Имя
            };

            var ownerData = new object[] { "Recipient", recipient.PkIdПолучателя };
            tabItem.Tag = ownerData;

            tabItem.Content = CreateTabContent("Recipient", recipient.PkIdПолучателя, recipient.Имя, allergies);
            return tabItem;
        }

        private FrameworkElement CreateTabContent(string ownerType, int ownerId, string ownerName, ObservableCollection<Аллергии> allergies)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            var leftPanel = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(10)
            };

            var leftStack = new StackPanel();

            var headerPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"Аллергии {ownerName}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var countText = new TextBlock
            {
                Text = $"Всего: {allergies.Count}",
                FontSize = 14,
                Foreground = Brushes.Gray
            };
            headerPanel.Children.Add(countText);

            leftStack.Children.Add(headerPanel);

            var listBox = new ListBox
            {
                ItemsSource = allergies,
                SelectionMode = SelectionMode.Single,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };

            listBox.ItemTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Grid));
            factory.SetValue(Grid.MarginProperty, new Thickness(0));

            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(30));

            factory.AppendChild(col1);
            factory.AppendChild(col2);

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Аллерген"));
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, 14.0);
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            textBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlockFactory.SetValue(Grid.ColumnProperty, 0);
            factory.AppendChild(textBlockFactory);

            var deleteButtonFactory = new FrameworkElementFactory(typeof(Button));
            deleteButtonFactory.SetValue(Button.ContentProperty, "×");
            deleteButtonFactory.SetValue(Button.FontSizeProperty, 16.0);
            deleteButtonFactory.SetValue(Button.FontWeightProperty, FontWeights.Bold);
            deleteButtonFactory.SetValue(Button.ForegroundProperty, Brushes.Red);
            deleteButtonFactory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
            deleteButtonFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            deleteButtonFactory.SetValue(Button.PaddingProperty, new Thickness(5));
            deleteButtonFactory.SetValue(Button.CursorProperty, Cursors.Hand);
            deleteButtonFactory.SetValue(Grid.ColumnProperty, 1);
            deleteButtonFactory.SetValue(Button.TagProperty, new System.Windows.Data.Binding("."));
            deleteButtonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteAllergyButton_Click));

            factory.AppendChild(deleteButtonFactory);

            listBox.ItemTemplate.VisualTree = factory;
            leftStack.Children.Add(listBox);

            leftPanel.Child = leftStack;
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = new Border
            {
                Padding = new Thickness(20)
            };

            var rightStack = new StackPanel();

            rightStack.Children.Add(new TextBlock
            {
                Text = "Добавить новую аллергию",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var infoBlock = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(20, 33, 150, 243)),
                BorderBrush = Brushes.LightBlue,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var infoStack = new StackPanel();

            var infoRow1 = new TextBlock
            {
                Text = "Если вы не нашли нужный аллерген в списке, можете добавить свой",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            infoStack.Children.Add(infoRow1);

            var attentionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var attentionIcon = new TextBlock
            {
                Text = "⚠️",
                FontSize = 14,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var attentionText = new TextBlock
            {
                Text = "ВНИМАНИЕ",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkOrange
            };

            attentionPanel.Children.Add(attentionIcon);
            attentionPanel.Children.Add(attentionText);
            infoStack.Children.Add(attentionPanel);

            var infoRow3 = new TextBlock
            {
                Text = "При добавлении своего аллергена, постарайтесь указать правильное название/формулировку. " +
                       "При возможности лучше всего указать аллерген в той формулировке, в которой он может быть указан " +
                       "в составе потенциальных препаратов, чтобы вы могли получать предупреждения в случае совпадения.",
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.DimGray
            };
            infoStack.Children.Add(infoRow3);

            infoBlock.Child = infoStack;
            rightStack.Children.Add(infoBlock);

            var inputPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            inputPanel.Children.Add(new TextBlock
            {
                Text = "Аллерген*",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var textBox = new TextBox
            {
                FontSize = 14,
                MinHeight = 35,
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };

            textBox.Tag = new object[] { ownerType, ownerId, allergies };

            var popup = new Popup
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = textBox,
                StaysOpen = false,
                MaxHeight = 200,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Slide,
                IsOpen = false
            };

            var suggestionListBox = new ListBox
            {
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                MaxHeight = 200,
                ItemTemplate = CreateSuggestionItemTemplate()
            };

            suggestionListBox.MouseUp += (s, e) =>
            {
                if (suggestionListBox.SelectedItem != null)
                {
                    textBox.Text = suggestionListBox.SelectedItem.ToString();
                    popup.IsOpen = false;
                    textBox.Focus();
                    textBox.CaretIndex = textBox.Text.Length;
                }
            };

            popup.Child = suggestionListBox;

            textBox.TextChanged += (sender, eArgs) =>
            {
                TextBox_TextChanged(textBox, popup, suggestionListBox, allergies);
            };

            textBox.PreviewKeyDown += (sender, eArgs) =>
            {
                TextBox_PreviewKeyDown(textBox, eArgs, popup, suggestionListBox, allergies);
            };

            textBox.LostFocus += (s, eArgs) =>
            {
                popup.IsOpen = false;
            };

            inputPanel.Children.Add(textBox);
            rightStack.Children.Add(inputPanel);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var addButton = new Button
            {
                Content = "Добавить",
                Style = (Style)FindResource("BtnActivestyle"),
                Padding = new Thickness(15)
            };

            addButton.Tag = new object[] { textBox, popup, ownerType, ownerId, allergies };

            addButton.Click += AddAllergyButton_Click;

            var clearButton = new Button
            {
                Content = "Очистить",
                Style = (Style)FindResource("BtnActive2style"),
                Padding = new Thickness(15),
                Margin = new Thickness(10, 0, 0, 0)
            };

            clearButton.Click += (s, e) =>
            {
                textBox.Clear();
                popup.IsOpen = false;
            };

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(clearButton);
            rightStack.Children.Add(buttonPanel);

            rightPanel.Child = rightStack;
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        private DataTemplate CreateSuggestionItemTemplate()
        {
            var template = new DataTemplate();
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("."));
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, 14.0);
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(5));
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            template.VisualTree = textBlockFactory;
            return template;
        }

        private void TextBox_TextChanged(TextBox textBox, Popup popup, ListBox suggestionListBox, ObservableCollection<Аллергии> allergies)
        {
            if (textBox == null)
            {
                return;
            }

            string inputText = textBox.Text;

            if (string.IsNullOrWhiteSpace(inputText))
            {
                popup.IsOpen = false;
                return;
            }

            var suggestions = _allAllergens.Where(a => a.IndexOf(inputText, StringComparison.OrdinalIgnoreCase) >= 0).Take(10) .ToList();

            var existingAllergies = allergies.Select(a => a.Аллерген).Where(a => a.IndexOf(inputText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            var allSuggestions = suggestions.Union(existingAllergies).Distinct().Take(10).ToList();

            if (allSuggestions.Any())
            {
                suggestionListBox.ItemsSource = allSuggestions;
                popup.Width = textBox.ActualWidth;
                popup.IsOpen = true;

                if (suggestionListBox.Items.Count > 0)
                {
                    suggestionListBox.SelectedIndex = 0;
                }
            }
            else
            {
                popup.IsOpen = false;
            }
        }

        private void TextBox_PreviewKeyDown(TextBox textBox, KeyEventArgs e, Popup popup, ListBox suggestionListBox, ObservableCollection<Аллергии> allergies)
        {
            if (popup.IsOpen)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        if (suggestionListBox.SelectedIndex < suggestionListBox.Items.Count - 1)
                            suggestionListBox.SelectedIndex++;
                        e.Handled = true;
                        break;
                    case Key.Up:
                        if (suggestionListBox.SelectedIndex > 0)
                            suggestionListBox.SelectedIndex--;
                        e.Handled = true;
                        break;
                    case Key.Enter:
                        if (suggestionListBox.SelectedItem != null)
                        {
                            textBox.Text = suggestionListBox.SelectedItem.ToString();
                            popup.IsOpen = false;
                            e.Handled = true;
                        }
                        else
                        {
                            AddAllergy(textBox, allergies);
                            textBox.Clear();
                            e.Handled = true;
                        }
                        break;
                    case Key.Escape:
                        popup.IsOpen = false;
                        e.Handled = true;
                        break;
                    case Key.Tab:
                        if (suggestionListBox.SelectedItem != null)
                        {
                            textBox.Text = suggestionListBox.SelectedItem.ToString();
                            popup.IsOpen = false;
                            e.Handled = true;
                        }
                        break;
                }
            }
            else if (e.Key == Key.Enter)
            {
                AddAllergy(textBox, allergies);
                textBox.Clear();
                e.Handled = true;
            }
        }

        private void AddAllergyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is object[] tagData && tagData.Length >= 5)
            {
                var textBox = tagData[0] as TextBox;
                var popup = tagData[1] as Popup;
                var ownerType = tagData[2] as string;
                var ownerId = (int)tagData[3];
                var allergies = tagData[4] as ObservableCollection<Аллергии>;

                popup.IsOpen = false;

                if (textBox != null && allergies != null)
                {
                    AddAllergy(textBox, allergies, ownerType, ownerId);
                }

                textBox.Clear();
            }
        }

        private void AddAllergy(TextBox textBox, ObservableCollection<Аллергии> allergies, string ownerType = null, int ownerId = -1)
        {
            if (ownerType == null && ownerId == -1 && textBox.Tag is object[] textBoxTagData && textBoxTagData.Length >= 3)
            {
                ownerType = textBoxTagData[0] as string;
                ownerId = (int)textBoxTagData[1];
            }

            string allergen = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(allergen))
            {
                MessageBox.Show("Введите название аллергена", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (allergies.Any(a => a.Аллерген.Equals(allergen, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Эта аллергия уже добавлена", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAllergy = new Аллергии
            {
                Аллерген = allergen
            };

            if (ownerType == "User")
            {
                newAllergy.FkIdПользователя = _userId;
                newAllergy.FkIdПолучателя = null;
            }
            else if (ownerType == "Recipient")
            {
                newAllergy.FkIdПолучателя = ownerId;
                newAllergy.FkIdПользователя = null;
            }

            _context.Аллергииs.Add(newAllergy);
            _context.SaveChanges();

            allergies.Add(newAllergy);

            var sortedAllergies = new ObservableCollection<Аллергии>(allergies.OrderBy(a => a.Аллерген));
            allergies.Clear();
            foreach (var item in sortedAllergies)
            {
                allergies.Add(item);
            }

            UpdateTabContent(ownerType, ownerId);
        }

        private void DeleteAllergyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Аллергии allergy)
            {
                var result = MessageBox.Show($"Удалить аллергию \"{allergy.Аллерген}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string ownerType = null;
                    int ownerId = -1;

                    if (allergy.FkIdПользователя == _userId)
                    {
                        ownerType = "User";
                        ownerId = _userId;
                        _userAllergies.Remove(allergy);
                    }
                    else
                    {
                        ownerType = "Recipient";
                        ownerId = allergy.FkIdПолучателя ?? -1;
                        if (_recipientAllergies.ContainsKey(ownerId))
                        {
                            _recipientAllergies[ownerId].Remove(allergy);
                        }
                    }

                    _context.Аллергииs.Remove(allergy);
                    _context.SaveChanges();

                    if (ownerType != null && ownerId != -1)
                    {
                        UpdateTabContent(ownerType, ownerId);
                    }

                    MessageBox.Show("Аллергия удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void UpdateTabContent(string ownerType, int ownerId)
        {
            foreach (TabItem tabItem in AllergiesTabControl.Items)
            {
                if (tabItem.Tag is object[] ownerData && ownerData.Length >= 2)
                {
                    string tabOwnerType = ownerData[0] as string;
                    int tabOwnerId = (int)ownerData[1];

                    if (tabOwnerType == ownerType && tabOwnerId == ownerId)
                    {
                        ObservableCollection<Аллергии> allergies = null;
                        string ownerName = "";

                        if (ownerType == "User")
                        {
                            allergies = _userAllergies;
                            ownerName = "Вы";
                        }
                        else if (ownerType == "Recipient")
                        {
                            allergies = _recipientAllergies.ContainsKey(ownerId) ? _recipientAllergies[ownerId] : null;
                            var recipient = _recipients.FirstOrDefault(r => r.PkIdПолучателя == ownerId);
                            ownerName = recipient?.Имя ?? "Получатель";
                        }

                        if (allergies != null)
                        {
                            tabItem.Content = CreateTabContent(ownerType, ownerId, ownerName, allergies);
                        }
                        break;
                    }
                }
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Home HomeWindow = new Home(_userId);
            HomeWindow.Show();
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