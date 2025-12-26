using System.Windows;
using System.Windows.Input;
using Курсовая.Models;

namespace Курсовая.View.Windows
{
    public partial class Start : Window
    {
        private int _userId;
        private HealthcareManagementContext _context;

        public Start(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new HealthcareManagementContext();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = _context.Пользовательs.FirstOrDefault(u => u.PkIdПользователя == _userId);

            if (user != null)
            {
                BoxUserLogin.Text = user.Логин;
            }
        }

        private void MedicinesCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Medicines Med = new Medicines(_userId);
            Med.Show();
            this.Close();
        }

        private void MeasurementsCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Measurement Meas = new Measurement(_userId);
            Meas.Show();
            this.Close();
        }

        private void PersonalMedArchiveCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PersonalMedArchive PersMed = new PersonalMedArchive(_userId);
            PersMed.Show();
            this.Close();
        }

        private void MoodCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoodAndSymptoms Mood = new MoodAndSymptoms(_userId);
            Mood.Show();
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Home HomeWindow = new Home(_userId);
            HomeWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}