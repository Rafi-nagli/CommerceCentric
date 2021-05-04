using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Amazon.Common.Helpers
{
    public class PdfHelper
    {
        public static void CombineDhlLabelIntoOnePage(byte[] data, string path)
        {
            using (var file = File.Create(path))
            {
                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document(PageSize.LETTER))
                {
                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, file))
                    {
                        //Open the document for writing
                        doc.Open();

                        PdfHelper.AddDhlPdf(doc, writer, data);

                        doc.Close();
                    }
                }
            }
        }

        public static void AddDhlPdf(Document doc, PdfWriter writer, byte[] pdfData)
        {
            var pdf = new PdfReader(pdfData); //using cause Exception
            {
                doc.NewPage();

                if (pdf.NumberOfPages == 2)
                {
                    float dy = 0;
                    for (int page = 2; page > 0; page--)
                    {
                        var pg = writer.GetImportedPage(pdf, page);

                        if (page == 1)
                            dy = pdf.GetPageSizeWithRotation(page).Width / 2 - 80;

                        //http://stackoverflow.com/questions/3579058/rotating-pdf-in-c-sharp-using-itextsharp
                        writer.DirectContent.AddTemplate(pg, 0, 1f, -1f, 0, pdf.GetPageSizeWithRotation(page).Height, dy); //270
                    }
                }
            }
        }

        public static byte[] BuildPdfFromHtml(string html, string css, int count)
        {
            //Create a byte array that will eventually hold our final PDF
            Byte[] bytes;

            //Boilerplate iTextSharp setup here
            //Create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {

                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document())
                {

                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {

                        //Open the document for writing
                        doc.Open();

                        for (int i = 0; i < count; i++)
                        {
                            doc.NewPage();

                            using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(css)))
                            {
                                using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                                {

                                    //Parse the HTML
                                    iTextSharp.tool.xml.XMLWorkerHelper.GetInstance()
                                        .ParseXHtml(writer, doc, msHtml, msCss);
                                }
                            }
                        }

                        doc.Close();
                    }
                }

                //After all of the PDF "stuff" above is done and closed but **before** we
                //close the MemoryStream, grab all of the active bytes from the stream
                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}
