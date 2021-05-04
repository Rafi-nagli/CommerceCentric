using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IAppConfigWrapper
    {
        string GetFilename(string name, string defValue = null);
        string[] GetStringList(string name);
        T GetValue<T>(string name, T defValue);
        T GetValue<T>(string name);
    }
}
