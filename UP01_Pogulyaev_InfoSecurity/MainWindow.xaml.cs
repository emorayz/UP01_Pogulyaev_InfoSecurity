using InfoSecurity.Pages;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace InfoSecurity
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AuthPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            timer.Tick += (o, t) => DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            timer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !Core.AskYesNo("Вы действительно хотите закрыть приложение?");
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Toggle();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!Core.AskYesNo("Вы действительно хотите выйти из учётной записи?"))
                return;

            AuditService.Log("Выход", "Пользователь вышел из системы");
            Core.Logout();
            MainFrame.Navigate(new AuthPage());
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (!(MainFrame.Content is Page page))
                return;

            Title = "Информационная безопасность — " + page.Title;
            PageTitle.Text = page.Title;
            BtnBack.Visibility = page is AuthPage || page is UserPage || page is AdminPage
                ? Visibility.Collapsed : Visibility.Visible;
            BtnLogout.Visibility = Core.CurrentUser == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
