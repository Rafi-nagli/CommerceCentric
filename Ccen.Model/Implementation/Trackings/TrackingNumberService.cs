using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO.TrackingNumbers;
using BarcodeLib;
using Ccen.Core.Contracts;
using Ccen.DTO.Trackings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Trackings
{
    public class TrackingNumberService : ITrackingNumberService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public TrackingNumberService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public bool HasAvailableTrackingNumber()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var freeTrackingNumber = db.CustomTrackingNumbers.GetAll()
                        .OrderBy(t => t.Id)
                        .FirstOrDefault(t => !t.AttachedDate.HasValue);

                return freeTrackingNumber != null;
            }
        }

        public CustomTrackingNumberDTO AttachTrackingNumber(long shippingInfoId,
            string sourceTrackingNumber,
            DateTime when,
            long? by)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var freeTrackingNumber = db.CustomTrackingNumbers.GetAll()
                        .OrderBy(t => t.Id)
                        .FirstOrDefault(t => !t.AttachedDate.HasValue);
                if (freeTrackingNumber != null)
                {
                    freeTrackingNumber.AttachedDate = when;
                    freeTrackingNumber.AttachedToShippingInfoId = shippingInfoId;
                    freeTrackingNumber.AttachedToTrackingNumber = sourceTrackingNumber;
                    db.Commit();

                    return new CustomTrackingNumberDTO()
                    {
                        TrackingNumber = freeTrackingNumber.TrackingNumber
                    };
                }

                throw new Exception("Running out of free custom tracking numbers");
            }
        }

        public bool ApplyTrackingNumber(string filename, string trackingNumber)
        {
            var resultFilePath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + "_result" + Path.GetExtension(filename));
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var groupedbarCode = GetGroupedString(trackingNumber, 4);
                using (var image = WriteBarcodeOnImage(stream, trackingNumber, BarCodeWidth, BarCodeHeight, BarCodeX, BarCodeY, RotateType))
                {
                    WriteImageToLabel(image, groupedbarCode, new Font(new FontFamily(LabelFont), LabelSize, FontStyle.Regular, GraphicsUnit.Pixel), LabelWidth, LabelHeight, LabelX, LabelY, RotateType);
                    image.Save(resultFilePath, ImageFormat.Jpeg);
                }
            }

            File.Delete(filename);
            File.Move(resultFilePath, filename);

            return true;
        }

        public string AppendWithCheckDigit(string trackingNumber)
        {
            int f1 = 0;
            for (var i = 0; i < trackingNumber.Length; i += 2)
            {
                f1 += Int32.Parse("" + trackingNumber[i]);
            }
            int f2 = f1 * 3;
            int f3 = 0;
            for (var i = 1; i < trackingNumber.Length; i += 2)
            {
                f3 += Int32.Parse("" + trackingNumber[i]);
            }
            int f4 = f2 + f3;
            int f5 = (int)Math.Ceiling(f4 / 10.0M) * 10 - f4;
            return trackingNumber + f5.ToString();
        }


        //static string sourceFilePath = "c:\\temp\\source.jpg";
        //static string resultFilePath = "c:\\temp\\result.jpg";
        static RotateFlipType RotateType = RotateFlipType.Rotate270FlipNone;

        //static string BarCode = "9400116901495496317829";
        static int BarCodeWidth = 381;
        static int BarCodeHeight = 80;
        static int BarCodeX = 790;
        static int BarCodeY = -15;

        static string LabelFont = "Arial";
        static int LabelSize = 16;

        static int LabelWidth = 300;
        static int LabelHeight = 15;
        static int LabelX = 970;
        static int LabelY = 10;


        static Image WriteBarcodeOnImage(Stream inputFile, string barCodeString, int width, int height, int x, int y, RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone)
        {
            var baseImage = Image.FromStream(inputFile);
            var overlayImage = DrawBarCode(barCodeString, width, height);
            overlayImage.RotateFlip(rotateFlipType);
            Graphics g = Graphics.FromImage(baseImage);
            g.DrawImage(overlayImage, x, y);
            return baseImage;
        }

        static void WriteImageToLabel(Image baseImage, string text, Font font, int width, int height, int x, int y, RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone)
        {
            Color foreColor = Color.Black;
            Color backColor = Color.Transparent;
            var overlayImage = DrawText(text, font, width, height);
            overlayImage.RotateFlip(rotateFlipType);
            Graphics g = Graphics.FromImage(baseImage);
            g.DrawImage(overlayImage, x, y);
        }

        static Image DrawBarCode(string barCodeString, int width, int height)
        {
            var barCode = new Barcode();
            Color foreColor = Color.Black;
            Color backColor = Color.Transparent;
            var overlayImage = barCode.Encode(TYPE.CODE128, barCodeString, foreColor, backColor, width, height);
            return overlayImage;
        }

        static Image DrawText(String text, Font font, int width, int height)
        {
            Color textColor = Color.Black;
            Color backColor = Color.Transparent;
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();
            img = new Bitmap(width, height);
            drawing = Graphics.FromImage(img);
            drawing.Clear(backColor);
            Brush textBrush = new SolidBrush(textColor);
            drawing.DrawString(text, font, textBrush, 0, 0);
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            return img;
        }

        static string GetGroupedString(string text, int itemsIngroup)
        {
            text = text.Replace(" ", "");
            return String.Join(" ", Split(text, itemsIngroup));
        }

        static IEnumerable<string> Split(string str, int itemsIngroup)
        {
            while (str.Length > itemsIngroup)
            {
                yield return new string(str.Take(itemsIngroup).ToArray());
                str = new string(str.Skip(itemsIngroup).ToArray());
            }
            yield return str;
        }
    }
}
