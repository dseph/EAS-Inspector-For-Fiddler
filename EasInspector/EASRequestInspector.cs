using System;
using System.Collections.Generic;

using System.Text;
using Fiddler;

namespace EASView
{
    public class EASRequestInspector : EASInspector, IRequestInspector2
    {
        HTTPRequestHeaders saveRequestHeaders;

        public HTTPRequestHeaders headers
        {
            get { return saveRequestHeaders; }
            set
            {
                saveRequestHeaders = value;
                this.SessionType = SessionTypeEnum.Request;
            }
        }

        protected override string GetContentType()
        {
            string result;
            try
            {
                result = saveRequestHeaders["Content-Type"];
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        public override bool DecodeBody()
        {
            // This Fiddler method will permanently update the body and headers
            return this.session.utilDecodeRequest(true);
        }
    }
}
