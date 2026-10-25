using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace InfoSecurity
{
    public static class ExportService
    {
        private static List<SecurityIncident> GetIncidents()
        {
            using (var db = Core.CreateContext())
                return db.Incidents.Include("Asset").Include("AssignedUser")
                    .OrderByDescending(x => x.IncidentDate).ToList();
        }

        public static void ExportWord()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспорт отчёта в Word",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = "Инциденты_безопасности.docx"
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                CreateWordReport(GetIncidents(), dialog.FileName, null);
                AuditService.Log("Экспорт Word", dialog.FileName);
                Core.ShowInfo("Документ Word успешно создан.");
            }
            catch (Exception ex)
            {
                ShowExportError(ex);
            }
        }

        public static void ExportPdf()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспорт отчёта в PDF",
                Filter = "Документ PDF (*.pdf)|*.pdf",
                FileName = "Инциденты_безопасности.pdf"
            };
            if (dialog.ShowDialog() != true)
                return;

            string tempDocx = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".docx");
            try
            {
                CreateWordReport(GetIncidents(), tempDocx, dialog.FileName);
                AuditService.Log("Экспорт PDF", dialog.FileName);
                Core.ShowInfo("Документ PDF успешно создан.");
            }
            catch (Exception ex)
            {
                ShowExportError(ex);
            }
            finally
            {
                if (File.Exists(tempDocx))
                    File.Delete(tempDocx);
            }
        }

        private static void CreateWordReport(List<SecurityIncident> incidents, string docxPath, string pdfPath)
        {
            dynamic word = null;
            dynamic document = null;
            try
            {
                Type type = Type.GetTypeFromProgID("Word.Application");
                if (type == null)
                    throw new InvalidOperationException("Microsoft Word не установлен.");
                word = Activator.CreateInstance(type);
                word.Visible = false;
                document = word.Documents.Add();
                document.Content.Text = "ОТЧЁТ ПО ИНЦИДЕНТАМ ИНФОРМАЦИОННОЙ БЕЗОПАСНОСТИ\r" +
                    "Дата формирования: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\r" +
                    "Сформировал: " + Core.CurrentUser.FIO + "\r";
                document.Content.Font.Name = "Times New Roman";
                document.Content.Font.Size = 12;
                document.Paragraphs[1].Range.Font.Bold = 1;
                document.Paragraphs[1].Range.Font.Size = 16;

                int end = document.Content.End - 1;
                dynamic range = document.Range(end, end);
                dynamic table = document.Tables.Add(range, incidents.Count + 1, 7);
                table.Borders.Enable = true;
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 10;
                string[] headers = { "ID", "Дата", "Инцидент", "Статус", "Серьёзность", "Актив", "Ответственный" };
                for (int i = 0; i < headers.Length; i++)
                    table.Cell(1, i + 1).Range.Text = headers[i];
                table.Rows[1].Range.Font.Bold = 1;

                for (int i = 0; i < incidents.Count; i++)
                {
                    var item = incidents[i];
                    table.Cell(i + 2, 1).Range.Text = item.ID.ToString();
                    table.Cell(i + 2, 2).Range.Text = item.IncidentDate.ToString("dd.MM.yyyy HH:mm");
                    table.Cell(i + 2, 3).Range.Text = item.Title;
                    table.Cell(i + 2, 4).Range.Text = item.Status;
                    table.Cell(i + 2, 5).Range.Text = item.Severity.ToString();
                    table.Cell(i + 2, 6).Range.Text = item.Asset?.Name ?? "—";
                    table.Cell(i + 2, 7).Range.Text = item.AssignedUser?.FIO ?? "Не назначен";
                }
                table.Columns.AutoFit();

                foreach (dynamic section in document.Sections)
                    section.Footers[1].PageNumbers.Add(2);

                document.SaveAs2(docxPath, 16);
                if (!string.IsNullOrEmpty(pdfPath))
                    document.ExportAsFixedFormat(pdfPath, 17);
            }
            finally
            {
                if (document != null)
                {
                    document.Close(false);
                    Marshal.ReleaseComObject(document);
                }
                if (word != null)
                {
                    word.Quit();
                    Marshal.ReleaseComObject(word);
                }
            }
        }

        public static void ExportExcel()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспорт данных в Excel",
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                FileName = "Инциденты_безопасности.xlsx"
            };
            if (dialog.ShowDialog() != true)
                return;

            dynamic excel = null;
            dynamic workbook = null;
            try
            {
                Type type = Type.GetTypeFromProgID("Excel.Application");
                if (type == null)
                    throw new InvalidOperationException("Microsoft Excel не установлен.");
                excel = Activator.CreateInstance(type);
                excel.Visible = false;
                excel.DisplayAlerts = false;
                workbook = excel.Workbooks.Add();
                dynamic sheet = workbook.ActiveSheet;
                sheet.Name = "Инциденты";
                string[] headers = { "ID", "Дата", "Инцидент", "Описание", "Статус", "Серьёзность", "Актив", "Ответственный" };
                for (int i = 0; i < headers.Length; i++)
                    sheet.Cells[1, i + 1] = headers[i];
                sheet.Rows[1].Font.Bold = true;

                var incidents = GetIncidents();
                for (int i = 0; i < incidents.Count; i++)
                {
                    var item = incidents[i];
                    int row = i + 2;
                    sheet.Cells[row, 1] = item.ID;
                    sheet.Cells[row, 2] = item.IncidentDate.ToString("dd.MM.yyyy HH:mm");
                    sheet.Cells[row, 3] = item.Title;
                    sheet.Cells[row, 4] = item.Description;
                    sheet.Cells[row, 5] = item.Status;
                    sheet.Cells[row, 6] = item.Severity;
                    sheet.Cells[row, 7] = item.Asset?.Name ?? "—";
                    sheet.Cells[row, 8] = item.AssignedUser?.FIO ?? "Не назначен";
                }
                sheet.Columns.AutoFit();
                workbook.SaveAs(dialog.FileName, 51);
                AuditService.Log("Экспорт Excel", dialog.FileName);
                Core.ShowInfo("Книга Excel успешно создана.");
            }
            catch (Exception ex)
            {
                ShowExportError(ex);
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excel != null)
                {
                    excel.Quit();
                    Marshal.ReleaseComObject(excel);
                }
            }
        }

        private static void ShowExportError(Exception ex)
        {
            Core.ShowError("Экспорт не выполнен. Для выгрузки требуется установленный Microsoft Office.\n" + ex.Message);
        }
    }
}
