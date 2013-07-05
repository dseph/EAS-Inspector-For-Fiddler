namespace EASView
{
    partial class EasViewControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtEasResults = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtEasResults
            // 
            this.txtEasResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEasResults.Location = new System.Drawing.Point(0, 0);
            this.txtEasResults.Multiline = true;
            this.txtEasResults.Name = "txtEasResults";
            this.txtEasResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEasResults.Size = new System.Drawing.Size(274, 220);
            this.txtEasResults.TabIndex = 0;
            // 
            // EasViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtEasResults);
            this.Name = "EasViewControl";
            this.Size = new System.Drawing.Size(274, 220);
            this.Load += new System.EventHandler(this.ucEasView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox txtEasResults;

    }
}
