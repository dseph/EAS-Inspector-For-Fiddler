using System;
using System.Collections.Generic;

using System.Text;
using VisualSync;

namespace VisualSync
{
    class ASWBXMLCodePage
    {
        private string strNamespace = "";
        private string strXmlns = "";
        private Dictionary<byte, string> tokenLookup = new Dictionary<byte, string>();
        private Dictionary<byte, string> docRefLookup = new Dictionary<byte, string>();
        private Dictionary<string, byte> tagLookup = new Dictionary<string, byte>();

        public string Namespace
        {
            get
            {
                return strNamespace;
            }
            set
            {
                strNamespace = value;
            }
        }

        public string Xmlns
        {
            get
            {
                return strXmlns;
            }
            set
            {
                strXmlns = value;
            }
        }

        public void AddToken(byte token, string tag)
        {
            tokenLookup.Add(token, tag);
            tagLookup.Add(tag, token);
        }

        public void AddDocRef(byte token, string url)
        {
            docRefLookup.Add(token, url);
        }

        public string GetDocRef(byte token)
        {
            if (docRefLookup.ContainsKey(token))
            {
                return docRefLookup[token];
            }

            return null;
        }

        public byte GetToken(string tag)
        {
            if (tagLookup.ContainsKey(tag))
                return tagLookup[tag];

            return 0xFF;
        }

        public string GetTag(byte token)
        {
            if (tokenLookup.ContainsKey(token))
                return tokenLookup[token];

            return null;
        }
    }
}

