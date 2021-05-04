using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.Messages
{
    public class ValueResult<T>
    {
        public bool IsSuccess { get; set; }
        
        [AllowHtml]
        public string Message { get; set; }

        [AllowHtml]
        public T Data { get; set; }
        
        public ValueResult() { }

        public ValueResult(bool isSuccess, string message, T data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public static ValueResult<T> Success(string message, T data = default(T))
        {
            return new ValueResult<T>(true, message, data);
        }

        public static ValueResult<T> Success()
        {
            return new ValueResult<T>(true, String.Empty, default(T));
        }

        public static ValueResult<T> Error(string message, T data = default(T))
        {
            return new ValueResult<T>(false, message, data);
        }

        public static ValueResult<T> Error()
        {
            return new ValueResult<T>(false, String.Empty, default(T));
        }
    }
}