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
            richTextBox1 = new ExtendedRichTextBox();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.AcceptsTab = true;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Font = new Font("Consolas", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(540, 450);
            richTextBox1.TabIndex = 0;
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            richTextBox1.KeyDown += richTextBox1_KeyDown;
            richTextBox1.Text = "using System;\n\npublic static class Program\n{\n\tpublic static void Main()\n\t{\n\t\tConsole.WriteLine(\"Hello world\");\n\t}\n}";
            // 
            // CodeEditorBox
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(richTextBox1);
            Name = "CodeEditorBox";
            Size = new Size(540, 450);
            ResumeLayout(false);
        }

        #endregion

        private ExtendedRichTextBox richTextBox1;
    }
}
