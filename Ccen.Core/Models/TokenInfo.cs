using System;

namespace Amazon.Core.Models
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
