using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Models
{
    public class RetriesResult<T>
    {
        public T Result { get; set; }
        public Exception LastAttemptException { get; set; }
        public bool IsSuccess { get; set; }
    }
}
