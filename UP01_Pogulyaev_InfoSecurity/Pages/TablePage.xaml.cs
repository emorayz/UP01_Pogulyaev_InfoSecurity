using InfoSecurity.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace InfoSecurity.Pages
{
    public partial class TablePage : Page
    {
        private readonly Type entityType;
        private readonly List<PropertyInfo> properties;
        private readonly string filterProperty;
        private List<object> allItems = new List<object>();

        public TablePage(Type type, string title)
        {
            entityType = type;
            Title = title;
            properties = EntityMetadata.GetScalarProperties(entityType);
            filterProperty = EntityMetadata.GetFilterProperty(entityType);
            InitializeComponent();
            HeaderText.Text = title;
            BuildColumns();
            BuildSortAndFilter();
            LoadData();
        }

        private void BuildColumns()
        {
            foreach (var property in properties)
            {
                var binding = new Binding(property.Name);
                if (Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime) || property.PropertyType == typeof(DateTime))
                    binding.StringFormat = "dd.MM.yyyy HH:mm";
                DataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = EntityMetadata.DisplayName(property),
                    Binding = binding,
                    Width = property.Name == "Description" || property.Name == "Content" || property.Name == "Mitigation"
                        ? new DataGridLength(1, DataGridLengthUnitType.Star)
                        : new DataGridLength(1, DataGridLengthUnitType.Auto)
                });
            }
        }

        private void BuildSortAndFilter()
        {
            SortCombo.Items.Add(new ComboBoxItem { Content = "Без сортировки", Tag = "" });
            foreach (var property in properties)
                SortCombo.Items.Add(new ComboBoxItem { Content = EntityMetadata.DisplayName(property), Tag = property.Name });
            SortCombo.SelectedIndex = 0;

            if (string.IsNullOrEmpty(filterProperty))
                FilterCombo.Visibility = Visibility.Collapsed;
        }

        private void LoadData()
        {
            try
            {
                using (var db = Core.CreateContext())
                    allItems = db.Set(entityType).AsNoTracking().Cast<object>().ToList();
                UpdateFilterValues();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Core.ShowError("Не удалось загрузить таблицу: " + ex.Message);
            }
        }

        private void UpdateFilterValues()
        {
            if (string.IsNullOrEmpty(filterProperty))
                return;

            string previous = FilterCombo.SelectedIndex > 0 ? FilterCombo.SelectedItem.ToString() : null;
            var property = entityType.GetProperty(filterProperty);
            var values = allItems.Select(x => Convert.ToString(property.GetValue(x)))
                .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
            FilterCombo.Items.Clear();
            FilterCombo.Items.Add("Все значения");
            foreach (string value in values)
                FilterCombo.Items.Add(value);
            FilterCombo.SelectedIndex = previous == null ? 0 : Math.Max(0, FilterCombo.Items.IndexOf(previous));
        }

        private void ApplyFilters()
        {
            if (SearchBox == null || SortCombo == null || FilterCombo == null)
                return;

            IEnumerable<object> query = allItems;
            string search = SearchBox.Text.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(item => properties.Any(p =>
                    Convert.ToString(p.GetValue(item))?.ToLowerInvariant().Contains(search) == true));
            }

            if (!string.IsNullOrEmpty(filterProperty) && FilterCombo.SelectedIndex > 0)
            {
                var property = entityType.GetProperty(filterProperty);
                query = query.Where(item => Convert.ToString(property.GetValue(item)) == FilterCombo.SelectedItem.ToString());
            }

            if (SortCombo.SelectedItem is ComboBoxItem selected && !string.IsNullOrEmpty(selected.Tag?.ToString()))
            {
                var property = entityType.GetProperty(selected.Tag.ToString());
                query = query.OrderBy(item => property.GetValue(item));
            }
            DataGrid.ItemsSource = query.ToList();
        }

        private bool OpenEditor(object entity)
        {
            var editor = new EntityEditorWindow(entityType, entity) { Owner = Window.GetWindow(this) };
            bool result = editor.ShowDialog() == true;
            if (result)
            {
                AuditService.Log(entity == null ? "Добавление" : "Редактирование",
                    (entity == null ? "Добавлена" : "Изменена") + " запись: " + Title);
                LoadData();
            }
            return result;
        }

        private void Add_Click(object sender, RoutedEventArgs e) => OpenEditor(null);

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem == null)
            {
                Core.ShowError("Выберите запись для редактирования.");
                return;
            }
            OpenEditor(DataGrid.SelectedItem);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGrid.SelectedItems.Cast<object>().ToList();
            if (selected.Count == 0)
            {
                Core.ShowError("Выберите хотя бы одну запись для удаления.");
                return;
            }
            if (!Core.AskYesNo("Вы действительно хотите удалить записи в количестве " + selected.Count + "? Связанные данные могут быть удалены каскадно."))
                return;

            try
            {
                using (var db = Core.CreateContext())
                {
                    var set = db.Set(entityType);
                    foreach (object item in selected)
                    {
                        if (item is User user && Core.CurrentUser != null && user.ID == Core.CurrentUser.ID)
                            throw new InvalidOperationException("Нельзя удалить учётную запись, под которой выполнен вход.");
                        set.Attach(item);
                        set.Remove(item);
                    }
                    db.SaveChanges();
                }
                AuditService.Log("Удаление", "Удалено записей: " + selected.Count + "; таблица: " + Title);
                LoadData();
            }
            catch (Exception ex)
            {
                Core.ShowError("Удаление не выполнено: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadData();

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            FilterCombo.SelectedIndex = 0;
            SortCombo.SelectedIndex = 0;
            ApplyFilters();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilters();

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedItem != null)
                OpenEditor(DataGrid.SelectedItem);
        }
    }
}
