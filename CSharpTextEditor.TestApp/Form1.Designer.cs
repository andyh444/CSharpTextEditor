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
            highlightButton = new Button();
            button1 = new Button();
            textBox1 = new TextBox();
            codeEditorBox21 = new CodeEditorBox();
            comboBox1 = new ComboBox();
            undoButton = new Button();
            redoButton = new Button();
            SuspendLayout();
            // 
            // highlightButton
            // 
            highlightButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            highlightButton.Location = new Point(12, 986);
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
            button1.Location = new Point(140, 986);
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
            textBox1.Location = new Point(1162, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(370, 965);
            textBox1.TabIndex = 3;
            // 
            // codeEditorBox21
            // 
            codeEditorBox21.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            codeEditorBox21.Font = new Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point);
            codeEditorBox21.Location = new Point(12, 42);
            codeEditorBox21.Margin = new Padding(4);
            codeEditorBox21.Name = "codeEditorBox21";
            codeEditorBox21.Size = new Size(1143, 935);
            codeEditorBox21.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Light", "Dark" });
            comboBox1.Location = new Point(13, 12);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 5;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // undoButton
            // 
            undoButton.Enabled = false;
            undoButton.Location = new Point(140, 11);
            undoButton.Name = "undoButton";
            undoButton.Size = new Size(75, 23);
            undoButton.TabIndex = 6;
            undoButton.Text = "Undo";
            undoButton.UseVisualStyleBackColor = true;
            undoButton.Click += undoButton_Click;
            // 
            // redoButton
            // 
            redoButton.Enabled = false;
            redoButton.Location = new Point(221, 11);
            redoButton.Name = "redoButton";
            redoButton.Size = new Size(75, 23);
            redoButton.TabIndex = 7;
            redoButton.Text = "Redo";
            redoButton.UseVisualStyleBackColor = true;
            redoButton.Click += redoButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1544, 1021);
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

        private Button highlightButton;
        private Button button1;
        private TextBox textBox1;
        private CodeEditorBox codeEditorBox21;
        private ComboBox comboBox1;
        private Button undoButton;
        private Button redoButton;
    }
}