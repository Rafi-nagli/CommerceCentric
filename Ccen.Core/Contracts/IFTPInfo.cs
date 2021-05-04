using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IFTPInfo
    {
        string Protocol { get; }
        string FtpSite { get; }
        string FtpFolder { get;}
        string UserName { get; }
        string Password { get; }
        bool IsPassiveMode { get; }
        bool IsSFTP { get; }
    }
}
