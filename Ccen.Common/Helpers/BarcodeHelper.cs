using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Core.Contracts;

namespace Amazon.Common.Helpers
{
    public class BarcodeHelper
    {
        private static Regex _gtinRegex = new Regex("^(\\d{8}|\\d{12,14})$");
        private static Regex _amazonIdRegex = new Regex("^([a-zA-Z0-9]{10})$");

        public static bool IsCustomGenerated(string barcode)
        {
            if (String.IsNullOrEmpty(barcode))
                return false;
            if (barcode.StartsWith("64716"))
                return true;
            return false;
        }

        public static bool IsValidBarcode(string code)
        {
            //1. Amazon Id
            if (_amazonIdRegex.IsMatch(code))
                return true;

            //2. Barcode UPC
            if (!(_gtinRegex.IsMatch(code)))
                return false; // check if all digits and with 8, 12, 13 or 14 digits
            
            //Validate check sum
            code = code.PadLeft(14, '0'); // stuff zeros at start to garantee 14 digits
            var checkSum = GetCheckSum(code);
            return checkSum == int.Parse(code[13].ToString()); // STEP 3 Equivalent to "Subtract the sum from the nearest equal or higher multiple of ten = CHECK DIGIT"
        }

        public static string GenerateBarcode(IBarcodeService barcodeService,
            string sku,
            DateTime when)
        {
            var barcodeInfo = barcodeService.AssociateBarcodes(sku, when, null);
            if (barcodeInfo != null)
                return barcodeInfo.Barcode;
            return String.Empty;
        }

        public static string FixupBarcode(string barcode)
        {
            if (String.IsNullOrEmpty(barcode))
                return barcode;

            if (barcode.Length != 11)
                return barcode;

            var code = barcode.PadLeft(13, '0'); //NOTE: 13 instead of 14, we haven't check digit at now
            var checkSum = GetCheckSum(code);
            return barcode + checkSum.ToString();
        }

        private static int GetCheckSum(string barcode)
        {
            int[] mult = Enumerable.Range(0, 13).Select(i => ((int)(barcode[i] - '0')) * ((i % 2 == 0) ? 3 : 1)).ToArray(); // STEP 1: without check digit, "Multiply value of each position" by 3 or 1
            int sum = mult.Sum(); // STEP 2: "Add results together to create sum"
            return (10 - (sum%10))%10;
        }
    }
}
