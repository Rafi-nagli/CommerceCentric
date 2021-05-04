using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DSFileLineMessageDTO
    {
        public long Id { get; set; }
        public long FileLineId { get; set; }
        public string Message { get; set; }
        public int MessageStatus { get; set; }

        public DSFileLineMessageDTO()
        {
            
        }

        public DSFileLineMessageDTO(string message, int status)
        {
            Message = message;
            MessageStatus = status;
        }
    }
}
