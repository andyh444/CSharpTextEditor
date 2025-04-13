﻿using System.Drawing;
using System.Windows.Forms;

namespace NTextEditor.View.Winforms
{
    internal partial class CodeCompletionSuggestionForm
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
            components = new System.ComponentModel.Container();
            listBox = new ListBox();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.Dock = DockStyle.Fill;
            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new Point(0, 0);
            listBox.Name = "listBox";
            listBox.Size = new Size(189, 200);
            listBox.TabIndex = 0;
            listBox.DrawItem += listBox_DrawItem;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            listBox.MouseDoubleClick += listBox_MouseDoubleClick;
            // 
            // toolTip1
            // 
            toolTip1.AutomaticDelay = 0;
            toolTip1.BackColor = SystemColors.ControlLightLight;
            toolTip1.OwnerDraw = true;
            toolTip1.ShowAlways = true;
            toolTip1.Draw += toolTip1_Draw;
            // 
            // CodeCompletionSuggestionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(189, 200);
            ControlBox = false;
            Controls.Add(listBox);
            FormBorderStyle = FormBorderStyle.None;
            Name = "CodeCompletionSuggestionForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Code Completion Suggestions";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox;
        private ToolTip toolTip1;
    }
}