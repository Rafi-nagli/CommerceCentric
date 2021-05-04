using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.Web.ViewModels.Inventory;
using UrlHelper = Amazon.Web.Models.UrlHelper;


namespace Amazon.Web.ViewModels
{
    public class StyleHistoryViewModel
    {
        public long? StyleId { get; set; }
        public string StyleString { get; set; }

        
        public IList<StyleChangeViewModel> Changes { get; set; }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public override string ToString()
        {
            var text = ToStringHelper.ToString(this);
            return text;
        }

        public StyleHistoryViewModel()
        {
            Changes = new List<StyleChangeViewModel>();
        }
        
        public static StyleHistoryViewModel GetByStyleString(IUnitOfWork db, 
            ILogService log,
            string styleString)
        {
            StyleEntireDto style = null;
            
            if (!String.IsNullOrEmpty(styleString))
            {
                styleString = StringHelper.TrimWhitespace(styleString);
                style = db.Styles.GetActiveByStyleIdAsDto(styleString);
            }
            
            if (style == null)
            {
                return null;
            }
            else
            {
                var qtyChanges = db.StyleItemQuantityHistories.GetAllAsDto().Where(si => si.StyleId == style.Id)
                    .OrderByDescending(sq => sq.CreateDate)
                    .Take(15)
                    .ToList()
                    .Select(ch => new StyleChangeViewModel(ch))
                    .ToList();
                
                var changes = db.StyleChangeHistories.GetByStyleIdDto(style.Id)
                    .ToList()
                    .OrderByDescending(ch => ch.ChangeDate)
                    .Select(ch => new StyleChangeViewModel(ch))
                    .Where(ch => !String.IsNullOrEmpty(ch.ChangeName)) //NOTE: Skipped empty
                    .ToList();
                
                changes.Add(StyleChangeViewModel.BuildCreateChange(style));
                changes.AddRange(qtyChanges);

                return new StyleHistoryViewModel
                {
                    //Notes =  string.Format("{0} {1}", order.OrderId, itemsNotes),
                    StyleString = style.StyleID,
                    StyleId = style.Id,

                    Changes = changes.OrderByDescending(c => c.ChangeDate).ToList(),
                };
            }
        }
    }
}