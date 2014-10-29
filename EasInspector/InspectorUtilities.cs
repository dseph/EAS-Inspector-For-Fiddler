namespace EasInspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Fiddler;
    using System.Xml.Xsl;
    using System.Xml;
    using System.IO;

    /// <summary>
    /// Static class for utility methods
    /// </summary>
    public static class InspectorUtilities
    {
        public static int SessionId;

        #region Fiddler UI Helpers
        public static void SetFiddlerStatus(string status)
        {
            FiddlerApplication.UI.SetStatusText(status);
        }

        public static void LogDebug(string logString)
        {
#if (DEBUG)
            InspectorUtilities.Log(logString);
#endif
        }

        public static void Log(string logString)
        {
            Fiddler.FiddlerApplication.Log.LogString(String.Format("[EASInspector][{0}] {1}", InspectorUtilities.SessionId, logString));
        }

        public static string FriendlyException(string description, string exception)
        {
            return string.Format(@"{0}<br/><br/><div class=""level1""><code>{1}</code></div>", description, InspectorUtilities.ExceptionCleanup(exception));
        }

        public static string ExceptionCleanup(string exceptionIn)
        {
            return exceptionIn.Replace(@"   at ", @"<br/>&nbsp;&nbsp;&nbsp;at ");
        }
        #endregion

        public static string TransformXml(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter xml = XmlWriter.Create(sb);

            XslCompiledTransform xsl = new XslCompiledTransform();

            // Load the XSL from the embedded resources
            xsl.Load(XmlReader.Create(InspectorUtilities.GetEmbeddedResourceAsStream("EasInspector", "Embedded.XmlTransform.xslt")));

            // Write the XSLT transformed document
            xsl.Transform(doc, null, xml);

            return sb.ToString();
        }

        /// <summary>
        /// Quoted-Printable decoding
        /// RFC2045 - 6.6 Canonical Encoding Model
        /// http://tools.ietf.org/html/rfc2045
        ///
        /// This is often how iOS sends ICS attachments
        /// </summary>
        /// <param name="qpString">Input quoted-printable string</param>
        /// <returns>Decoded string</returns>
        public static string DecodeQP(string qpString)
        {
            // Soft line break / 78-character line-wrap
            qpString = qpString.Replace("=<br/>", "");
            qpString = qpString.Replace("=0D=0A=20", "");

            // Newline to <br/>
            qpString = qpString.Replace("=0D=0A", "<br/>");

            // Tab to spaces
            qpString = qpString.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

            // Space to space
            qpString = qpString.Replace("=20", " ");

            // = to =
            qpString = qpString.Replace("=3D", "=");

            return qpString;
        }

        public static string DecodeEmailData(string dataString)
        {
            dataString = System.Web.HttpUtility.HtmlEncode(dataString);
            dataString = dataString.Replace("\r\n ", "<br/>&nbsp;");
            dataString = dataString.Replace("\r\n", "<br/>");
            dataString = dataString.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            dataString = dataString.Replace("    ", "&nbsp;&nbsp;&nbsp;&nbsp;");

            // For now, only decode ICS items, since I have no example data for other QP data
            if (dataString.ToLower().Contains("content-transfer-encoding: quoted-printable") && dataString.ToLower().Contains(@"text/calendar"))
            {
                dataString = DecodeQP(dataString);
            }

            return dataString;
        }

        /// <summary>
        /// Returns the printable bytes as a string
        /// </summary>
        /// <param name="bytesFromFiddler">byte[] to convert</param>
        /// <returns>string representation of the passed bytes</returns>
        public static string GetByteString(byte[] bytesFromFiddler)
        {
            StringBuilder sb = new StringBuilder();

            // Output the byte pairs
            foreach (byte singleByte in bytesFromFiddler)
            {
                sb.Append(singleByte.ToString("x2").ToUpper());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a stream object from an embedded resource
        /// </summary>
        /// <param name="ns">The namespace of the resource</param>
        /// <param name="res">The resource name including path (e.g. Embedded.ResourceName.res)</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceAsStream(string ns, string res)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", ns, res));
        }

        /// <summary>
        /// Gets an embedded resource as a string
        /// </summary>
        /// <param name="ns">The namespace of the resource</param>
        /// <param name="res">The resource name including path (e.g. Embedded.ResourceName.res)</param>
        /// <returns>String value with the contents of the resource</returns>
        public static string GetEmbeddedResourceAsString(string ns, string res)
        {
            using (var reader = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", ns, res))))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
