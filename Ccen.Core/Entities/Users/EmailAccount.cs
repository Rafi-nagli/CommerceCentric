using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Users
{
    public class EmailAccount
    {
        [Key]
        public long Id { get; set; }

        public long CompanyId { get; set; }

        public int Type { get; set; }

        public string ServerHost { get; set; }
        public int ServerPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string DisplayName { get; set; }
        public string FromEmail { get; set; }

        public string AcceptingToAddresses { get; set; }

        public string AttachmentDirectory { get; set; }
        public string AttachmentFolderRelativeUrl { get; set; }

        public bool UseSsl { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
