using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class Error
    {
        public ErrorType Type;
        public string Message;

        public Error(string message, ErrorType? type = null)
        {
            Message = message;
            Type = type ?? ErrorType.Error;
        }
    }
    public enum ErrorType
    {
        Error,
        Warning
    }
}