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
        // The session object for reference data
        public Session session;

        // Form control references to hook into the UI
        // These are not public so we have to pull them
        public static Control infoBarRequest;
        public static Control infoBarRequestText;
        public static Control infoBarResponse;
        public static Control infoBarResponseText;
        public static Control warningInfoBar;
        public static Control warningInfoBarText;

        // Members required by Fiddler
        private bool m_bDirty = false;
        private byte[] m_entityBody;
        private bool m_bReadOnly = true;

        // Session information used for decisions on decoding
        public SessionTypeEnum SessionType;

        /// <summary>
        /// Custom control to populate the inspector tab
        /// </summary>
        EasViewControl oEasViewControl = null;

        /// <summary>
        /// Called by Fiddler to determine how confident this inspector is that it can
        /// decode the data.  This is only called when a user hits enter or double-
        /// clicks a session.  It appears it's not called 100% of the time.
        /// If we score the highest out of the other inspectors, Fiddler will open this
        /// inspector's tab and then call AssignSession.
        /// Note that some built-in tabs may still override this (Headers for example will
        /// always override when it sees a 401)
        /// </summary>
        /// <param name="oS">Session object passed in</param>
        /// <returns>int between 0-100 with 100 being the most confident</returns>
        public override int ScoreForSession(Session oS)
        {
            this.session = oS;

            // If either of these are true, we're 100% sure we're responsible
            if (this.isActiveSync)
            {
                return 100;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// This is called every time this inspector is shown
        /// </summary>
        /// <param name="oS">Session object passed by Fiddler</param>
        public override void AssignSession(Session oS)
        {
            // Save off the session, this is used to make decisions about parsing
            this.session = oS;

            base.AssignSession(oS);
        }

        public bool isActiveSync
        {
            get
            {
                return CheckIfActiveSync();
            }
        }

        public bool CheckIfActiveSync()
        {
            if (this.session != null && (this.session.fullUrl.Contains("/Microsoft-Server-ActiveSync") || this.GetContentType() == "application/vnd.ms-sync.wbxml"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called by Fiddler to add the inspector tab
        /// </summary>
        /// <param name="o">The tab control for the inspector</param>
        public override void AddToTab(TabPage o)
        {
            oEasViewControl = new EasViewControl();

            o.Text = "EAS XML";
            o.Controls.Add(oEasViewControl);
            o.Controls[0].Dock = DockStyle.Fill;

            // Let's do this now, so we're not taking cycles every refresh
            warningInfoBar = GetControl(FiddlerApplication.UI, "pnlTopNotice");
            warningInfoBarText = GetControl(warningInfoBar, "btnWarnDetached");
            infoBarRequest = GetControl(FiddlerApplication.UI, "pnlInfoTipRequest");
            infoBarRequestText = GetControl(infoBarRequest, "btnDecodeRequest");
            infoBarResponse = GetControl(FiddlerApplication.UI, "pnlInfoTip");
            infoBarResponseText = GetControl(infoBarResponse, "btnDecodeResponse");
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
                Clear();

                // Re-evaluate every time we set the body, since Fiddler doesn't always score
                // and users can manually select this inspector for the wrong data anyway
                if ( !this.isActiveSync )
                {
                    this.SetEmptyWarning();
                    return;
                }

                bool isDecoded = this.DecodeBody();

                // These are split because the first time we decode the GZIP etc the "value" given here
                // is NOT decoded yet.  But the decoding process updates the session object and we need
                // to pass that in instead, so we need to know which session object to send in -- request or response
                // The session type is already known as the headers are updated before the body
                // Also, let's go ahead and hide the notification bar, since Fiddler's decoding doesn't hide it
                // automatically when we programatically decode the session.
                if ( this.SessionType == SessionTypeEnum.Request )
                {
                    if (isDecoded)
                    {
                        infoBarRequest.Visible = false;
                    }
                    UpdateView(this.session.requestBodyBytes);
                }
                else
                {
                    if (isDecoded)
                    {
                        infoBarResponse.Visible = false;
                    }
                    UpdateView(this.session.responseBodyBytes);
                }
            }
        }

        protected abstract string GetContentType();
        /// <summary>
        /// Fiddler uses separate decode calls for request vs response
        /// so we abstract this here
        /// </summary>
        /// <returns>Bool indicating whether the decode was successful</returns>
        public abstract bool DecodeBody();

        private void UpdateView(byte[] bytes)
        {
            // Try to only decode valid EAS requests/responses
            if (isActiveSync && bytes.Length > 0 && (this.SessionType == SessionTypeEnum.Request == true || this.session.responseCode == 200))
            {
                try
                {
                    ASCommandResponse commandResponse = new ASCommandResponse(bytes);
                    oEasViewControl.SetText(commandResponse.XMLString);
                    commandResponse = null;
                }
                catch (Exception ex)
                {
                    oEasViewControl.SetText("Error in decoding EAS body.\r\n\r\n" + ex.ToString());
                }
            }
            else
            {
                // Will integrate a more robust tips system later
                SetEmptyWarning();

                oEasViewControl.SetText("This does not appear to be EAS traffic, or the content is empty.");
                if (this.session.fullUrl.ToLower().Contains("cmd=ping"))
                {
                    oEasViewControl.AppendLine("\r\nThis may be normal if the device is just resending a Ping with a Content-Length of 0. A cached version would be used:\r\nhttp://msdn.microsoft.com/en-us/library/ee200913.aspx\r\n\r\nFull URL: " + this.session.fullUrl);
                    oEasViewControl.AppendLine("Content-Length: " + bytes.Length);
                }
                else if (this.session.fullUrl.ToLower().Contains("cmd=sync"))
                {
                    oEasViewControl.AppendLine("\r\nThis may be normal if the device is just resending a Sync with a Content-Length of 0. A cached version would be used:");
                    if (this.SessionType == SessionTypeEnum.Request)
                    {
                        oEasViewControl.AppendLine("http://msdn.microsoft.com/en-us/library/ee203280.aspx");
                    }
                    else
                    {
                        oEasViewControl.AppendLine("http://msdn.microsoft.com/en-us/library/ee200474.aspx");
                    }
                    oEasViewControl.AppendLine("\r\nFull URL: " + this.session.fullUrl);
                    oEasViewControl.AppendLine("Content-Length: " + bytes.Length);
                }
            }
        }

        public void SetEmptyWarning()
        {
            Control infoBar;
            Control infoBarText;

            if (this.SessionType == SessionTypeEnum.Request)
            {
                infoBar = infoBarRequest;
                infoBarText = infoBarRequestText;

            }
            else
            {
                infoBar = infoBarResponse;
                infoBarText = infoBarResponseText;
            }
            if (null != infoBar && null != infoBarText)
            {
                infoBar.Visible = true;
                infoBar.Enabled = false;
                infoBarText.Text = "This does not appear to be EAS traffic, or the content is empty.";
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

        /// <summary>
        /// Searches a control for a child control
        /// </summary>
        /// <param name="root">Root control to search</param>
        /// <param name="searchString">Name of the child control to find</param>
        /// <returns>Control with passed name or null</returns>
        public static Control GetControl(Control root, string searchString)
        {
            for (int i = 0; i < root.Controls.Count; i++)
            {
                //System.Diagnostics.Debug.Print("Checking " + searchString + " against '" + root.Controls[i].Name + "' -- Match? " + root.Controls[i].Name.Equals(searchString).ToString() );
                if (root.Controls[i].Name.Equals(searchString))
                {
                    return root.Controls[i];
                }
                if (root.Controls[i].Controls.Count > 0)
                {
                    Control tControl = GetControl(root.Controls[i], searchString);
                    if (null != tControl)
                    {
                        return tControl;
                    }
                }
            }
            return null;
        }
    }

    public enum SessionTypeEnum
    {
        Request,
        Response
    }
}
