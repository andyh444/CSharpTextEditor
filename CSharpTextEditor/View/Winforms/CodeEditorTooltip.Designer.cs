namespace CSharpTextEditor.View.Winforms
{
    partial class CodeEditorTooltip
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.doubleBufferedPanel1 = new CSharpTextEditor.View.Winforms.DoubleBufferedPanel();
            this.SuspendLayout();
            // 
            // doubleBufferedPanel1
            // 
            this.doubleBufferedPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doubleBufferedPanel1.Location = new System.Drawing.Point(0, 0);
            this.doubleBufferedPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.doubleBufferedPanel1.Name = "doubleBufferedPanel1";
            this.doubleBufferedPanel1.Size = new System.Drawing.Size(513, 36);
            this.doubleBufferedPanel1.TabIndex = 0;
            this.doubleBufferedPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.doubleBufferedPanel1_Paint);
            // 
            // CodeEditorTooltip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 36);
            this.Controls.Add(this.doubleBufferedPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CodeEditorTooltip";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "CodeEditorTooltip";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private DoubleBufferedPanel doubleBufferedPanel1;
    }
}