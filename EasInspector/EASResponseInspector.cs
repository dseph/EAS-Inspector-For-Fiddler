namespace EASView
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Fiddler;

    /// <summary>
    /// Request inspector derived from EASInspector and
    /// implementing Fiddler.IResponseInspector2
    /// </summary>
    public class EASResponseInspector : EASInspector, IResponseInspector2
    {
        /// <summary>
        /// Gets or sets the headers from the frame
        /// </summary>
        public HTTPResponseHeaders headers
        {
            get
            {
                return this.BaseHeaders as HTTPResponseHeaders;
            }

            set
            {
                this.BaseHeaders = value;
            }
        }
    }
}
