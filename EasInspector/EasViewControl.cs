using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EASView
{
    public partial class EasViewControl : UserControl
    {
        public EasViewControl()
        {
            InitializeComponent();
        }

        private void ucEasView_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;
        }

        public void SetText(string text)
        {
            // mstehle - 7/19/2013 - Ran into a response were text had "\0\0" in the ConversationIndex element
            // the TextBox and RichTextBox controls will truncate the display
            if (text.Contains("\0\0"))
            {
                System.Diagnostics.Debug.WriteLine("Cleaning up double null to make sure all data is displayed in text box.");
                text = text.Replace("\0\0", "    ");
            }

            txtEasResults.Text = text;
        }

        /// <summary>
        /// Appends text to the results window, separated by a newline (\r\n)
        /// </summary>
        /// <param name="text">String value to append</param>
        public void AppendLine(string text)
        {
            SetText(txtEasResults.Text + "\r\n" + text);
        }

        //// https://tangoxmlview.codeplex.com/SourceControl/latest#FiddlerExtension/FiddlerExtension/RequestDisplayControl.cs
        //public void Display(byte[] Body, string sContentType)
        //{
        //    txtEasResults.Text = "";

        //    if (sContentType == "application/vnd.ms-sync.wbxml")
        //    {
        //        try
        //        {
        //            //ASCommandResponse commandResponse = new ASCommandResponse(Body);
        //            //txtEasResults.Text = commandResponse.XMLString;
        //            commandResponse = null;

        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Error in EAS decoding: \r\n" + ex.ToString());
        //        }


        //    }

        //}


    }
}
