using System.Linq;
using System.Windows;

namespace InfoSecurity
{
    public static class Core
    {
        public static User CurrentUser { get; private set; }

        public static InfoSecurityEntities CreateContext()
        {
            return new InfoSecurityEntities();
        }

        public static bool Authenticate(string login, string password)
        {
            string hash = SecurityHelper.GetHash(password);
            using (var db = CreateContext())
            {
                CurrentUser = db.Users.AsNoTracking()
                    .FirstOrDefault(u => u.Login == login && u.PasswordHash == hash && u.IsActive);
                return CurrentUser != null;
            }
        }

        public static bool LoginExists(string login)
        {
            using (var db = CreateContext())
                return db.Users.Any(u => u.Login == login);
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInfo(string text)
        {
            MessageBox.Show(text, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool AskYesNo(string text)
        {
            return MessageBox.Show(text, "Подтверждение", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
