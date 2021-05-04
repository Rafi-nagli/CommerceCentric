using Amazon.Common.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Exports.Attributes;
using System.IO;

namespace ImageExportToExcel
{

    public class TestImage
    {
        [ExcelSerializable("Category ID", Order = 0)]
        public string CategoryID { get; set; }

        [ExcelImageSerializable("Awesome Image")]
        public string ImagePath { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var b = new ExportColumnBuilder<TestImage>();
            

            var filename = "Test_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            var items = new List<TestImage>() { new TestImage { CategoryID="test", ImagePath= "https://images.sftcdn.net/images/t_app-cover-l,f_auto/p/befbcde0-9b36-11e6-95b9-00163ed833e7/260663710/the-test-fun-for-friends-screenshot.jpg" } };
            using (var stream = ExcelHelper.Export(items, null))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(filepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
