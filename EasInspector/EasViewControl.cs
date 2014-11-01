using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

namespace EASView
{
    public partial class EasViewControl : UserControl
    {
        /// <summary>
        /// The browser control used for HTML formatted text
        /// </summary>
        private WebBrowser browser = new WebBrowser();

        /// <summary>
        /// One instance each for request and response
        /// </summary>
        private StringBuilder sb;

        public EasViewControl()
        {
            InitializeComponent();
        }

        private void ucEasView_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;

            // Init the browser control
            this.browser.CreateControl();
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.AllowWebBrowserDrop = false;
            this.browser.DocumentText = string.Empty;
            this.browser.Dock = DockStyle.Fill;

            this.Controls.Add(this.browser);
            this.Controls.SetChildIndex(this.browser, 0);
        }

        /// <summary>
        /// Called by Fiddler to clear the display of the control
        /// </summary>
        public void Clear()
        {
            // Create a new StringBuilder since StringBuilder.Clear() isn't around until .NET 4.0
            ////this.sb.Clear();
            this.sb = new StringBuilder();
            this.txtEasResults.Text = string.Empty;
            this.browser.Document.OpenNew(true);

            this.SetLabel1(string.Empty);
            this.SetLabel2(string.Empty);
        }

        /// <summary>
        /// Flushes the internal StringBuilder to the document window
        /// </summary>
        public void UpdateBrowser()
        {
            this.browser.DocumentText = EasInspector.InspectorUtilities.GetEmbeddedResourceAsString("EasInspector", "Embedded.BrowserTemplate.html").Replace("##ParsedData##", this.sb.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", ""));
        }

        /// <summary>
        /// Appends text to the browser window
        /// </summary>
        /// <param name="txt">string to append</param>
        public void Append(string txt)
        {
            this.sb.Append(txt);
        }

        /// <summary>
        /// Appends text and a new line to the browser window
        /// </summary>
        /// <param name="txt">string to append</param>
        public void AppendLine(string txt)
        {
            this.sb.AppendLine(txt + "<br/>");
        }

        /// <summary>
        /// Appends a new line to the browser window
        /// </summary>
        internal void AppendLine()
        {
            this.sb.AppendLine("<br/>");
        }

        internal void SetRaw(string text)
        {
            // mstehle - 7/19/2013 - Ran into a response were text had "\0\0" in the ConversationIndex element
            // the TextBox and RichTextBox controls will truncate the display
            if (text.Contains("\0\0"))
            {
                EasInspector.InspectorUtilities.LogDebug("Cleaning up double null to make sure all data is displayed in text box.");
                text = text.Replace("\0\0", "    ");
            }

            txtEasResults.Text = text;
        }

        internal void SetLabel1(string txt)
        {
            this.toolStrip_Label1.Text = txt;
        }

        internal void SetLabel2(string txt)
        {
            this.toolStrip_Label2.Text = txt;
        }

        private void ViewRaw()
        {
            this.txtEasResults.Visible = true;
            this.browser.Visible = false;
            this.toolStrip_ViewButton.Text = "Raw View";
        }

        private void ViewSmart()
        {
            this.txtEasResults.Visible = false;
            this.browser.Visible = true;
            this.toolStrip_ViewButton.Text = "Smart View";
        }

        /// <summary>
        /// Toggles the view between "Smart" and "Raw", where the smart view
        /// decodes quoted-printable, transforms the XML, etc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip_ViewButton_ButtonClick(object sender, EventArgs e)
        {
            if (this.txtEasResults.Visible)
            {
                ViewSmart();
            }
            else
            {
                ViewRaw();
            }
        }
    }
}
