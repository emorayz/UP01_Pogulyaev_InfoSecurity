using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InfoSecurity.Pages
{
    public class IncidentListItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime IncidentDate { get; set; }
        public int Severity { get; set; }
        public string Status { get; set; }
        public string AssetName { get; set; }
        public string AssignedUser { get; set; }
    }

    public partial class UserPage : Page
    {
        private List<IncidentListItem> allItems = new List<IncidentListItem>();

        public UserPage()
        {
            InitializeComponent();
            WelcomeText.Text = "Добро пожаловать, " + Core.CurrentUser.FIO;
            StatusFilter.Items.Add("Все статусы");
            foreach (string status in new[] { "Новый", "В работе", "Решён", "Закрыт" })
                StatusFilter.Items.Add(status);
            StatusFilter.SelectedIndex = 0;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = Core.CreateContext())
                {
                    allItems = db.Incidents.Select(x => new IncidentListItem
                    {
                        ID = x.ID,
                        Title = x.Title,
                        Description = x.Description,
                        IncidentDate = x.IncidentDate,
                        Severity = x.Severity,
                        Status = x.Status,
                        AssetName = x.Asset.Name,
                        AssignedUser = x.AssignedUser == null ? "Не назначен" : x.AssignedUser.FIO
                    }).ToList();
                }
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Core.ShowError("Не удалось загрузить инциденты: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (SearchBox == null || StatusFilter == null || SortBox == null)
                return;

            IEnumerable<IncidentListItem> query = allItems;
            string search = SearchBox.Text.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => (x.Title + " " + x.Description).ToLowerInvariant().Contains(search));

            if (StatusFilter.SelectedIndex > 0)
                query = query.Where(x => x.Status == StatusFilter.SelectedItem.ToString());

            switch (SortBox.SelectedIndex)
            {
                case 1: query = query.OrderBy(x => x.IncidentDate); break;
                case 2: query = query.OrderByDescending(x => x.Severity).ThenByDescending(x => x.IncidentDate); break;
                case 3: query = query.OrderBy(x => x.Title); break;
                default: query = query.OrderByDescending(x => x.IncidentDate); break;
            }
            IncidentsGrid.ItemsSource = query.ToList();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            StatusFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            ApplyFilters();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
