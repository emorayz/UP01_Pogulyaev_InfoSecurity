using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InfoSecurity.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFio.Text) || string.IsNullOrWhiteSpace(TxtLogin.Text) ||
                string.IsNullOrWhiteSpace(PassFirst.Password) || string.IsNullOrWhiteSpace(PassSecond.Password))
            {
                Core.ShowError("Заполните все поля.");
                return;
            }

            if (!SecurityHelper.IsPasswordValid(PassFirst.Password, out string passwordError))
            {
                Core.ShowError(passwordError);
                return;
            }

            if (PassFirst.Password != PassSecond.Password)
            {
                Core.ShowError("Пароли не совпадают.");
                return;
            }

            try
            {
                string login = TxtLogin.Text.Trim();
                if (Core.LoginExists(login))
                {
                    Core.ShowError("Пользователь с таким логином уже существует.");
                    return;
                }

                using (var db = Core.CreateContext())
                {
                    db.Users.Add(new User
                    {
                        FIO = TxtFio.Text.Trim(),
                        Login = login,
                        PasswordHash = SecurityHelper.GetHash(PassFirst.Password),
                        Role = "User",
                        IsActive = true
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Core.ShowError("Регистрация не выполнена: " + ex.Message);
                return;
            }

            Core.ShowInfo("Пользователь успешно зарегистрирован. Теперь выполните вход.");
            NavigationService.Navigate(new AuthPage());
        }

        private void TxtFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(char.IsLetter(e.Text, 0) || e.Text == " " || e.Text == "-");
        }

        private void TxtLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char c = e.Text[0];
            bool valid = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
            e.Handled = !valid;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }
    }
}
