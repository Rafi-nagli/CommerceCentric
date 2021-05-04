using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class Row
    {
        public class Cell
        {
            public string Name { get; set; }
            public int Order { get; set; }
            public int Width { get; set; }
            public int CellCount { get; set; }
        }

        public int Number { get; set; }
        public IList<Cell> Cells { get; set; }

            
    }

    public class PreservedHeaders
    {
        public IEnumerable<Row> Rows { get; set; }

        public IList<Row> InitHeaders()
        {
            return new List<Row>
            {
                new Row
                {
                    Number = 1,
                    Cells = new List<Row.Cell>
                    {
                        new Row.Cell { Order = 1, CellCount = 1, Width = 27, Name = "TemplateType=Clothing" },
                        new Row.Cell { Order = 2, CellCount = 1, Width = 65, Name = "Version=2014.0203" },
                        new Row.Cell { Order = 3, CellCount = 7, Width = 0, Name = "Version=2014.0203" },
                    }
                }
            };
        }
    }
}