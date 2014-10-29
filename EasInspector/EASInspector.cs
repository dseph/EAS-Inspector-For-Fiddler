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
using EasInspector;

namespace EASView
{
    public abstract class EASInspector : Inspector2
    {
        // Form control references to hook into the UI
        // These are not public so we have to pull them
        #region UI control references

        /// <summary>
        /// Gets or sets the request panel information bar control
        /// </summary>
        public static Control InfoBarRequest { get; set; }

        /// <summary>
        /// Gets or sets the text label for the request information bar control
        /// </summary>
        public static Control InfoBarRequestText { get; set; }

        /// <summary>
        /// Gets or sets the response panel information bar control
        /// </summary>
        public static Control InfoBarResponse { get; set; }

        /// <summary>
        /// Gets or sets the text label for the response information bar control
        /// </summary>
        public static Control InfoBarResponseText { get; set; }

        /// <summary>
        /// Gets or sets the top Fiddler information bar control
        /// </summary>
        public static Control WarningInfoBar { get; set; }

        /// <summary>
        /// Gets or sets the text label for the Fiddler information bar control
        /// </summary>
        public static Control WarningInfoBarText { get; set; }

        /// <summary>
        /// Gets or sets the custom tab control where we'll put EAS parsed data
        /// </summary>
        public EasViewControl oEasViewControl { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether or not the frame has been changed
        /// </summary>
        public bool bDirty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the frame is read-only
        /// </summary>
        public bool bReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the Session object to pull frame data from Fiddler
        /// </summary>
        internal Session session { get; set; }

        /// <summary>
        /// Gets or sets the raw bytes from the frame
        /// </summary>
        private byte[] rawBody { get; set; }

        /// <summary>
        /// Gets the direction of the traffic
        /// </summary>
        public TrafficDirection Direction
        {
            get
            {
                if (this is IRequestInspector2)
                {
                    return TrafficDirection.In;
                }
                else
                {
                    return TrafficDirection.Out;
                }
            }
        }

        /// <summary>
        /// Gets or sets the base HTTP headers assigned by the request or response
        /// </summary>
        public HTTPHeaders BaseHeaders { get; set; }

        public bool IsActiveSync
        {
            get
            {
                if (this.session != null && this.session.fullUrl.ToLower().Contains("/microsoft-server-activesync"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Called by Fiddler to add the inspector tab
        /// </summary>
        /// <param name="o">The tab control for the inspector</param>
        public override void AddToTab(TabPage o)
        {
            o.Text = "EAS XML";
            o.ToolTipText = "Exchange ActiveSync Inspector";

            this.oEasViewControl = new EasViewControl();
            o.Controls.Add(this.oEasViewControl);
            o.Controls[0].Dock = DockStyle.Fill;

            // Let's do this now, so we're not taking cycles every refresh
            WarningInfoBar = GetControl(FiddlerApplication.UI, "pnlTopNotice");
            WarningInfoBarText = GetControl(WarningInfoBar, "btnWarnDetached");
            InfoBarRequest = GetControl(FiddlerApplication.UI, "pnlInfoTipRequest");
            InfoBarRequestText = GetControl(InfoBarRequest, "btnDecodeRequest");
            InfoBarResponse = GetControl(FiddlerApplication.UI, "pnlInfoTip");
            InfoBarResponseText = GetControl(InfoBarResponse, "btnDecodeResponse");
        }

        /// <summary>
        /// Method that returns a sorting hint
        /// </summary>
        /// <returns>An integer indicating where we should order ourselves</returns>
        public override int GetOrder()
        {
            return 0;
        }

        /// <summary>
        /// Method Fiddler calls to clear the display
        /// </summary>
        public void Clear()
        {
            this.oEasViewControl.Clear();
        }

        // Deal with the session object
        #region Session handling
        /// <summary>
        /// Decodes the body bytes
        /// so we abstract this here
        /// </summary>
        /// <returns>Bool indicating whether the decode was successful</returns>
        public bool DecodeBody()
        {
            if (this.Direction == TrafficDirection.In)
            {
                return this.session.utilDecodeRequest(true);
            }
            else
            {
                return this.session.utilDecodeResponse(true);
            }
        }

        /// <summary>
        /// Called by Fiddler to determine how confident this inspector is that it can
        /// decode the data.  This is only called when the user hits enter or double-
        /// clicks a session.  It does not seem to be called 100% of the time.
        /// _
        /// If we score the highest out of the other inspectors, Fiddler will open this
        /// inspector's tab and then call AssignSession.
        /// ****************************************************************************
        /// Note that some built-in tabs may still override this.  Known overrides:
        /// - "Headers" will always override when it sees a 401
        /// - "Headers" will always override when it sees a 200 and a Content-Length of 0
        /// </summary>
        /// <param name="oS">the session object passed by Fiddler</param>
        /// <returns>int between 0-100 with 100 being the most confident</returns>
        public override int ScoreForSession(Session oS)
        {
            if (null == this.session)
            {
                this.session = oS;
            }

            // BaseHeaders is set in the headers call in the derived classes
            // When this is called from ScoreForSession, Fiddler hasn't called
            // headers yet
            if (null == this.BaseHeaders)
            {
                if (this is IRequestInspector2)
                {
                    this.BaseHeaders = this.session.oRequest.headers;
                }
                else
                {
                    this.BaseHeaders = this.session.oResponse.headers;
                }
            }

            if (this.IsActiveSync)
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

            EasInspector.InspectorUtilities.SessionId = session.id;
            this.oEasViewControl.SetStatus("Session: " + session.id);
        }

        #endregion

        /// <summary>
        /// Gets or sets the body byte[], called by Fiddler with session byte[]
        /// </summary>
        public byte[] body
        {
            get
            {
                return this.rawBody;
            }
            set
            {
                this.rawBody = value;
                this.UpdateView();
            }
        }

        /// <summary>
        /// Update the view with parsed and diagnosed data
        /// </summary>
        private void UpdateView()
        {
            this.Clear();
            this.oEasViewControl.AppendLine("Selected session: " + this.session.id);

            this.DisplayKnownHeaders();

            // Check for HTTP-level issues
            // e.g. 302, 401, empty body, etc
            if (this.Check_PrologIsFatal())
            {
                this.oEasViewControl.UpdateBrowser();
                return;
            }

            // ---------------------------------------------------------------------------------------------------
            // These are split because the first time we decode the GZIP etc the "value" given to us
            // is NOT decoded yet.  But the decoding process updates the session object and we need
            // to pass that in instead, so we need to know which session object to send in -- request or response
            // ---------------------------------------------------------------------------------------------------
            // The session type is already known as the headers are updated before the body
            // ---------------------------------------------------------------------------------------------------
            // Also, let's go ahead and hide the notification bar, since Fiddler's decoding doesn't hide it
            // automatically when we programatically decode the session.
            // ---------------------------------------------------------------------------------------------------
            if (this.Direction == TrafficDirection.In)
            {
                if (this.DecodeBody())
                {
                    InfoBarRequest.Visible = false;
                }

                this.Parse(this.session.requestBodyBytes);
            }
            else
            {
                if (this.DecodeBody())
                {
                    InfoBarResponse.Visible = false;
                }

                this.Parse(this.session.responseBodyBytes);
            }

            this.oEasViewControl.UpdateBrowser();
        }

        /// <summary>
        /// Parse the bytes from Fiddler and append the parsed
        /// string to the WebBrowser document window
        /// </summary>
        /// <param name="bytesFromFiddler">byte[] from Fiddler</param>
        public void Parse(byte[] bytesFromFiddler)
        {
            if (bytesFromFiddler.Length == 0)
            {
                return;
            }

            InspectorUtilities.SetFiddlerStatus("Parsing payload...");

            // Try to only decode valid EAS requests/responses
            try
            {
                ASCommandResponse commandResponse = new ASCommandResponse(bytesFromFiddler);

                if (!string.IsNullOrEmpty(commandResponse.XMLString) && commandResponse.XMLString.Contains(@"<?xml"))
                {
                    this.oEasViewControl.SetRaw(commandResponse.XMLString);

                    XmlDocument doc = commandResponse.XmlDoc;
                    string outputDoc = InspectorUtilities.TransformXml(doc);
                    this.oEasViewControl.AppendLine(outputDoc);
                }

                commandResponse = null;
            }
            catch (Exception ex)
            {
                oEasViewControl.Append(InspectorUtilities.FriendlyException(@"Error in decoding EAS body.", ex.ToString()));
            }

            InspectorUtilities.SetFiddlerStatus(this.session.fullUrl);
        }

        /// <summary>
        /// Check for known headers and display them
        /// </summary>
        public void DisplayKnownHeaders()
        {
            this.DisplayHeader("X-MS-PolicyKey", "Policy Key");
            this.DisplayHeader("User-Agent", "Client Version (User-Agent)");
            this.DisplayHeader("X-ClientInfo", "Client Info");
            this.DisplayHeader("X-RequestId", "Request ID");
            this.DisplayHeader("X-FEServer", "FrontEnd Server");
            this.DisplayHeader("X-ServerApplication", "Server Version");
            this.DisplayHeader("X-ClientApplication", "Client Version");
            this.DisplayHeader("X-ExpirationInfo", "Timeout", "Response header ExpirationInfo");

            this.oEasViewControl.AppendLine();
        }

        /// <summary>
        /// Appends a specified header and value using a specified display name
        /// </summary>
        /// <param name="headerToDisplay">The HTTP header to display</param>
        /// <param name="headerDisplayName">The display name to use in the output</param>
        public void DisplayHeader(string headerToDisplay, string headerDisplayName)
        {
            this.DisplayHeader(headerToDisplay, headerDisplayName, headerToDisplay);
        }

        /// <summary>
        /// Appends a specified header and value using a specified display name and tooltip
        /// </summary>
        /// <param name="headerToDisplay">The HTTP header to display</param>
        /// <param name="headerDisplayName">The display name to use in the output</param>
        /// <param name="toolTip">The tooltip to use for the header, output as a Title attribute</param>
        public void DisplayHeader(string headerToDisplay, string headerDisplayName, string toolTip)
        {
            if (this.BaseHeaders.Exists(headerToDisplay))
            {
                this.oEasViewControl.AppendLine(string.Format(@"<span title=""{2}"" class=""headerItem"">{0}: {1}</span>", headerDisplayName, this.BaseHeaders[headerToDisplay], toolTip));
            }
        }

        /// <summary>
        /// Check the traffic for known HTTP issues and notify the end-user
        /// </summary>
        /// <returns>bool indicating whether any HTTP issues are fatal</returns>
        public bool Check_PrologIsFatal()
        {
            if (this.session.isTunnel)
            {
                this.oEasViewControl.AppendLine(@"This is a Fiddler SSL tunnel to " + this.session.url + ".");
                return true;
            }

            if (!this.IsActiveSync)
            {
                this.oEasViewControl.AppendLine(@"This does not appear to be Exchange ActiveSync traffic.<br/><br/>URL: " + this.session.fullUrl);
                return true;
            }

            if (this.BaseHeaders.ExistsAndEquals("Content-Length", "0") || this.body.Length == 0)
            {
                this.SetEmptyWarning();

                this.oEasViewControl.AppendLine(@"Content-Length is 0.<br/><div class=""level1"">The HTTP payload (body) is empty. &nbsp;This may be normal for Ping, Sync, etc. &nbsp;Refer to the protocol docs (<a title=""[MS-ASCMD]: Exchange ActiveSync: Command Reference Protocol"" target=""_BLANK"" href=""http://msdn.microsoft.com/en-us/library/dd299441.aspx"">MS-ASCMD</a>) to be sure.</div>");

                if (this.session.fullUrl.ToLower().Contains("cmd=ping"))
                {
                    // [MS-ASCMD] 2.2.2.12 Ping
                    oEasViewControl.AppendLine(@"This may be normal if the device is just resending a Ping with a Content-Length of 0.&nbsp; A cached version would be used -- <a title=""[MS-ASCMD] 2.2.2.12 Ping"" href=""http://msdn.microsoft.com/en-us/library/ee200913.aspx"">2.2.2.12 Ping</a>");
                    oEasViewControl.AppendLine("<br/>");
                    oEasViewControl.AppendLine("Full URL: " + this.session.fullUrl);
                }
                else if (this.session.fullUrl.ToLower().Contains("cmd=sync"))
                {
                    oEasViewControl.Append("This may be normal if the device is just resending a Sync with a Content-Length of 0.&nbsp; A cached version would be used -- ");
                    if (this.Direction == TrafficDirection.In)
                    {
                        // [MS-ASCMD] 2.2.2.20.1 Empty Sync Request
                        oEasViewControl.AppendLine(@"<a title=""[MS-ASCMD] 2.2.2.20.1 Empty Sync Request"" href=""http://msdn.microsoft.com/en-us/library/ee203280.aspx"">2.2.2.20.1 Empty Sync Request</a>");
                    }
                    else
                    {
                        // [MS-ASCMD] 2.2.2.20.2 Empty Sync Response
                        oEasViewControl.AppendLine(@"<a title=""[MS-ASCMD] 2.2.2.20.2 Empty Sync Response"" href=""http://msdn.microsoft.com/en-us/library/ee200474.aspx"">2.2.2.20.2 Empty Sync Response</a>");
                    }
                    oEasViewControl.AppendLine();
                    oEasViewControl.AppendLine("Full URL: " + this.session.fullUrl);
                }
                else if (this.session.fullUrl.ToLower().Contains("cmd=sendmail") && this.Direction == TrafficDirection.Out)
                {
                    // [MS-ASCMD] 2.2.2.16 SendMail
                    oEasViewControl.AppendLine(@"A Content-Length of 0 is normal for a SendMail, and indicates a success -- <a href=""http://msdn.microsoft.com/en-us/library/ee178477.aspx"" title=""[MS-ASCMD] 2.2.2.16 SendMail"">2.2.2.16 SendMail</a>");
                }
            }

            string diagServer = "unknown";
            if (this.BaseHeaders.Exists("X-DiagInfo"))
            {
                diagServer = this.BaseHeaders["X-DiagInfo"];
            }
            else if (this.BaseHeaders.Exists("X-FEServer"))
            {
                diagServer = this.BaseHeaders["X-FEServer"];
            }

            switch (this.session.responseCode)
            {
                case 404:
                    if (this.BaseHeaders.ExistsAndContains("X-CasErrorCode", "MailboxGuidWithDomainNotFound"))
                    {
                        this.oEasViewControl.AppendLine(string.Format(@"404 Not Found -- An HTTP 404 Not Found and ""MailboxGuidWithDomainNotFound"" response was received from the remote server ({0}). Ensure the mailbox is licensed for Exchange Online.", diagServer));
                        return true;
                    }
                    else
                    {
                        this.oEasViewControl.AppendLine(string.Format(@"404 Not Found -- An HTTP 404 Not Found response was received from the remote server ({0}).", diagServer));
                        return true;
                    }
                case 456:
                    this.oEasViewControl.AppendLine(string.Format("456 Unauthorized -- An HTTP 456 Unauthorized response was received from the remote server ({0}). This indicates that the user may not have logged on for the first time, or the account may be locked.", diagServer));
                    return true;
                case 401:
                    this.oEasViewControl.AppendLine(@"401 Unauthorized -- check against <a target=""_BLANK"" href=""http://httpweb/http4xx.htm"">HTTP Web</a> for more info. &nbsp;This may simply be part of an auth handshake (<a target=""_BLANK"" href=""http://support.microsoft.com/kb/969060"">KB 969060</a>).");
                    return true;
                case 301:
                case 302:
                    if (this.BaseHeaders.Exists("Location"))
                    {
                        this.oEasViewControl.AppendLine(this.session.responseCode + " Redirect -- The server redirected the client to " + this.session.GetRedirectTargetURL() + ".");
                    }
                    else
                    {
                        this.oEasViewControl.AppendLine(this.session.responseCode + " Redirect -- The server redirected the client but the target location is unknown.");
                    }

                    return true;
                case 200:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Sets the info bars in the Fiddler UI to warn the user about and empty payload
        /// </summary>
        public void SetEmptyWarning()
        {
            Control infoBar;
            Control infoBarText;

            if (this.Direction == TrafficDirection.In)
            {
                infoBar = InfoBarRequest;
                infoBarText = InfoBarRequestText;
            }
            else
            {
                infoBar = InfoBarResponse;
                infoBarText = InfoBarResponseText;
            }

            if (null != infoBar && null != infoBarText)
            {
                infoBar.Visible = true;
                infoBar.Enabled = false;
                infoBarText.Text = "The Content-Length is 0.";
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

    /// <summary>
    /// Enum for traffic direction
    /// </summary>
    public enum TrafficDirection
    {
        /// <summary>
        /// Represents an incoming request
        /// </summary>
        In,

        /// <summary>
        /// Represents the outgoing response
        /// </summary>
        Out
    }
}