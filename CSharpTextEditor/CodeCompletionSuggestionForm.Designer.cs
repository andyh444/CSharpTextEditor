namespace CSharpTextEditor
{
    partial class CodeCompletionSuggestionForm
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
            listBox = new ListBox();
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.Dock = DockStyle.Fill;
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new Point(0, 0);
            listBox.Name = "listBox";
            listBox.Size = new Size(189, 200);
            listBox.TabIndex = 0;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            listBox.MouseDoubleClick += listBox_MouseDoubleClick;
            // 
            // CodeCompletionSuggestionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(189, 200);
            ControlBox = false;
            Controls.Add(listBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "CodeCompletionSuggestionForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "CodeCompletionSuggestionForm";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox;
    }
}