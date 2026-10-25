using System.Windows;

namespace InfoSecurity
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.ApplySavedTheme();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ThemeManager.SavePreference();
            base.OnExit(e);
        }
    }
}
