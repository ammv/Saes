using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Saes.AvaloniaMvvmClient.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Helpers
{
    public static class FileDialogHelper
    {
        public static async Task<string> SaveExcel<T>(string title, ICollection<T> entities)
        {
            ExcelExporterConfig excelExporterConfig = App.ServiceProvider.GetService<ExcelExporterConfig>();

            var config = excelExporterConfig.GetConfig<T>();

            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(WindowManager.Windows.Last());

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title ?? "Сохраните файл",
                DefaultExtension="xlsx",
                ShowOverwritePrompt = true,
                SuggestedFileName = $"{config.SuggestedFileNamePrefix}_{DateTime.Now.ToString().Replace(":", "_").Replace(".", "_").Replace(" ", "_")}"
            });

            if (file is not null)
            {

                using (ExcelPackage pck = new ExcelPackage(await file.OpenWriteAsync()))
                {
                    // Запишем заголовки в Excel файл
                    var ws = pck.Workbook.Worksheets.Add(title);

                    int i = 1;
                    foreach (var item in config.Headers)
                    {
                        ws.Cells[1, ++i].Value = item;
                    }

                    i = 2;
                    ws.Cells["A1:Z1"].Style.Font.Bold = true;

                    // Запишем данные в Excel файл
                    
                    foreach (var entity in entities)
                    {
                        var values = config.DataExtractor(entity).ToArray();
                        for (int k = 0; k < values.Length; k++)
                        {
                            ws.Cells[i, k + 1].Value = values[k];
                        }
                        i++;
                    }

                    // Автонастройка ширины столбцов
                    //ws.Cells.AutoFitColumns();

                    await pck.SaveAsync();
                }

                string filePath = file.Path.AbsolutePath;

                return filePath;


            }
            return null;
        }

        public static void Open(string fileName)
        {
            System.Diagnostics.Process.Start(fileName);
        }
    }
}
