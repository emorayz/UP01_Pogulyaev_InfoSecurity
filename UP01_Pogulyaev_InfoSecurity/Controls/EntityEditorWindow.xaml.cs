using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace InfoSecurity.Controls
{
    public partial class EntityEditorWindow : Window
    {
        private readonly Type entityType;
        private readonly object entity;
        private readonly bool isNew;
        private readonly Dictionary<PropertyInfo, Control> controls = new Dictionary<PropertyInfo, Control>();
        private PasswordBox passwordBox;

        public EntityEditorWindow(Type type, object current)
        {
            entityType = type;
            entity = current ?? Activator.CreateInstance(type);
            isNew = current == null;
            InitializeComponent();
            HeaderText.Text = (isNew ? "Добавление: " : "Редактирование: ") + Title;
            BuildForm();
        }

        private void BuildForm()
        {
            var properties = EntityMetadata.GetScalarProperties(entityType, true);
            FieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(215) });
            FieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            int row = 0;

            foreach (var property in properties)
            {
                AddRow(property, row++);
            }

            if (entityType == typeof(User))
            {
                FieldsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var label = new Label { Content = isNew ? "Пароль" : "Новый пароль (необязательно)" };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                FieldsGrid.Children.Add(label);
                passwordBox = new PasswordBox
                {
                    Style = FindResource("HintPasswordBoxStyle") as Style,
                    Tag = isNew ? "Не менее 6 символов и одна цифра" : "Оставьте пустым, чтобы не менять",
                    ToolTip = "Пароль сохраняется только в виде хэша SHA-256"
                };
                Grid.SetRow(passwordBox, row);
                Grid.SetColumn(passwordBox, 1);
                FieldsGrid.Children.Add(passwordBox);
            }
        }

        private void AddRow(PropertyInfo property, int row)
        {
            FieldsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new Label { Content = EntityMetadata.DisplayName(property) };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            FieldsGrid.Children.Add(label);

            object value = property.GetValue(entity);
            Control input;
            Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (type == typeof(bool))
            {
                input = new CheckBox
                {
                    IsChecked = value != null && (bool)value,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    ToolTip = "Установите или снимите флажок"
                };
            }
            else if (property.Name == nameof(User.Role))
            {
                var combo = new ComboBox { ToolTip = "Выберите роль пользователя" };
                combo.Items.Add("User");
                combo.Items.Add("Admin");
                combo.SelectedItem = Convert.ToString(value) ?? "User";
                input = combo;
            }
            else if (property.Name == nameof(SecurityIncident.Status))
            {
                var combo = new ComboBox { ToolTip = "Выберите статус обработки инцидента" };
                foreach (string status in new[] { "Новый", "В работе", "Решён", "Закрыт" })
                    combo.Items.Add(status);
                combo.SelectedItem = Convert.ToString(value) ?? "Новый";
                input = combo;
            }
            else
            {
                string text = value is DateTime date
                    ? date.ToString("dd.MM.yyyy HH:mm")
                    : Convert.ToString(value);
                if (isNew && type == typeof(DateTime) && (value == null || (value is DateTime currentDate && currentDate == default(DateTime))))
                    text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                if (isNew && type == typeof(int) && property.Name != "ID" && !property.Name.EndsWith("ID"))
                    text = "1";
                input = new TextBox
                {
                    Text = text,
                    Style = FindResource("HintTextBoxStyle") as Style,
                    Tag = GetHint(property),
                    ToolTip = GetHint(property),
                    AcceptsReturn = property.Name == "Description" || property.Name == "Content" || property.Name == "Mitigation",
                    TextWrapping = property.Name == "Description" || property.Name == "Content" || property.Name == "Mitigation"
                        ? TextWrapping.Wrap : TextWrapping.NoWrap,
                    Height = property.Name == "Description" || property.Name == "Content" || property.Name == "Mitigation" ? 85 : 34
                };
            }

            Grid.SetRow(input, row);
            Grid.SetColumn(input, 1);
            FieldsGrid.Children.Add(input);
            controls.Add(property, input);
        }

        private static string GetHint(PropertyInfo property)
        {
            Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (type == typeof(DateTime)) return "Формат: дд.мм.гггг чч:мм";
            if (type == typeof(int)) return "Введите целое число";
            if (property.Name.EndsWith("ID")) return "Укажите ID существующей связанной записи";
            return "Введите значение поля";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var pair in controls)
                    ReadValue(pair.Key, pair.Value);

                if (entityType == typeof(User))
                    PrepareUserPassword();

                var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (!Validator.TryValidateObject(entity, new ValidationContext(entity), results, true))
                {
                    Core.ShowError(string.Join("\n", results.Select(x => x.ErrorMessage)));
                    return;
                }

                using (var db = Core.CreateContext())
                {
                    ValidateBusinessRules(db);
                    if (isNew)
                        db.Set(entityType).Add(entity);
                    else
                    {
                        db.Set(entityType).Attach(entity);
                        db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                Core.ShowError("Проверьте введённые данные. " + ex.Message);
            }
        }

        private void ReadValue(PropertyInfo property, Control input)
        {
            if (input is CheckBox checkBox)
            {
                property.SetValue(entity, checkBox.IsChecked == true);
                return;
            }

            string text = input is ComboBox combo
                ? Convert.ToString(combo.SelectedItem)
                : ((TextBox)input).Text.Trim();
            Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            bool nullable = Nullable.GetUnderlyingType(property.PropertyType) != null;

            if (string.IsNullOrWhiteSpace(text))
            {
                if (type == typeof(string) || nullable)
                {
                    property.SetValue(entity, null);
                    return;
                }
                throw new InvalidOperationException("Поле «" + EntityMetadata.DisplayName(property) + "» не заполнено.");
            }

            if (type == typeof(string)) property.SetValue(entity, text);
            else if (type == typeof(int)) property.SetValue(entity, int.Parse(text, CultureInfo.CurrentCulture));
            else if (type == typeof(bool)) property.SetValue(entity, bool.Parse(text));
            else if (type == typeof(decimal)) property.SetValue(entity, decimal.Parse(text, CultureInfo.CurrentCulture));
            else if (type == typeof(double)) property.SetValue(entity, double.Parse(text, CultureInfo.CurrentCulture));
            else if (type == typeof(DateTime))
            {
                if (!DateTime.TryParse(text, new CultureInfo("ru-RU"), DateTimeStyles.None, out DateTime date))
                    throw new InvalidOperationException("Некорректная дата в поле «" + EntityMetadata.DisplayName(property) + "».");
                property.SetValue(entity, date);
            }
        }

        private void PrepareUserPassword()
        {
            var user = (User)entity;
            if (!isNew && string.IsNullOrEmpty(passwordBox.Password))
                return;
            if (!SecurityHelper.IsPasswordValid(passwordBox.Password, out string error))
                throw new InvalidOperationException(error);
            user.PasswordHash = SecurityHelper.GetHash(passwordBox.Password);
        }

        private void ValidateBusinessRules(InfoSecurityEntities db)
        {
            if (entity is User user)
            {
                if (!Regex.IsMatch(user.Login ?? "", "^[A-Za-z0-9]+$"))
                    throw new InvalidOperationException("Логин должен содержать только английские буквы и цифры.");
                if (user.Role != "User" && user.Role != "Admin")
                    throw new InvalidOperationException("Допустимы только роли User и Admin.");
                if (db.Users.Any(x => x.Login == user.Login && x.ID != user.ID))
                    throw new InvalidOperationException("Такой логин уже существует.");
            }
            if (entity is Employee employee)
            {
                if (!db.Departments.Any(x => x.ID == employee.DepartmentID))
                    throw new InvalidOperationException("Подразделение с указанным ID не найдено.");
                if (employee.UserID.HasValue && !db.Users.Any(x => x.ID == employee.UserID.Value))
                    throw new InvalidOperationException("Пользователь с указанным ID не найден.");
            }
            if (entity is SecurityAsset asset && !db.Departments.Any(x => x.ID == asset.DepartmentID))
                throw new InvalidOperationException("Подразделение с указанным ID не найдено.");
            if (entity is SecurityIncident incident)
            {
                if (!db.Assets.Any(x => x.ID == incident.AssetID))
                    throw new InvalidOperationException("Актив с указанным ID не найден.");
                if (incident.AssignedUserID.HasValue && !db.Users.Any(x => x.ID == incident.AssignedUserID.Value))
                    throw new InvalidOperationException("Ответственный пользователь не найден.");
            }
            if (entity is RiskAssessment risk && !db.Assets.Any(x => x.ID == risk.AssetID))
                throw new InvalidOperationException("Актив с указанным ID не найден.");
            if (entity is SecurityPolicy policy && !db.Departments.Any(x => x.ID == policy.DepartmentID))
                throw new InvalidOperationException("Подразделение с указанным ID не найдено.");
            if (entity is AuditEvent audit && !db.Users.Any(x => x.ID == audit.UserID))
                throw new InvalidOperationException("Пользователь с указанным ID не найден.");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
