using Amazon.Core.Entities;

namespace Amazon.ReportParser.Reader
{
    public interface IFileReader
    {
        string FilePath { get; }

        string Line { get; }
        //ILine LineEntry { get; }
        int LineNumber { get; }

        int FileLength { get; }

        bool EndOfFile { get; }
        bool ReadLine();
        string ReadAll();
        bool Open(string filePath);
        void Close();
    }
}
