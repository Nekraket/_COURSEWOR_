using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Курсовая.View.Windows
{
    public partial class ValueInputDialog : Window
    {
        public float? Result { get; private set; }

        public ValueInputDialog(string title, string description, string unit, string? defaultValue = null)
        {
            InitializeComponent();

            TitleText.Text = title;
            DescriptionText.Text = description;
            UnitText.Text = unit;

            if (!string.IsNullOrEmpty(defaultValue))
            {
                ValueTextBox.Text = defaultValue;
                ValidateInput(defaultValue);
            }

            Loaded += (s, e) =>
            {
                ValueTextBox.Focus();
                ValueTextBox.SelectAll();
            };
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput(ValueTextBox.Text);
        }

        private void ValidateInput(string text)
        {
            var trimmedText = text.Trim();

            if (string.IsNullOrWhiteSpace(trimmedText))
            {
                HintText.Text = "Введите значение";
                HintText.Foreground = System.Windows.Media.Brushes.Gray;
                BtnOK.IsEnabled = false;
                return;
            }

            var cleanText = trimmedText.Replace(',', '.');

            if (float.TryParse(cleanText, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                HintText.Text = "Корректное значение";
                HintText.Foreground = System.Windows.Media.Brushes.Green;
                BtnOK.IsEnabled = true;
            }
            else
            {
                HintText.Text = "Некорректное число";
                HintText.Foreground = System.Windows.Media.Brushes.Red;
                BtnOK.IsEnabled = false;
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            var text = ValueTextBox.Text.Trim();

            if (float.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                Result = value;
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}