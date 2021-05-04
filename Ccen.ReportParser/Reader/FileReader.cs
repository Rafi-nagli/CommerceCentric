using System;
using System.IO;
using System.Text;

namespace Amazon.ReportParser.Reader
{
    public class FileReader : IFileReader
    {
        private StringBuilder readedString;
        private StreamReader reader;

        private string filePath;
        public string FilePath { get { return filePath; } }

        private string line;
        public string Line { get { return line; } }

        private int lineNumber;
        public int LineNumber { get { return lineNumber; } }

        private int fileLength;
        public int FileLength { get { return fileLength; } }

        public bool EndOfFile { get { return reader.EndOfStream; } }

        public bool ReadLine()
        {
            if (reader.EndOfStream)
            {
                return false;
            }
            readedString.Clear();

            while (reader.Peek() >= 0)
            {
                var ch = (char)reader.Read();
                if (ch == '\n' || ch == '\r')
                {
                    break;
                }
                readedString.Append(ch);
            }

            line = readedString.ToString();
            if (string.IsNullOrWhiteSpace(line))
                return false;
            lineNumber++;
            return true;
        }

        public string ReadAll()
        {
            return reader.ReadToEnd();
        }

        public bool Open(string file)
        {
            filePath = file;
            readedString = new StringBuilder(256);
            try
            {
                var fileInfo = new FileInfo(filePath);
                fileLength = (int) fileInfo.Length;

                reader = new StreamReader(filePath, Encoding.Default, true);
            }
            catch (Exception)
            {
                Close();
                throw;
            }
            return true;
        }

        public void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }
    }
}
