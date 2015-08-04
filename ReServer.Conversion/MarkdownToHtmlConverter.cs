using ReServer.Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Conversion
{
    public class MarkdownToHtmlConverter : IConverter
    {
        private string MarkdownText;
        private string HtmlText;

        public RSResponse Convert(RSResponse original)
        {
            MarkdownText = original.StringValue;
            PopulateHtmlFromMarkdown();

            var newResponse = new RSResponse()
            {
                Type = RSResponse.RSResponseType.Converted,
                Text = HtmlText,
                Satisfied = true,
                ContentType = new System.Net.Mime.ContentType("text/html")
            };

            return newResponse;
        }

        private void PopulateHtmlFromMarkdown()
        {
            HtmlText = CommonMark.CommonMarkConverter.Convert(MarkdownText);
        }
    }
}
