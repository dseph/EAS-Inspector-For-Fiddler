using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Text;
using VisualSync;

namespace VisualSync
{
    class ASCommandResponse
    {
        private byte[] wbxmlBytes = null;
        private string xmlString = null;

        public byte[] WBXMLBytes
        {
            get
            {
                return wbxmlBytes;
            }
        }

        public string XMLString
        {
            get
            {
                return xmlString;
            }
        }

        public ASCommandResponse(HttpWebResponse httpResponse)
        {
            Stream responseStream = httpResponse.GetResponseStream();
            List<byte> bytes = new List<byte>();
            byte[] byteBuffer = new byte[256];
            int count = 0;

            count = responseStream.Read(byteBuffer, 0, 256);
            while (count > 0)
            {
                bytes.AddRange(byteBuffer);

                if (count < 256)
                {
                    int excess = 256 - count;
                    bytes.RemoveRange(bytes.Count - excess, excess);
                }

                count = responseStream.Read(byteBuffer, 0, 256);
            }

            wbxmlBytes = bytes.ToArray();

            xmlString = DecodeWBXML(wbxmlBytes);
        }

        public ASCommandResponse(byte[] wbxml)
        {
            wbxmlBytes = wbxml;
            xmlString = DecodeWBXML(wbxmlBytes);
        }

        private string DecodeWBXML(byte[] wbxml)
        {
            try
            {
                ASWBXML decoder = new ASWBXML();

                if (wbxml.Length > 0)
                {
                    decoder.LoadBytes(wbxml);
                    return decoder.GetXml();
                }
                else
                {
                    return "Empty Body\r\nThe body is empty.";
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message, "Error calling DecodeWBXML");
                //VSError.ReportException(ex);
                return "Error decoding the WBXML\r\nException: " + ex.Message;
            }
        }
    }
}
