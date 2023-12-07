using System.Drawing;
using System.Windows.Forms;
using CSharpTextEditor.Winforms;

namespace CSharpTextEditor
{
    partial class CodeEditorBox
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
            components = new System.ComponentModel.Container();
            vScrollBar1 = new VScrollBar();
            panel1 = new DoubleBufferedPanel();
            hScrollBar1 = new HScrollBar();
            hoverToolTip = new ToolTip(components);
            panel2 = new Panel();
            lineLabel = new Label();
            methodToolTip = new ToolTip(components);
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // vScrollBar1
            // 
            vScrollBar1.Dock = DockStyle.Right;
            vScrollBar1.Location = new Point(533, 0);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new Size(17, 570);
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
            panel1.Size = new Size(533, 570);
            panel1.TabIndex = 1;
            panel1.Paint += panel1_Paint;
            panel1.MouseClick += panel1_MouseClick;
            panel1.MouseDoubleClick += panel1_MouseDoubleClick;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            hScrollBar1.Location = new Point(0, 0);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(449, 17);
            hScrollBar1.TabIndex = 2;
            hScrollBar1.Scroll += hScrollBar1_Scroll;
            // 
            // hoverToolTip
            // 
            hoverToolTip.OwnerDraw = true;
            hoverToolTip.ShowAlways = true;
            hoverToolTip.UseAnimation = false;
            hoverToolTip.UseFading = false;
            hoverToolTip.Draw += hoverToolTip_Draw;
            // 
            // panel2
            // 
            panel2.Controls.Add(lineLabel);
            panel2.Controls.Add(hScrollBar1);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 570);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(550, 19);
            panel2.TabIndex = 3;
            // 
            // lineLabel
            // 
            lineLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lineLabel.AutoSize = true;
            lineLabel.Font = new Font("Tahoma", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            lineLabel.Location = new Point(476, 0);
            lineLabel.Name = "lineLabel";
            lineLabel.Size = new Size(71, 16);
            lineLabel.TabIndex = 3;
            lineLabel.Text = "Ln: 0 Ch: 0";
            lineLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // methodToolTip
            // 
            methodToolTip.OwnerDraw = true;
            methodToolTip.ShowAlways = true;
            methodToolTip.UseAnimation = false;
            methodToolTip.UseFading = false;
            methodToolTip.Draw += methodToolTip_Draw;
            // 
            // CodeEditorBox
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(vScrollBar1);
            Controls.Add(panel2);
            Font = new Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "CodeEditorBox";
            Size = new Size(550, 589);
            KeyDown += CodeEditorBox2_KeyDown;
            KeyPress += CodeEditorBox2_KeyPress;
            PreviewKeyDown += CodeEditorBox2_PreviewKeyDown;
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private VScrollBar vScrollBar1;
        private DoubleBufferedPanel panel1;
        private HScrollBar hScrollBar1;
        private ToolTip hoverToolTip;
        private Panel panel2;
        private Label lineLabel;
        private ToolTip methodToolTip;
    }
}
