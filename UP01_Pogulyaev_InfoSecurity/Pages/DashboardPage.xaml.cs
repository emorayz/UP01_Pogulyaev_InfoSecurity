using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace InfoSecurity.Pages
{
    public partial class DashboardPage : Page
    {
        private Series series;

        public DashboardPage()
        {
            InitializeComponent();
            ChartIncidents.ChartAreas.Add(new ChartArea("Main"));
            series = new Series("Инциденты") { IsValueShownAsLabel = true };
            ChartIncidents.Series.Add(series);
            ChartIncidents.Legends.Add(new Legend("MainLegend"));
            UpdateChart();
        }

        private void UpdateChart()
        {
            try
            {
                using (var db = Core.CreateContext())
                {
                    var data = db.Incidents.GroupBy(x => x.Status)
                        .Select(x => new { Status = x.Key, Count = x.Count() }).ToList();
                    series.Points.Clear();
                    foreach (var item in data)
                        series.Points.AddXY(item.Status, item.Count);
                }
                series.ChartType = GetChartType();
                ChartIncidents.Titles.Clear();
                ChartIncidents.Titles.Add("Количество инцидентов по статусам");
                ChartIncidents.ChartAreas[0].AxisX.Title = "Статус";
                ChartIncidents.ChartAreas[0].AxisY.Title = "Количество";
            }
            catch (Exception ex)
            {
                Core.ShowError("Не удалось построить диаграмму: " + ex.Message);
            }
        }

        private SeriesChartType GetChartType()
        {
            switch (ChartTypeCombo.SelectedIndex)
            {
                case 1: return SeriesChartType.Bar;
                case 2: return SeriesChartType.Pie;
                case 3: return SeriesChartType.Doughnut;
                case 4: return SeriesChartType.Line;
                default: return SeriesChartType.Column;
            }
        }

        private void ChartType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (series != null)
                series.ChartType = GetChartType();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => UpdateChart();
    }
}
