namespace Amazon.Web.ViewModels.Html
{
    public class SelectListItemTag
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public string Tag { get; set; }

        public bool Selected { get; set; }

        public SelectListItemTag()
        {
            
        }

        public SelectListItemTag(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public SelectListItemTag(string text, string value, string tag)
        {
            Text = text;
            Value = value;
            Tag = tag;
        }
    }
}