using System.Windows;
using System.Windows.Controls;

namespace InfoSecurity
{
    /// <summary>
    /// Передаёт длину пароля в шаблон PasswordBox для корректного отображения подсказки.
    /// Свойство Password само по себе не является DependencyProperty и не может использоваться в Trigger.
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached(
                "PasswordLength",
                typeof(int),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(0));

        public static int GetPasswordLength(DependencyObject element)
        {
            return (int)element.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject element, int value)
        {
            element.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached(
                "IsMonitoring",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnIsMonitoringChanged));

        public static bool GetIsMonitoring(DependencyObject element)
        {
            return (bool)element.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject element, bool value)
        {
            element.SetValue(IsMonitoringProperty, value);
        }

        private static void OnIsMonitoringChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;

            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            if ((bool)e.NewValue)
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;

            SetPasswordLength(passwordBox, passwordBox.Password.Length);
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            SetPasswordLength(passwordBox, passwordBox.Password.Length);
        }
    }
}
