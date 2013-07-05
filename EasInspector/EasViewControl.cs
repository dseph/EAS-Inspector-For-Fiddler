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
            txtEasResults.Text = text;
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
