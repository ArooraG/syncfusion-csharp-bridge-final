using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf.Imaging; // Yeh line ab .csproj file ki wajah se kaam karegi
using Syncfusion.Pdf.Parsing;
using Syncfusion.XlsIO;
using System.IO;

namespace SyncfusionBridgeAPI.Controllers // Namespace aapke project ke naam se match karega
{
    [ApiController]
    [Route("api/[controller]")] // Route ko behtar kar diya hai
    public class ConvertController : ControllerBase
    {
        [HttpPost("ToWord")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ConvertToWord(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not provided.");
            }

            try
            {
                using var pdfStream = file.OpenReadStream();
                using var loadedDocument = new PdfLoadedDocument(pdfStream);
                using var document = new WordDocument();

                for (int i = 0; i < loadedDocument.Pages.Count; i++)
                {
                    using var imageStream = loadedDocument.Pages[i].ConvertToImage(PdfImageType.Bitmap);
                    IWSection section = document.AddSection();
                    IWPicture picture = section.AddParagraph().AppendPicture(imageStream);

                    section.PageSetup.PageSize = new Syncfusion.Drawing.SizeF(loadedDocument.Pages[i].Size.Width, loadedDocument.Pages[i].Size.Height);
                    section.PageSetup.Margins.All = 0;
                    picture.Width = section.PageSetup.ClientWidth;
                    picture.Height = section.PageSetup.ClientHeight;
                }

                using var wordStream = new MemoryStream();
                document.Save(wordStream, FormatType.Docx);
                wordStream.Position = 0;
                return File(wordStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ConvertedDocument.docx");
            }
            catch (Exception ex)
            {
                // Error logging (optional but recommended)
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("ToExcel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ConvertToExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not provided.");
            }

            try
            {
                using var pdfStream = file.OpenReadStream();
                using var loadedDocument = new PdfLoadedDocument(pdfStream);

                PdfTableExtractor extractor = new PdfTableExtractor(loadedDocument);
                PdfTable[] pdfTables = extractor.ExtractTable();

                using var excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = application.Workbooks.Create(pdfTables.Length > 0 ? pdfTables.Length : 1);

                if (pdfTables.Length > 0)
                {
                    for (int i = 0; i < pdfTables.Length; i++)
                    {
                        IWorksheet worksheet = workbook.Worksheets[i];
                        worksheet.Name = $"Table_{i + 1}";
                        for (int row = 0; row < pdfTables[i].RowCount; row++)
                        {
                            for (int col = 0; col < pdfTables[i].ColumnCount; col++)
                            {
                                worksheet.Range[row + 1, col + 1].Text = pdfTables[i][row, col].GetText();
                            }
                        }
                    }
                }
                else
                {
                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Range["A1"].Text = "No tables found in the PDF document.";
                }

                using var excelStream = new MemoryStream();
                workbook.SaveAs(excelStream);
                excelStream.Position = 0;
                return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ConvertedDocument.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}