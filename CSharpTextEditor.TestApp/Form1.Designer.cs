namespace CSharpTextEditor.TestApp
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
            highlightButton = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            codeEditorBox21 = new CodeEditorBox();
            comboBox1 = new System.Windows.Forms.ComboBox();
            undoButton = new System.Windows.Forms.Button();
            redoButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // highlightButton
            // 
            highlightButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            highlightButton.Location = new System.Drawing.Point(12, 986);
            highlightButton.Name = "highlightButton";
            highlightButton.Size = new System.Drawing.Size(122, 23);
            highlightButton.TabIndex = 1;
            highlightButton.Text = "Syntax Highlight";
            highlightButton.UseVisualStyleBackColor = true;
            highlightButton.Click += highlightButton_Click;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button1.Location = new System.Drawing.Point(140, 986);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(122, 23);
            button1.TabIndex = 2;
            button1.Text = "Compile and Run";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Location = new System.Drawing.Point(1162, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(370, 965);
            textBox1.TabIndex = 3;
            // 
            // codeEditorBox21
            // 
            codeEditorBox21.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            codeEditorBox21.Font = new System.Drawing.Font("Cascadia Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            codeEditorBox21.Location = new System.Drawing.Point(12, 42);
            codeEditorBox21.Margin = new System.Windows.Forms.Padding(4);
            codeEditorBox21.Name = "codeEditorBox21";
            codeEditorBox21.Size = new System.Drawing.Size(1143, 935);
            codeEditorBox21.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Light", "Dark" });
            comboBox1.Location = new System.Drawing.Point(13, 12);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(121, 23);
            comboBox1.TabIndex = 5;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // undoButton
            // 
            undoButton.Enabled = false;
            undoButton.Location = new System.Drawing.Point(140, 11);
            undoButton.Name = "undoButton";
            undoButton.Size = new System.Drawing.Size(75, 23);
            undoButton.TabIndex = 6;
            undoButton.Text = "Undo";
            undoButton.UseVisualStyleBackColor = true;
            undoButton.Click += undoButton_Click;
            // 
            // redoButton
            // 
            redoButton.Enabled = false;
            redoButton.Location = new System.Drawing.Point(221, 11);
            redoButton.Name = "redoButton";
            redoButton.Size = new System.Drawing.Size(75, 23);
            redoButton.TabIndex = 7;
            redoButton.Text = "Redo";
            redoButton.UseVisualStyleBackColor = true;
            redoButton.Click += redoButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1544, 1021);
            Controls.Add(redoButton);
            Controls.Add(undoButton);
            Controls.Add(comboBox1);
            Controls.Add(codeEditorBox21);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(highlightButton);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button highlightButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private CodeEditorBox codeEditorBox21;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.Button redoButton;
    }
}