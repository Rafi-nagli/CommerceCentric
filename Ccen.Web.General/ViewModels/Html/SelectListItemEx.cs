namespace Amazon.Web.ViewModels.Html
{
    public class SelectListItemEx
    {
        public string Text { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// For linking cascade dropdown
        /// </summary>
        public string ParentValue { get; set; }

        public bool Selected { get; set; }

        public SelectListItemEx()
        {
            
        }

        public SelectListItemEx(string text, string value)
        {
            Text = text;
            Value = value;
            ParentValue = value;
        }

        public SelectListItemEx(string text, string value, string parentValue)
        {
            Text = text;
            Value = value;
            ParentValue = parentValue;
        }
    }
}