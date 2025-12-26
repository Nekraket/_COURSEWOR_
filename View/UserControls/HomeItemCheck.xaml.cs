using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Курсовая.View.UserControls
{
    public partial class HomeItemCheck : UserControl
    {
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(HomeItemCheck), new PropertyMetadata("", OnOwnerTextChanged));

        public static readonly DependencyProperty DescriptionTextProperty = DependencyProperty.Register("DescriptionText", typeof(string), typeof(HomeItemCheck), new PropertyMetadata(""));

        public static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register("TimeText", typeof(string), typeof(HomeItemCheck), new PropertyMetadata(""));

        public static readonly DependencyProperty OwnerTextProperty = DependencyProperty.Register("OwnerText", typeof(string), typeof(HomeItemCheck), new PropertyMetadata("", OnOwnerTextChanged));

        public static readonly DependencyProperty IconPathDataProperty = DependencyProperty.Register("IconPathData", typeof(string), typeof(HomeItemCheck), new PropertyMetadata(""));

        public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register("IconColor", typeof(string), typeof(HomeItemCheck), new PropertyMetadata("#000000"));

        public static readonly DependencyProperty IsCompletedProperty = DependencyProperty.Register("IsCompleted", typeof(bool), typeof(HomeItemCheck), new PropertyMetadata(false, OnIsCompletedChanged));

        public static readonly DependencyProperty IsCompletionEnabledProperty = DependencyProperty.Register("IsCompletionEnabled", typeof(bool), typeof(HomeItemCheck), new PropertyMetadata(true));

        public event EventHandler? CompleteTaskClicked;
        public event EventHandler? UncompleteTaskClicked;

        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

        public string DescriptionText
        {
            get => (string)GetValue(DescriptionTextProperty);
            set => SetValue(DescriptionTextProperty, value);
        }

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
        }

        public string OwnerText
        {
            get => (string)GetValue(OwnerTextProperty);
            set => SetValue(OwnerTextProperty, value);
        }

        public string IconPathData
        {
            get => (string)GetValue(IconPathDataProperty);
            set => SetValue(IconPathDataProperty, value);
        }

        public string IconColor
        {
            get => (string)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public bool IsCompleted
        {
            get => (bool)GetValue(IsCompletedProperty);
            set => SetValue(IsCompletedProperty, value);
        }

        public bool IsCompletionEnabled
        {
            get => (bool)GetValue(IsCompletionEnabledProperty);
            set => SetValue(IsCompletionEnabledProperty, value);
        }

        public HomeItemCheck()
        {
            InitializeComponent();
            UpdateOwnerBadgeVisibility();
        }

        private static void OnOwnerTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HomeItemCheck)d;
            control.UpdateOwnerBadgeVisibility();
        }

        private static void OnIsCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HomeItemCheck)d;
            control.UpdateVisualState();
        }

        private void UpdateOwnerBadgeVisibility()
        {
            if (OwnerBadge != null && OwnerTextBlock != null)
            {
                OwnerBadge.Visibility = string.IsNullOrEmpty(OwnerText) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void UpdateVisualState()
        {
            if (CompletionRadioButton != null && OuterBorder != null && TitleTextBlock != null && DescriptionTextBlock != null)
            {
                CompletionRadioButton.IsChecked = IsCompleted;
                CompletionRadioButton.Visibility = IsCompletionEnabled ? Visibility.Visible : Visibility.Collapsed;

                OuterBorder.Opacity = IsCompleted ? 0.7 : 1.0;

                var textDecoration = IsCompleted ? TextDecorations.Strikethrough : null;
                var textColor = IsCompleted ? Colors.Gray : Colors.Black;

                TitleTextBlock.TextDecorations = textDecoration;
                TitleTextBlock.Foreground = new SolidColorBrush(textColor);

                DescriptionTextBlock.TextDecorations = textDecoration;
                DescriptionTextBlock.Foreground = new SolidColorBrush(textColor);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IconPathDataProperty)
            {
                if (!string.IsNullOrEmpty(IconPathData))
                {
                    try
                    {
                        IconPath.Data = Geometry.Parse(IconPathData);
                    }
                    catch
                    {
                        // (просто круг)
                        IconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z");
                    }
                }
            }
            else if (e.Property == IconColorProperty)
            {
                if (!string.IsNullOrEmpty(IconColor))
                {
                    try
                    {
                        IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(IconColor));
                    }
                    catch
                    {
                        IconPath.Fill = Brushes.Black;
                    }
                }
            }
            else if (e.Property == IsCompletedProperty || e.Property == IsCompletionEnabledProperty)
            {
                UpdateVisualState();
            }
            else if (e.Property == OwnerTextProperty)
            {
                UpdateOwnerBadgeVisibility();
            }
        }

        private void CompletionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                CompleteTaskClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CompletionRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsCompleted)
            {
                IsCompleted = false;
                UncompleteTaskClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}