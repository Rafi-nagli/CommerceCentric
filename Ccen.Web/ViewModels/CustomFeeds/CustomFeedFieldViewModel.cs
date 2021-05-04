using Amazon.DTO.CustomFeeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.CustomFeeds
{
    public class CustomFeedFieldViewModel
    {
        public long Id { get; set; }
        public string SourceFieldName { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldValue { get; set; }
        public int SortOrder { get; set; }

        public CustomFeedFieldViewModel()
        {

        }

        public CustomFeedFieldViewModel(CustomFeedFieldDTO field)
        {
            Id = field.Id;
            SourceFieldName = field.SourceFieldName;
            CustomFieldName = field.CustomFieldName;
            CustomFieldValue = field.CustomFieldValue;
            SortOrder = field.SortOrder;
        }

        public static IList<CustomFeedFieldViewModel> Build(IList<CustomFeedFieldDTO> fields)
        {
            return fields.Select(f => new CustomFeedFieldViewModel(f)).ToList();
        }
    }
}