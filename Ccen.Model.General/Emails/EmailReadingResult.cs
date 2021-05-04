using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Emails
{
    public class EmailReadingResult
    {
        public EmailDTO Email { get; set; }
        public NameValueCollection Headers { get; set; }
        public string[] MatchedIdList; 
        public string Folder { get; set; }
        public EmailMatchingResultStatus Status;

        public bool HasMatches
        {
            get { return MatchedIdList != null && MatchedIdList.Any(); }
        }

        public bool WasEmailProcessed { get; set; }
    }


    public enum EmailMatchingResultStatus
    {
        New,
        Existing
    }
}
