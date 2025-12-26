using System.Windows;
using System.Windows.Controls;
using Курсовая.Models;
using Курсовая.View.Windows;

namespace Курсовая.View.UserControls
{
    public partial class RecipientCard : UserControl
    {
        public RecipientCard()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadAvatar();
            Unloaded += (s, e) => ClearImage();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = Window.GetWindow(this) as Settings;
            if (settingsWindow != null && DataContext is ПолучателиУхода recipient)
            {
                settingsWindow.DeleteRecipient(recipient);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = Window.GetWindow(this) as Settings;
            if (settingsWindow != null && DataContext is ПолучателиУхода recipient)
            {
                settingsWindow.EditRecipient(recipient);
            }
        }

        private void LoadAvatar()
        {
            if (DataContext is not ПолучателиУхода recipient)
            {
                return;
            }

            RecipientAvatarImage.Source = Settings.GetRecipientAvatarImage(recipient);
        }

        private void ClearImage()
        {
            RecipientAvatarImage.Source = null;
        }
    }
}