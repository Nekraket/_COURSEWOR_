using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Курсовая.View.Windows
{
    public partial class SymptomRatingDialog : Window
    {
        public int WellbeingRating { get; private set; }
        public string Notes { get; private set; }

        public SymptomRatingDialog(string symptomName)
        {
            InitializeComponent();
            InitializeDialog(symptomName, null, null);
        }

        public SymptomRatingDialog(string symptomName, int? defaultRating, string defaultNotes)
        {
            InitializeComponent();
            InitializeDialog(symptomName, defaultRating, defaultNotes);
        }

        private void InitializeDialog(string symptomName, int? defaultRating, string defaultNotes)
        {
            Title = $"Оценка симптома: {symptomName}";
            SymptomNameText.Text = $"Симптом: {symptomName}";

            CreateRatingButtons();

            if (defaultRating.HasValue && defaultRating.Value >= 1 && defaultRating.Value <= 10)
            {
                RatingTextBox.Text = defaultRating.Value.ToString();
                WellbeingRating = defaultRating.Value;
                HighlightSelectedRating(defaultRating.Value);
            }

            if (!string.IsNullOrEmpty(defaultNotes))
            {
                NotesTextBox.Text = defaultNotes;
                Notes = defaultNotes;
            }

            Loaded += (s, e) =>
            {
                if (string.IsNullOrEmpty(RatingTextBox.Text))
                    RatingTextBox.Focus();
                else
                    NotesTextBox.Focus();
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && BtnOK.IsEnabled)
                {
                    BtnOK_Click(null, null);
                }
                else if (e.Key == Key.Escape)
                {
                    BtnCancel_Click(null, null);
                }
            };
        }

        private void CreateRatingButtons()
        {
            for (int i = 1; i <= 10; i++)
            {
                var button = new Button
                {
                    Content = i.ToString(),
                    Tag = i,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(2),
                    Background = Brushes.White,
                    Foreground = Brushes.Gray,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    FontSize = 12,
                    Cursor = Cursors.Hand
                };

                button.Click += (s, e) =>
                {
                    if (s is Button ratingButton && ratingButton.Tag is int rating)
                    {
                        RatingTextBox.Text = rating.ToString();
                        RatingTextBox.CaretIndex = RatingTextBox.Text.Length;
                        HighlightSelectedRating(rating);
                        ValidateInput();
                    }
                };

                button.MouseEnter += (s, e) =>
                {
                    if (s is Button ratingButton)
                    {
                        ratingButton.Background = Brushes.LightGray;
                    }
                };

                button.MouseLeave += (s, e) =>
                {
                    if (s is Button ratingButton && ratingButton.Tag is int rating)
                    {
                        if (rating != WellbeingRating)
                        {
                            ratingButton.Background = Brushes.White;
                        }
                    }
                };

                RatingButtonsPanel.Children.Add(button);
            }
        }

        private void HighlightSelectedRating(int rating)
        {
            foreach (var child in RatingButtonsPanel.Children)
            {
                if (child is Button button && button.Tag is int buttonRating)
                {
                    if (buttonRating == rating)
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                        button.Foreground = Brushes.White;
                        button.BorderBrush = new SolidColorBrush(Color.FromRgb(245, 124, 0));
                    }
                    else
                    {
                        button.Background = Brushes.White;
                        button.Foreground = Brushes.Gray;
                        button.BorderBrush = Brushes.LightGray;
                    }
                }
            }
        }

        private void RatingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();

            if (int.TryParse(RatingTextBox.Text, out int rating))
            {
                HighlightSelectedRating(rating);
            }
        }

        private void RatingTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }

            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            if (newText.Length > 0)
            {
                if (int.TryParse(newText, out int value))
                {
                    if (value > 10 || value < 1)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void ValidateInput()
        {
            var ratingText = RatingTextBox.Text.Trim();

            if (string.IsNullOrEmpty(ratingText))
            {
                ShowError("Введите оценку от 1 до 10");
                BtnOK.IsEnabled = false;
                return;
            }

            if (int.TryParse(ratingText, out int rating))
            {
                if (rating < 1 || rating > 10)
                {
                    ShowError("Оценка должна быть от 1 до 10");
                    BtnOK.IsEnabled = false;
                    return;
                }

                ClearError();
                BtnOK.IsEnabled = true;
                WellbeingRating = rating;
            }
            else
            {
                ShowError("Введите числовое значение");
                BtnOK.IsEnabled = false;
            }
        }

        private void ShowError(string message)
        {
            HintText.Text = message;
            HintText.Foreground = Brushes.Red;
        }

        private void ClearError()
        {
            HintText.Text = "Введите оценку от 1 до 10";
            HintText.Foreground = Brushes.Gray;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Notes = NotesTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}