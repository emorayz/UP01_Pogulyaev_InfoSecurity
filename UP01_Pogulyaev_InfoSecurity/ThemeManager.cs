using System;
using System.IO;
using System.Windows;

namespace InfoSecurity
{
    public static class ThemeManager
    {
        private const string DefaultTheme = "Themes/Dictionary.xaml";
        private const string AlternativeTheme = "Themes/PurpleDictionary.xaml";
        private static readonly string PreferencePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UP01_Pogulyaev_InfoSecurity", "theme.txt");

        public static string CurrentTheme { get; private set; } = DefaultTheme;

        public static void ApplySavedTheme()
        {
            string theme = DefaultTheme;
            try
            {
                if (File.Exists(PreferencePath))
                {
                    string saved = File.ReadAllText(PreferencePath).Trim();
                    if (saved == DefaultTheme || saved == AlternativeTheme)
                        theme = saved;
                }
            }
            catch (IOException)
            {
                // При недоступности файла используется тема по умолчанию.
            }
            Apply(theme);
        }

        public static void Toggle()
        {
            Apply(CurrentTheme == DefaultTheme ? AlternativeTheme : DefaultTheme);
            SavePreference();
        }

        public static void Apply(string themePath)
        {
            var dictionary = Application.LoadComponent(new Uri(themePath, UriKind.Relative)) as ResourceDictionary;
            if (dictionary == null)
                return;

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            CurrentTheme = themePath;
        }

        public static void SavePreference()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PreferencePath));
                File.WriteAllText(PreferencePath, CurrentTheme);
            }
            catch (IOException)
            {
                // Настройка темы не должна препятствовать закрытию приложения.
            }
        }
    }
}
