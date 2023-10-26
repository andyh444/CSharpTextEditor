﻿namespace CSharpTextEditor.TestApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            codeEditorBox1 = new CodeEditorBox();
            highlightButton = new Button();
            button1 = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // codeEditorBox1
            // 
            codeEditorBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            codeEditorBox1.Location = new Point(12, 12);
            codeEditorBox1.Name = "richTextBox1";
            codeEditorBox1.Size = new Size(796, 426);
            codeEditorBox1.TabIndex = 0;
            // 
            // highlightButton
            // 
            highlightButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            highlightButton.Location = new Point(12, 447);
            highlightButton.Name = "highlightButton";
            highlightButton.Size = new Size(122, 23);
            highlightButton.TabIndex = 1;
            highlightButton.Text = "Syntax Highlight";
            highlightButton.UseVisualStyleBackColor = true;
            highlightButton.Click += highlightButton_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button1.Location = new Point(140, 447);
            button1.Name = "button1";
            button1.Size = new Size(122, 23);
            button1.TabIndex = 2;
            button1.Text = "Compile and Run";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            textBox1.Location = new Point(814, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(370, 426);
            textBox1.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1196, 482);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(highlightButton);
            Controls.Add(codeEditorBox1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CodeEditorBox codeEditorBox1;
        private Button highlightButton;
        private Button button1;
        private TextBox textBox1;
    }
}