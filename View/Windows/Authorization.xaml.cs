using System.Windows;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Authorization : Window
    {
        private HealthcareManagementContext _context;
        public Authorization()
        {
            InitializeComponent();
            _context = new HealthcareManagementContext();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BoxUserLogin.Text) && !string.IsNullOrWhiteSpace(BoxUserEmail.Text))
            {
                Пользователь? autUser = _context.Пользовательs.FirstOrDefault(q => q.Логин == BoxUserLogin.Text && q.Email == BoxUserEmail.Text);

                if (autUser != null)
                {
                    Home HomeWindow = new Home(autUser.PkIdПользователя);
                    HomeWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверные учетные данные");
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля!");
            }
        }

        private void RegisterHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Registration registrationWindow = new Registration();
            registrationWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
