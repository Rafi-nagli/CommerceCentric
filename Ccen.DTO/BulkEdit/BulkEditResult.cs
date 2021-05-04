using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DTO.BulkEdit
{
    public class BulkEditResult
    {
        public long Id { get; set; }

        private IList<Exception> _exceptions;
        public IList<Exception> ErrorExceptions => _exceptions = _exceptions ?? new List<Exception>();

        private IList<string> _w;
        public IList<string> WarningMessages => _w = _w ?? new List<string>();
        private IList<string> _s;
        public IList<string> SuccessMessages => _s = _s ?? new List<string>();

        public bool Finished { get; set; }

        private int _successCnt;
        public int SuccessCnt => _successCnt = _successCnt == 0 ? SuccessMessages.Count() : _successCnt;
        private int _warningCnt;
        public int WarningCnt => _warningCnt = _warningCnt == 0 ? WarningMessages.Count() : _warningCnt;

       
        private int _failedCnt;
        public int FailedCnt => _failedCnt = _failedCnt == 0 ? ErrorExceptions.Count() : _failedCnt;
    }
}
