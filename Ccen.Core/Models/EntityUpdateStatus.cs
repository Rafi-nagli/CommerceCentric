using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class EntityUpdateStatus<T>
    {
        public T Id { get; set; }
        public UpdateType Status { get; set; }

        public string Tag { get; set; }
        public string TagSecond { get; set; }

        public DateTime? CalcDate { get; set; }

        public EntityUpdateStatus()
        {
            
        } 

        public EntityUpdateStatus(T id, UpdateType status)
        {
            Id = id;
            Status = status;
        }
    }
}
