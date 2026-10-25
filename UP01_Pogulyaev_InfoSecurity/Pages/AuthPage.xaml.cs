using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InfoSecurity.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                Core.ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                if (Core.Authenticate(TextBoxLogin.Text.Trim(), PasswordBox.Password))
                {
                    AuditService.Log("Авторизация", "Успешный вход в систему");
                    Core.ShowInfo("Добро пожаловать, " + Core.CurrentUser.FIO + "!");
                    NavigationService.Navigate(Core.CurrentUser.Role == "Admin"
                        ? (Page)new AdminPage() : new UserPage());
                    return;
                }
            }
            catch (Exception ex)
            {
                Core.ShowError("Не удалось подключиться к базе данных. Проверьте App.config и выполните db/schema.sql.\n" + ex.Message);
                return;
            }

            failedAttempts++;
            Core.ShowError("Пользователь с такими данными не найден или учётная запись отключена.");
            if (failedAttempts >= 3)
                ShowCaptcha();
        }

        private void ShowCaptcha()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            CaptchaPanel.Visibility = Visibility.Visible;
            RefreshCaptcha();
        }

        private void RefreshCaptcha()
        {
            CaptchaText.Text = CaptchaHelper.Generate();
            CaptchaInput.Clear();
            CaptchaInput.Focus();
        }

        private void SubmitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (!CaptchaHelper.Verify(CaptchaText.Text, CaptchaInput.Text))
            {
                Core.ShowError("Капча введена неверно. Попробуйте ещё раз.");
                RefreshCaptcha();
                return;
            }

            failedAttempts = 0;
            CaptchaPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            PasswordBox.Clear();
            Core.ShowInfo("Капча принята. Теперь повторите вход.");
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            RefreshCaptcha();
        }

        private void Captcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePassPage());
        }
    }
}
