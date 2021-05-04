using System.Collections.Generic;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts
{
    public interface IFileMaker
    {
        string CreateFileWithLabels(IList<PrintLabelInfo> labels, 
            IList<string> scanFormImages, 
            BatchInfoToPrint batchInfo,
            string outputDirectory, 
            string name = null);
    }
}
