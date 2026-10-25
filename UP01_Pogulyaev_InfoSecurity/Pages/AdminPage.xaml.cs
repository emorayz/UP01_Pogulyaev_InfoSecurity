using System;
using System.Windows;
using System.Windows.Controls;

namespace InfoSecurity.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            AdminWelcome.Text = "Администратор: " + Core.CurrentUser.FIO;
        }

        private void OpenTable(Type type, string title)
        {
            NavigationService.Navigate(new TablePage(type, title));
        }

        private void Users_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(User), "Пользователи");
        private void Departments_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(Department), "Подразделения");
        private void Employees_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(Employee), "Сотрудники");
        private void Assets_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(SecurityAsset), "Информационные активы");
        private void Incidents_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(SecurityIncident), "Инциденты безопасности");
        private void Risks_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(RiskAssessment), "Оценки рисков");
        private void Policies_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(SecurityPolicy), "Политики безопасности");
        private void Audit_Click(object sender, RoutedEventArgs e) => OpenTable(typeof(AuditEvent), "Журнал аудита");
        private void Dashboard_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DashboardPage());

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            ExportService.ExportWord();
        }

        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            ExportService.ExportPdf();
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportService.ExportExcel();
        }
    }
}
