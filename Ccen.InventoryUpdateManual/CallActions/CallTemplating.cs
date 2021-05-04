using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Models.Templates;
using Amazon.Templates;


namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallTemplating
    {
        public void BuildRizorTemplate()
        {
            var model = new eBayDescriptionTemplate();
            var content = File.ReadAllText(Path.Combine(AppSettings.TemplateDirectory, TemplateHelper.EBayDescriptionTemplateName));
            var html = TemplateHelper.RunRazorTemplate("eBayTemplate", content, model);
            Console.WriteLine(html);
        }
    }
}
