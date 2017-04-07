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
        /// The command for this request, populated in ProcessQueryString()
        /// </summary>
        private string command;

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
            this.oEasViewControl.SetLabel1("Session: " + session.id);
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

            this.DisplayKnownHeaders();

            if (this.IsActiveSync)
            {
                this.ProcessQueryString();
            }

            // Check for HTTP-level issues
            // e.g. 302, 401, empty body, etc
            if (this.Check_PrologIsFatal())
            {
                this.oEasViewControl.UpdateBrowser();
                return;
            }

            this.oEasViewControl.AppendLine();
            this.DisplayTips();

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

        public void DisplayInfo(string title, string data)
        {
            DisplayInfo(title, data, title);
        }

        public void DisplayInfo(string headerDisplayName, string data, string tooltip)
        {
            this.oEasViewControl.AppendLine(string.Format(@"<span title=""{2}"" class=""headerItem"">{0}: {1}</span>", headerDisplayName, data, tooltip));
        }

        /// <summary>
        /// Check for known headers and info and display them
        /// </summary>
        public void DisplayKnownHeaders()
        {
            // We should only see one of these
            this.DisplayHeader("MS-ASProtocolVersion", "ActiveSync Version");
            this.DisplayHeader("MS-Server-ActiveSync", "ActiveSync Version");

            this.DisplayHeader("X-MS-PolicyKey", "Policy Key");
            this.DisplayHeader("User-Agent", "Client Version (User-Agent)");
            this.DisplayHeader("X-ClientInfo", "Client Info");
            this.DisplayHeader("X-RequestId", "Request ID");
            this.DisplayHeader("X-FEServer", "FrontEnd Server");
            this.DisplayHeader("X-ServerApplication", "Server Version");
            this.DisplayHeader("X-ClientApplication", "Client Version");
            this.DisplayHeader("X-ExpirationInfo", "Timeout", "Response header ExpirationInfo");
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
                string headerData = this.BaseHeaders[headerToDisplay];

                if (headerToDisplay == "User-Agent")
                {
                    // Special case
                    this.oEasViewControl.SetLabel2(InspectorUtilities.GetExtendedUserAgentInfo(headerData));
                }
                else
                {
                    // To reduce clutter in the display, only show the header if we don't have a special case
                    // e.g. User-Agent has a label in the status bar
                    if (headerData.IndexOf(',') > -1)
                    {
                        var values = headerData.Split(',');
                        var valueList = new StringBuilder();

                        foreach (var value in values)
                        {
                            valueList.AppendLine($@"<div class=""level1"">{value}</div>");
                        }

                        this.oEasViewControl.AppendLine($@"<span title=""{toolTip}"" class=""headerItem"">{headerDisplayName}:</span><br/>{valueList.ToString()}");
                    }
                    else
                    {
                        // Just display the header
                        this.oEasViewControl.AppendLine(string.Format(@"<span title=""{2}"" class=""headerItem"">{0}: {1}</span>", headerDisplayName, headerData, toolTip));
                    }
                }
            }
        }

        /// <summary>
        /// Check the traffic for known fatal (can't decode EAS traffic) issues and notify the end-user
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

            // This should also cover the 505 Http Version Not Supported
            if (this.session.RequestMethod == "GET")
            {
                this.oEasViewControl.AppendLine("Exchange ActiveSync only supports HTTP methods OPTIONS and POST");
                return true;
            }

            // Requests can be fatal, but we should catch those above
            if (this.Direction == TrafficDirection.In)
            {
                return false;
            }
            else
            {
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
                            this.oEasViewControl.AppendLine(string.Format(@"<br/>404 Not Found -- An HTTP 404 Not Found and ""MailboxGuidWithDomainNotFound"" response was received from the remote server ({0}). Ensure the mailbox is licensed for Exchange Online.", diagServer));
                            return true;
                        }
                        else
                        {
                            this.oEasViewControl.AppendLine(string.Format(@"<br/>404 Not Found -- An HTTP 404 Not Found response was received from the remote server ({0}).", diagServer));
                            return true;
                        }
                    case 456:
                        this.oEasViewControl.AppendLine(string.Format(@"<br/>456 Unauthorized -- An HTTP 456 Unauthorized response was received from the remote server ({0}). This indicates that the user may not have logged on for the first time, or the account may be locked.", diagServer));
                        return true;
                    case 401:
                        this.oEasViewControl.AppendLine(@"<br/>401 Unauthorized -- check against <a target=""_BLANK"" href=""https://support.microsoft.com/en-us/help/943891/the-http-status-code-in-iis-7.0,-iis-7.5,-and-iis-8.0"">KB 943891</a> for more info. &nbsp;This may simply be part of an auth handshake (<a target=""_BLANK"" href=""http://support.microsoft.com/kb/969060"">KB 969060</a>).");
                        return true;
                    case 301:
                    case 302:
                        if (this.BaseHeaders.Exists("Location"))
                        {
                            this.oEasViewControl.AppendLine(@"<br/>" + this.session.responseCode + " Redirect -- The server redirected the client to " + this.session.GetRedirectTargetURL() + ".");
                        }
                        else
                        {
                            this.oEasViewControl.AppendLine(@"<br/>" + this.session.responseCode + " Redirect -- The server redirected the client but the target location is unknown.");
                        }

                        return true;
                    case 500:
                        // In this case, display the HTTP response raw, since if it's a 500 we want to see the error
                        this.oEasViewControl.Clear();
                        this.oEasViewControl.Append(this.session.GetResponseBodyAsString());
                        return true;
                    case 503:
                        if (this.body.Length > 0)
                        {
                            // In this case, display the HTTP response raw, not sure if there's a 503 with a body in EAS
                            this.oEasViewControl.Clear();
                            this.oEasViewControl.Append(this.session.GetResponseBodyAsString());
                        }
                        else
                        {
                            // Try to find errors
                            DisplayHeader("X-CasErrorCode", "ErrorCode");

                            this.oEasViewControl.AppendLine();
                            this.oEasViewControl.Append("503 Service Unavailable. Something went wrong while processing the request.");

                            if (this.BaseHeaders.Exists("X-FailureContext"))
                            {
                                this.oEasViewControl.AppendLine(" More information may be available in the X-FailureContext header below.");
                                this.oEasViewControl.AppendLine();
                                this.oEasViewControl.AppendLine(@"<span title=""X-FailureContext"" class=""headerItem"">X-FailureContext:</span>");
                                var errors = this.BaseHeaders["X-FailureContext"].Split(';');
                                foreach (var value in errors)
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        continue;
                                    }

                                    try
                                    {
                                        var decodedValue = Convert.FromBase64String(value);
                                        foreach (char c in decodedValue)
                                        {
                                            if (Char.IsControl(c))
                                            {
                                                throw new Exception();
                                            }
                                        }
                                        this.oEasViewControl.Append($@"<div class=""level1"">{Encoding.ASCII.GetString(decodedValue)}</div>");
                                    }
                                    catch
                                    {
                                        // This will catch both a Base64 failure as well as a control character failure
                                        // Had to do this because it turns out "FrontEnd" is Base64-decodable as well as
                                        // other possible combinations
                                        this.oEasViewControl.Append($@"<div class=""level1"">{value}</div>");
                                    }
                                }
                            }
                        }
                        return true;
                    case 200:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public void DisplayTips()
        {
            // REQUEST known cases
            if (this.Direction == TrafficDirection.In)
            {
                if (this.session.RequestMethod == "OPTIONS")
                {
                    this.oEasViewControl.AppendLine(@"This is an <a target=""_BLANK"" title=""[MS-ASHTTP] 2.2.3 HTTP OPTIONS Request"" href=""https://msdn.microsoft.com/en-us/library/gg651018.aspx"">HTTP OPTIONS Request</a>, and is used to discover what protocol versions are supported, and which protocol commands are supported on the server. The client uses the HTTP OPTIONS command to determine whether the server supports the same versions of the protocol that the client supports.<br/><br/>There is no HTTP body for this request, so the content-length of 0 is normal.");
                    return;
                }

                // Request body is empty
                if (this.BaseHeaders.ExistsAndEquals("Content-Length", "0") || this.body.Length == 0)
                {
                    switch (command.ToLower())
                    {
                        case "ping":
                            // [MS-ASCMD] 2.2.2.12 Ping
                            this.oEasViewControl.AppendLine($@"This is an empty <a title=""[MS-ASCMD] 2.2.2.12 Ping"" href=""http://msdn.microsoft.com/en-us/library/ee200913.aspx"">Ping</a> request. A Ping command can be sent with no body, in which case the cached version is used.");
                            break;
                        case "sync":
                            // [MS-ASCMD] 2.2.2.20.1 Empty Sync Request
                            oEasViewControl.AppendLine($@"This is an <a target=""_BLANK"" title=""[MS-ASCMD] 2.2.2.20.1 Empty Sync Request"" href=""http://msdn.microsoft.com/en-us/library/ee203280.aspx"">Empty Sync Request</a>. A Sync command can be sent with no body, in which case the cached version is used.");
                            break;
                        default:
                            this.SetEmptyWarning();
                            break;
                    }
                }
            }
            // RESPONSE known cases
            else
            {
                if (this.session.RequestMethod == "OPTIONS")
                {
                    this.oEasViewControl.AppendLine(@"This is an <a target=""_BLANK"" title=""[MS-ASHTTP] 2.2.4 HTTP OPTIONS Response"" href=""https://msdn.microsoft.com/en-us/library/gg672039.aspx"">HTTP OPTIONS Response</a>. After receiving an HTTP OPTIONS request, a server responds with an HTTP OPTIONS response that specifies the protocol versions it supports.  There is no HTTP body for this response, so the content-length of 0 is normal.");
                    this.oEasViewControl.AppendLine();
                    DisplayHeader("MS-ASProtocolVersions", "Supported Versions");
                    DisplayHeader("MS-ASProtocolCommands", "Supported Commands");
                    return;
                }

                // Response body is empty
                if (this.BaseHeaders.ExistsAndEquals("Content-Length", "0") || this.body.Length == 0)
                {
                    switch (command.ToLower())
                    {
                        case "sync":
                            // [MS-ASCMD] 2.2.2.20.2 Empty Sync Response
                            oEasViewControl.AppendLine(@"This is an <a target=""_BLANK"" title=""[MS-ASCMD] 2.2.2.20.2 Empty Sync Response"" href=""http://msdn.microsoft.com/en-us/library/ee200474.aspx"">Empty Sync Response</a>. This generally indicates no changes detected on the server.");
                            break;
                        case "sendmail":
                            // [MS-ASCMD] 2.2.2.16 SendMail
                            oEasViewControl.AppendLine(@"A Content-Length of 0 is normal for a <a target=""_BLANK"" href=""http://msdn.microsoft.com/en-us/library/ee178477.aspx"" title=""[MS-ASCMD] 2.2.2.16 SendMail"">SendMail</a>, and indicates the message was sent successfully.");
                            break;
                        default:
                            this.SetEmptyWarning();
                            break;
                    }
                }
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

            this.oEasViewControl.AppendLine(@"Content-Length is 0.<br/><div class=""level1"">The HTTP payload (body) is empty. This can be normal for Ping, Sync, and other commands. Refer to the protocol docs (<a title=""[MS-ASCMD]: Exchange ActiveSync: Command Reference Protocol"" target=""_BLANK"" href=""http://msdn.microsoft.com/en-us/library/dd299441.aspx"">MS-ASCMD</a>) to be sure.</div>");
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

        // Processes the query string, converting from Base64 if needed
        // Also displays items as headers where needed
        // Base64: "/Microsoft-Server-ActiveSync?jREJBBDIbLhmwWR9sOSgpRFjJNNCBAAAAAALV2luZG93c01haWw=";
        //      [MS-ASHTTP] 2.2.1.1.1.1 Base64-Encoded Query Value
        //      http://msdn.microsoft.com/en-us/library/ee160227.aspx
        // Normal: "/Microsoft-Server-ActiveSync?User=user1@contoso.com&DeviceId=DEADBEEF&DeviceType=iPad&Cmd=Sync";
        // Options: "/Microsoft-Server-ActiveSync?User=user1&DeviceId=DEADBEEF&DeviceType=WindowsMail";
        private void ProcessQueryString()
        {
            var query = string.Empty;
            var queryIndex = session.PathAndQuery.ToLower().IndexOf('?');

            // Do we even have a querystring?
            if (queryIndex > -1 && queryIndex + 1 < session.PathAndQuery.Length)
            {
                query = session.PathAndQuery.Substring(queryIndex + 1);

                try
                {
                    var base64bytes = Convert.FromBase64String(query);

                    var convertedQueryString = new StringBuilder();

                    // /Microsoft-Server-ActiveSync/default.eas?Cmd=Provision&DeviceId=DEADBEEF&DeviceType=WindowsMail
                    convertedQueryString.Append("/Microsoft-Server-ActiveSync/default.eas?");

                    MemoryStream ms = new MemoryStream(base64bytes);
                    BinaryReader br = new BinaryReader(ms);

                    float protocolVersion = br.ReadByte() / 10f;
                    CommandCode commandCode = (CommandCode)br.ReadByte();
                    this.command = commandCode.ToString();

                    if (this.Direction == TrafficDirection.Out)
                    {
                        // We just wanted the command value
                        return;
                    }

                    DisplayInfo("ActiveSync Version", string.Format("{0:N1}", protocolVersion), "MS-ASProtocolVersion");
                    convertedQueryString.Append($"cmd={this.command}");

                    int locale = br.ReadUInt16();

                    int deviceIdLength = br.ReadByte();
                    byte[] deviceIdBytes = br.ReadBytes(deviceIdLength);

                    // According to https://msdn.microsoft.com/en-us/library/ee160227.aspx this value is either
                    // a GUID or a string, but we've noticed that the byte values is often the device ID, not
                    // converted to a GUID
                    if (deviceIdLength == 16)
                    {
                        Guid deviceId = new Guid(deviceIdBytes);
                        DisplayInfo("DeviceId (as GUID)", deviceId.ToString());

                        // Also display as bytes
                        var byteString = new StringBuilder();
                        foreach (byte b in deviceIdBytes)
                        {
                            byteString.Append(b.ToString("X2"));
                        }
                        convertedQueryString.Append($"&DeviceId={byteString.ToString()}");
                        DisplayInfo("DeviceId (as Bytes)", byteString.ToString());
                    }
                    else
                    {
                        convertedQueryString.Append($"&DeviceId={Encoding.ASCII.GetString(deviceIdBytes)}");
                        DisplayInfo("DeviceId", Encoding.ASCII.GetString(deviceIdBytes));
                    }

                    // Valid values are only 0 and 4
                    int policyKeyLength = br.ReadByte();
                    if (policyKeyLength == 4)
                    {
                        UInt32 policyKey = br.ReadUInt32();
                        DisplayInfo("Policy Key", policyKey.ToString(), "X-MS-PolicyKey");
                    }

                    string deviceType = br.ReadString();
                    this.oEasViewControl.SetLabel2(deviceType);
                    convertedQueryString.Append($"&DeviceType={deviceType}");

                    this.oEasViewControl.AppendLine();
                    this.oEasViewControl.AppendLine($@"Query value is <a target=""_BLANK"" title=""[MS-ASHTTP] 2.2.1.1.1.1 Base64-Encoded Query Value"" href=""http://msdn.microsoft.com/en-us/library/ee160227.aspx"">Base64 encoded</a>.  Decoded query:<br/><div class=""level1"">{convertedQueryString.ToString()}</div>");

                    // At this point, the command parameters still need to be parsed
                    // More example Base64 query strings needed
                }
                catch
                {
                    // Not Base64
                    var commandIndex = session.PathAndQuery.ToLower().IndexOf("cmd=");
                    if (commandIndex > -1)
                    {
                        var commandEnd = session.PathAndQuery.ToLower().IndexOf('&', commandIndex);
                        if (commandEnd > -1)
                        {
                            this.command = session.PathAndQuery.Substring(commandIndex + 4, commandEnd);
                        }
                        else
                        {
                            this.command = session.PathAndQuery.Substring(commandIndex + 4);
                        }
                    }

                    if (this.Direction == TrafficDirection.Out)
                    {
                        // We just wanted the command value
                        return;
                    }

                    // outlook.office365.com/Microsoft-Server-ActiveSync?User=test@contoso.com&DeviceId=UR10CK7H897UB3DFM2A4008HFG&DeviceType=iPad&Cmd=Sync
                    int deviceIdStart = session.PathAndQuery.ToLower().IndexOf("DeviceId=");

                    if (deviceIdStart > -1)
                    {
                        int deviceIdEnd = session.PathAndQuery.IndexOf("&", deviceIdStart);
                        if (deviceIdEnd > -1)
                        {
                            string deviceId = session.PathAndQuery.Substring(deviceIdStart + 9, deviceIdEnd - (deviceIdStart + 9));
                            DisplayInfo("DeviceId", deviceId);
                        }
                    }
                }
            }
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

    // [MS-ASHTTP] 2.2.1.1.1.1.2 Command Codes
    // https://msdn.microsoft.com/en-us/library/ee157488.aspx
    public enum CommandCode : byte
    {
        Sync = 0x00,
        SendMail = 0x01,
        SmartForward = 0x02,
        SmartReply = 0x03,
        GetAttachment = 0x04,
        FolderSync = 0x09,
        FolderCreate = 0x0A,
        FolderDelete = 0x0B,
        FolderUpdate = 0x0C,
        MoveItems = 0x0D,
        GetItemEstimate = 0x0E,
        MeetingResponse = 0x0F,
        Search = 0x10,
        Settings = 0x11,
        Ping = 0x12,
        ItemOperations = 0x13,
        Provision = 0x14,
        ResolveRecipients = 0x15,
        ValidateCert = 0x16,
        Find = 0x17
    }
}