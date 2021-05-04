using Amazon.DTO.CustomReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DTO.BulkEdit
{
    public class BulkEditOperationDTO
    {
		public long Id { get; set; }

		public long CustomReportId { get; set; }

		public long CustomReportPredefinedFieldId { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public int OperatorId { private get; set; }

		public int? AllRecordsCnt { get; set; }
		public int? ProcessedRecordsCnt { get; set; }

		public EditOperator Operator
		{
			get
			{
				return (EditOperator)OperatorId;
			}
            set
            {
				OperatorId = (int)value;
			}
		}

		public string ValueToSet { get; set; }

		public DateTime DateStart { get; set; }

		public DateTime? DateEnd { get; set; }
		public DateTime? DateLastUpdated { get; set; }
		public int? SuccessCnt { get; set; }
		public int? WarningCnt { get; set; }
		public int? FailedCnt { get; set; }
		public long CreatedBy { get; set; }
		public string ExtraValue { get; set; }
		public static List<string> BulkEditFieldNames => new List<string>() { "CurrentPrice", "Tag", "Weight", "Description", "Division", "Season", "Year", "Main Item style" };	
	}
}
