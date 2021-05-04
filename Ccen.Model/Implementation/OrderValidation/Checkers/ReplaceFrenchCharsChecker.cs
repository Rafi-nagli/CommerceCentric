using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class ReplaceFrenchCharsChecker
    {
        private ILogService _log;
        private ITime _time;

        public ReplaceFrenchCharsChecker(ILogService log,
            ITime time)
        {
            _time = time;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (dbOrder.IsManuallyUpdated)
                dbOrder.ManuallyPersonName = ReplaceFrench(dbOrder.ManuallyPersonName);
            else
                dbOrder.PersonName = ReplaceFrench(dbOrder.PersonName);

            if (dbOrder.IsManuallyUpdated)
                dbOrder.ManuallyShippingAddress1 = ReplaceFrench(dbOrder.ManuallyShippingAddress1);
            else
                dbOrder.ShippingAddress1 = ReplaceFrench(dbOrder.ShippingAddress1);

            if (dbOrder.IsManuallyUpdated)
                dbOrder.ManuallyShippingAddress2 = ReplaceFrench(dbOrder.ManuallyShippingAddress2);
            else
                dbOrder.ShippingAddress2 = ReplaceFrench(dbOrder.ShippingAddress2);

            if (dbOrder.IsManuallyUpdated)
                dbOrder.ManuallyShippingState = ReplaceFrench(dbOrder.ManuallyShippingState);
            else
                dbOrder.ShippingState = ReplaceFrench(dbOrder.ShippingState);

            if (dbOrder.IsManuallyUpdated)
                dbOrder.ManuallyShippingCity = ReplaceFrench(dbOrder.ManuallyShippingCity);
            else
                dbOrder.ShippingCity = ReplaceFrench(dbOrder.ShippingCity);
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order,
            IList<ListingOrderDTO> items,
            AddressValidationStatus addressValidationStatus)
        {
            if (order.Id == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            if (order.OrderStatus == OrderStatusEnumEx.Pending)
                throw new ArgumentException("order.OrderStatus", "Not supported status Pending");

            if (order.IsManuallyUpdated)
                order.ManuallyPersonName = ReplaceFrench(order.ManuallyPersonName);
            else
                order.PersonName = ReplaceFrench(order.PersonName);

            if (order.IsManuallyUpdated)
                order.ManuallyShippingAddress1 = ReplaceFrench(order.ManuallyShippingAddress1);
            else
                order.ShippingAddress1 = ReplaceFrench(order.ShippingAddress1);

            if (order.IsManuallyUpdated)
                order.ManuallyShippingAddress2 = ReplaceFrench(order.ManuallyShippingAddress2);
            else
                order.ShippingAddress2 = ReplaceFrench(order.ShippingAddress2);

            if (order.IsManuallyUpdated)
                order.ManuallyShippingState = ReplaceFrench(order.ManuallyShippingState);
            else
                order.ShippingState = ReplaceFrench(order.ShippingState);

            if (order.IsManuallyUpdated)
                order.ManuallyShippingCity = ReplaceFrench(order.ManuallyShippingCity);
            else
                order.ShippingCity = ReplaceFrench(order.ShippingCity);

            return new CheckResult()
            {
                IsSuccess = false
            };
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string ReplaceFrench(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            var replaces = new Dictionary<string, string>
            {
                { "MontrÃ©al", "Montreal" },
                { "Ã©", "e" },
            };

            foreach (var r in replaces)
                text = text.Replace(r.Key, r.Value);

            return RemoveDiacritics(text);

            //var replaces = new Dictionary<string, string>
            //{
            //    { "MontrÃ©al", "Montreal" },
            //    { "é", "e" },
            //    { "à", "a" },
            //    { "è", "e" },
            //    { "ù", "u" },
            //    { "â", "a" },
            //    { "ê", "e" },
            //    { "î", "i" },
            //    { "ô", "o" },
            //    { "û", "u" },
            //    { "ç", "c" },



            //    { "äæǽ", "ae" },
            //    { "öœ", "oe" },
            //    { "ü", "ue" },
            //    { "Ä", "Ae" },
            //    { "Ü", "Ue" },
            //    { "Ö", "Oe" },
            //    { "ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A" },
            //    { "àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a" },
            //    { "Б", "B" },
            //    { "б", "b" },
            //    { "ÇĆĈĊČ", "C" },
            //    { "çćĉċč", "c" },
            //    { "Д", "D" },
            //    { "д", "d" },
            //    { "ÐĎĐΔ", "Dj" },
            //    { "ðďđδ", "dj" },
            //    { "ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E" },
            //    { "èéêëēĕėęěέεẽẻẹềếễểệеэ", "e" },
            //    { "Ф", "F" },
            //    { "ф", "f" },
            //    { "ĜĞĠĢΓГҐ", "G" },
            //    { "ĝğġģγгґ", "g" },
            //    { "ĤĦ", "H" },
            //    { "ĥħ", "h" },
            //    { "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I" },
            //    { "ìíîïĩīĭǐįıηήίιϊỉịиыї", "i" },
            //    { "Ĵ", "J" },
            //    { "ĵ", "j" },
            //    { "ĶΚК", "K" },
            //    { "ķκк", "k" },
            //    { "ĹĻĽĿŁΛЛ", "L" },
            //    { "ĺļľŀłλл", "l" },
            //    { "М", "M" },
            //    { "м", "m" },
            //    { "ÑŃŅŇΝН", "N" },
            //    { "ñńņňŉνн", "n" },
            //    { "ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O" },
            //    { "òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o" },
            //    { "П", "P" },
            //    { "п", "p" },
            //    { "ŔŖŘΡР", "R" },
            //    { "ŕŗřρр", "r" },
            //    { "ŚŜŞȘŠΣС", "S" },
            //    { "śŝşșšſσςс", "s" },
            //    { "ȚŢŤŦτТ", "T" },
            //    { "țţťŧт", "t" },
            //    { "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУ", "U" },
            //    { "ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u" },
            //    { "ÝŸŶΥΎΫỲỸỶỴЙ", "Y" },
            //    { "ýÿŷỳỹỷỵй", "y" },
            //    { "В", "V" },
            //    { "в", "v" },
            //    { "Ŵ", "W" },
            //    { "ŵ", "w" },
            //    { "ŹŻŽΖЗ", "Z" },
            //    { "źżžζз", "z" },
            //    { "ÆǼ", "AE" },
            //    { "ß", "ss" },
            //    { "Ĳ", "IJ" },
            //    { "ĳ", "ij" },
            //    { "Œ", "OE" },
            //    { "ƒ", "f" },
            //    { "ξ", "ks" },
            //    { "π", "p" },
            //    { "β", "v" },
            //    { "μ", "m" },
            //    { "ψ", "ps" },
            //    { "Ё", "Yo" },
            //    { "ё", "yo" },
            //    { "Є", "Ye" },
            //    { "є", "ye" },
            //    { "Ї", "Yi" },
            //    { "Ж", "Zh" },
            //    { "ж", "zh" },
            //    { "Х", "Kh" },
            //    { "х", "kh" },
            //    { "Ц", "Ts" },
            //    { "ц", "ts" },
            //    { "Ч", "Ch" },
            //    { "ч", "ch" },
            //    { "Ш", "Sh" },
            //    { "ш", "sh" },
            //    { "Щ", "Shch" },
            //    { "щ", "shch" },
            //    { "ЪъЬь", "" },
            //    { "Ю", "Yu" },
            //    { "ю", "yu" },
            //    { "Я", "Ya" },
            //    { "я", "ya" },
            //};

            //foreach (var r in replaces)
            //    text = text.Replace(r.Key, r.Value);

            //text = text.Replace('é', 'e');
            //text = text.Replace('à', 'a');
            //text = text.Replace('è', 'e');
            //text = text.Replace('ù', 'u');
            //text = text.Replace('â', 'a');
            //text = text.Replace('ê', 'e');
            //text = text.Replace('î', 'i');
            //text = text.Replace('ô', 'o');
            //text = text.Replace('û', 'u');
            //text = text.Replace('ç', 'c');

            //Ã©

            return text;
        }
    }
}
