using System;
using System.Collections.Generic;
using System.IO;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;
using Amazon.ReportParser.Reader;

namespace Amazon.ReportParser.Processing
{
    public abstract class BaseParser : IReportParser
    {
        protected ILogService Log;
        protected ParseContext Context;

        protected IFileReader _reader;

        protected BaseParser()
        {
        }

        public abstract ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId);
        public virtual ILineParser GetXmlParser(ILogService log, MarketType market, string marketplaceId)
        {
            throw new NotSupportedException();
        }

        public virtual void Init(ParseContext context)
        {
            Log = context.Log;
            Context = context;
        }

        public bool Open(string filePath)
        {
            _reader = new FileReader();
            _reader.Open(filePath);
            return true;
        }

        public abstract void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems);

        public virtual List<IReportItemDTO> GetReportItems(MarketType market, string marketplaceId)
        {
            _reader.ReadLine();
            var headers = (_reader.Line ?? "").Split('\t');
            var result = new List<IReportItemDTO>();

            var lineParser = GetLineParser(Log, market, marketplaceId);
            while (!_reader.EndOfFile)
            {
                if (_reader.ReadLine())
                {
                    try
                    {
                        var item = lineParser.Parse((_reader.Line ?? "").Split('\t'), headers);
                        if (item != null)
                            result.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(String.Format("Exception on parsing file: {0},\r\n Line: {1}\r\n",
                            Path.GetFileName(_reader.FilePath),
                            _reader.Line),
                            ex);
                    }
                }
            }

            return result;
        }
        
        public void Close()
        {
            try
            {
                _reader.Close();
            }
            catch (Exception ex)
            {
                Log.Error("Error on close reader", ex);
            }
        }
    }
}
