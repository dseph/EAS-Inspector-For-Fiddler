using System;
using System.Collections.Generic;

using System.Text;
using Fiddler;

namespace EASView
{
    public class EASResponseInspector : EASInspector, IResponseInspector2
    {
        HTTPResponseHeaders savedResponseHeaders;

        public HTTPResponseHeaders headers
        {
            get { return savedResponseHeaders; }
            set { savedResponseHeaders = value; }
        }

        protected override string GetContentType()
        {
            string result;
            try
            {
                result = savedResponseHeaders["Content-Type"];
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }
    }
}
