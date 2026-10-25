using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InfoSecurity.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(CurrentPassword.Password) ||
                string.IsNullOrWhiteSpace(NewPassword.Password) || string.IsNullOrWhiteSpace(ConfirmPassword.Password))
            {
                Core.ShowError("Все поля обязательны для заполнения.");
                return;
            }

            if (NewPassword.Password != ConfirmPassword.Password)
            {
                Core.ShowError("Новые пароли не совпадают.");
                return;
            }

            if (!SecurityHelper.IsPasswordValid(NewPassword.Password, out string error))
            {
                Core.ShowError(error);
                return;
            }

            try
            {
                using (var db = Core.CreateContext())
                {
                    string oldHash = SecurityHelper.GetHash(CurrentPassword.Password);
                    var user = db.Users.FirstOrDefault(u => u.Login == TxtLogin.Text.Trim() && u.PasswordHash == oldHash);
                    if (user == null)
                    {
                        Core.ShowError("Логин или текущий пароль указан неверно.");
                        return;
                    }

                    user.PasswordHash = SecurityHelper.GetHash(NewPassword.Password);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Core.ShowError("Не удалось изменить пароль: " + ex.Message);
                return;
            }

            Core.ShowInfo("Пароль успешно изменён.");
            NavigationService.Navigate(new AuthPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }
    }
}
