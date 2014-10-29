namespace EASView
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Fiddler;

    /// <summary>
    /// Request inspector derived from EASInspector and
    /// implementing Fiddler.IRequestInspector2
    /// </summary>
    public class EASRequestInspector : EASInspector, IRequestInspector2
    {
        /// <summary>
        /// Gets or sets the headers from the frame
        /// </summary>
        public HTTPRequestHeaders headers
        {
            get
            {
                return this.BaseHeaders as HTTPRequestHeaders;
            }

            set
            {
                this.BaseHeaders = value;
            }
        }
    }
}
