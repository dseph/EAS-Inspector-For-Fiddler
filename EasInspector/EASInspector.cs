using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;
using System.Windows.Forms;
using System.Xml;
 
using Fiddler;
using VisualSync;
 

namespace EASView
{
    
  
    public abstract class EASInspector : Inspector2
    {
        private bool m_bDirty = false;
        private byte[] m_entityBody;
        private bool m_bReadOnly = true;

        EasViewControl oEasViewControl = null;
         
         
        public override int ScoreForContentType(string sMIMEType)
        {
            return base.ScoreForContentType(sMIMEType);
        }

        public override void AddToTab(TabPage o)
        {
            oEasViewControl = new EasViewControl();
     
            o.Text = "EAS XML";
            o.Controls.Add(oEasViewControl);
            o.Controls[0].Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 0;
            
        }

        public void Clear()
        {
            oEasViewControl.SetText("");
        }

        public byte[] body
        {
            get
            {
                return m_entityBody;
            }
 
            set
            {

                m_entityBody = value;

                UpdateView(value);
                //m_bDirty = false;
             }
        }

        protected abstract string GetContentType();


        private void UpdateView(byte[] bytes)
        {
            string sContentType = GetContentType();

            if (sContentType == "application/vnd.ms-sync.wbxml")
            {
                try
                {
                    ASCommandResponse commandResponse = new ASCommandResponse(bytes);
                    oEasViewControl.SetText(commandResponse.XMLString);
                    commandResponse = null;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in EAS decoding: \r\n" + ex.ToString());
                }

            }
            else
            {
                oEasViewControl.SetText("");
            }
        }

        public bool bDirty
        {
            get
            {
                return m_bDirty;
            }
 
            set
            {
                m_bDirty = value;
            }
        }



        public bool bReadOnly
        {
            get
            {
                return m_bReadOnly;
            }
            set
            {
                m_bReadOnly = value;
            }

        }

    }
}
