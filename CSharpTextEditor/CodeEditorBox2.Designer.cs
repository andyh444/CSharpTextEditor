namespace CSharpTextEditor
{
    partial class CodeEditorBox2
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
            vScrollBar1 = new VScrollBar();
            panel1 = new DoubleBufferedPanel();
            hScrollBar1 = new HScrollBar();
            SuspendLayout();
            // 
            // vScrollBar1
            // 
            vScrollBar1.Dock = DockStyle.Right;
            vScrollBar1.Location = new Point(533, 0);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new Size(17, 533);
            vScrollBar1.TabIndex = 0;
            vScrollBar1.Scroll += vScrollBar1_Scroll;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Cursor = Cursors.IBeam;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(533, 516);
            panel1.TabIndex = 1;
            panel1.Paint += panel1_Paint;
            panel1.MouseClick += panel1_MouseClick;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Dock = DockStyle.Bottom;
            hScrollBar1.Location = new Point(0, 516);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(533, 17);
            hScrollBar1.TabIndex = 2;
            hScrollBar1.Scroll += hScrollBar1_Scroll;
            // 
            // CodeEditorBox2
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(hScrollBar1);
            Controls.Add(panel1);
            Controls.Add(vScrollBar1);
            Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "CodeEditorBox2";
            Size = new Size(550, 533);
            KeyDown += CodeEditorBox2_KeyDown;
            KeyPress += CodeEditorBox2_KeyPress;
            PreviewKeyDown += CodeEditorBox2_PreviewKeyDown;
            ResumeLayout(false);
        }

        #endregion

        private VScrollBar vScrollBar1;
        private DoubleBufferedPanel panel1;
        private HScrollBar hScrollBar1;
    }
}
