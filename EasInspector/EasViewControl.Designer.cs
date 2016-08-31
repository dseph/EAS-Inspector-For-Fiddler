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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip_Label1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_Label2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_SpringLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_ViewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.txtEasResults = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStrip_Label1,
            this.toolStrip_Label2,
            this.toolStrip_SpringLabel,
            this.toolStrip_ViewButton});
            this.statusStrip1.Location = new System.Drawing.Point(0, 245);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(365, 26);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip_Label1
            // 
            this.toolStrip_Label1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStrip_Label1.Name = "toolStrip_Label1";
            this.toolStrip_Label1.Size = new System.Drawing.Size(4, 21);
            // 
            // toolStrip_Label2
            // 
            this.toolStrip_Label2.Name = "toolStrip_Label2";
            this.toolStrip_Label2.Size = new System.Drawing.Size(0, 21);
            // 
            // toolStrip_SpringLabel
            // 
            this.toolStrip_SpringLabel.Name = "toolStrip_SpringLabel";
            this.toolStrip_SpringLabel.Size = new System.Drawing.Size(252, 21);
            this.toolStrip_SpringLabel.Spring = true;
            // 
            // toolStrip_ViewButton
            // 
            this.toolStrip_ViewButton.AutoToolTip = false;
            this.toolStrip_ViewButton.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip_ViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStrip_ViewButton.DropDownButtonWidth = 0;
            this.toolStrip_ViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStrip_ViewButton.Name = "toolStrip_ViewButton";
            this.toolStrip_ViewButton.Size = new System.Drawing.Size(89, 24);
            this.toolStrip_ViewButton.Text = "Smart View";
            this.toolStrip_ViewButton.ToolTipText = "Click to toggle between \"Smart\" parsing and raw decoded bytes";
            this.toolStrip_ViewButton.ButtonClick += new System.EventHandler(this.toolStrip_ViewButton_ButtonClick);
            // 
            // txtEasResults
            // 
            this.txtEasResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEasResults.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEasResults.Location = new System.Drawing.Point(0, 0);
            this.txtEasResults.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtEasResults.Multiline = true;
            this.txtEasResults.Name = "txtEasResults";
            this.txtEasResults.ReadOnly = true;
            this.txtEasResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEasResults.Size = new System.Drawing.Size(365, 245);
            this.txtEasResults.TabIndex = 0;
            this.txtEasResults.Visible = false;
            this.txtEasResults.TextChanged += new System.EventHandler(this.txtEasResults_TextChanged);
            // 
            // EasViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtEasResults);
            this.Controls.Add(this.statusStrip1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "EasViewControl";
            this.Size = new System.Drawing.Size(365, 271);
            this.Load += new System.EventHandler(this.ucEasView_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TextBox txtEasResults;
        private System.Windows.Forms.ToolStripStatusLabel toolStrip_Label1;
        private System.Windows.Forms.ToolStripSplitButton toolStrip_ViewButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStrip_SpringLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStrip_Label2;
    }
}
