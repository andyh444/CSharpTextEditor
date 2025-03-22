using System.Drawing;
using System.Windows.Forms;
using CSharpTextEditor.View.Winforms;

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
            vScrollBar = new VScrollBar();
            codePanel = new DoubleBufferedPanel();
            hScrollBar = new HScrollBar();
            lineLabel = new Label();
            mainTableLayout = new TableLayoutPanel();
            footerTableLayout = new TableLayoutPanel();
            cursorBlinkTimer = new Timer(this.components);
            mainTableLayout.SuspendLayout();
            footerTableLayout.SuspendLayout();
            SuspendLayout();
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = DockStyle.Fill;
            vScrollBar.Location = new Point(533, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new Size(17, 570);
            vScrollBar.TabIndex = 0;
            vScrollBar.Scroll += vScrollBar_Scroll;
            // 
            // codePanel
            // 
            codePanel.Cursor = Cursors.IBeam;
            codePanel.Dock = DockStyle.Fill;
            codePanel.Location = new Point(0, 0);
            codePanel.Margin = new Padding(0);
            codePanel.Name = "codePanel";
            codePanel.Size = new Size(533, 570);
            codePanel.TabIndex = 1;
            codePanel.Paint += codePanel_Paint;
            codePanel.MouseClick += codePanel_MouseClick;
            codePanel.MouseDoubleClick += codePanel_MouseDoubleClick;
            codePanel.MouseDown += codePanel_MouseDown;
            codePanel.MouseMove += codePanel_MouseMove;
            codePanel.MouseUp += codePanel_MouseUp;
            // 
            // hScrollBar
            // 
            hScrollBar.Dock = DockStyle.Fill;
            hScrollBar.Location = new Point(0, 0);
            hScrollBar.Name = "hScrollBar";
            hScrollBar.Size = new Size(417, 19);
            hScrollBar.TabIndex = 2;
            hScrollBar.Scroll += hScrollBar_Scroll;
            // 
            // lineLabel
            // 
            lineLabel.Dock = DockStyle.Fill;
            lineLabel.Font = new Font("Tahoma", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            lineLabel.Location = new Point(420, 0);
            lineLabel.Name = "lineLabel";
            lineLabel.Size = new Size(127, 19);
            lineLabel.TabIndex = 3;
            lineLabel.Text = "Ln: 0 Ch: 0";
            lineLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // mainTableLayout
            // 
            mainTableLayout.ColumnCount = 2;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 17F));
            mainTableLayout.Controls.Add(footerTableLayout, 0, 1);
            mainTableLayout.Controls.Add(codePanel, 0, 0);
            mainTableLayout.Controls.Add(vScrollBar, 1, 0);
            mainTableLayout.Dock = DockStyle.Fill;
            mainTableLayout.Location = new Point(0, 0);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.RowCount = 2;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 19F));
            mainTableLayout.Size = new Size(550, 589);
            mainTableLayout.TabIndex = 0;
            // 
            // footerTableLayout
            // 
            footerTableLayout.ColumnCount = 2;
            mainTableLayout.SetColumnSpan(footerTableLayout, 2);
            footerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
            footerTableLayout.Controls.Add(hScrollBar, 0, 0);
            footerTableLayout.Controls.Add(lineLabel, 1, 0);
            footerTableLayout.Dock = DockStyle.Fill;
            footerTableLayout.Location = new Point(0, 570);
            footerTableLayout.Margin = new Padding(0);
            footerTableLayout.Name = "footerTableLayout";
            footerTableLayout.RowCount = 1;
            footerTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            footerTableLayout.Size = new Size(550, 19);
            footerTableLayout.TabIndex = 0;
            // 
            // cursorBlinkTimer
            // 
            this.cursorBlinkTimer.Tick += new System.EventHandler(this.cursorBlinkTimer_Tick);
            // 
            // CodeEditorBox
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainTableLayout);
            Font = new Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "CodeEditorBox";
            Size = new Size(550, 589);
            KeyDown += CodeEditorBox_KeyDown;
            KeyPress += CodeEditorBox_KeyPress;
            PreviewKeyDown += CodeEditorBox_PreviewKeyDown;
            mainTableLayout.ResumeLayout(false);
            footerTableLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private VScrollBar vScrollBar;
        private DoubleBufferedPanel codePanel;
        private HScrollBar hScrollBar;
        private Panel panel2;
        private Label lineLabel;
        private TableLayoutPanel mainTableLayout;
        private TableLayoutPanel footerTableLayout;
        private Timer cursorBlinkTimer;
    }
}
