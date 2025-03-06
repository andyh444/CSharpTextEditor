namespace CSharpTextEditor.TestApp
{
    partial class MainForm
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
            this.executeButton = new System.Windows.Forms.Button();
            this.executionTextBox = new System.Windows.Forms.TextBox();
            this.paletteComboBox = new System.Windows.Forms.ComboBox();
            this.undoButton = new System.Windows.Forms.Button();
            this.redoButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.typeCombobox = new System.Windows.Forms.ComboBox();
            this.codeEditorBox = new CSharpTextEditor.CodeEditorBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.diagnosticsView = new System.Windows.Forms.ListView();
            this.positionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.idHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.descriptionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // executeButton
            // 
            this.executeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.executeButton.Location = new System.Drawing.Point(3, 3);
            this.executeButton.Name = "executeButton";
            this.executeButton.Size = new System.Drawing.Size(105, 20);
            this.executeButton.TabIndex = 2;
            this.executeButton.Text = "Execute";
            this.executeButton.UseVisualStyleBackColor = true;
            this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
            // 
            // executionTextBox
            // 
            this.executionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executionTextBox.Location = new System.Drawing.Point(995, 35);
            this.executionTextBox.Multiline = true;
            this.executionTextBox.Name = "executionTextBox";
            this.tableLayoutPanel1.SetRowSpan(this.executionTextBox, 2);
            this.executionTextBox.Size = new System.Drawing.Size(325, 814);
            this.executionTextBox.TabIndex = 3;
            // 
            // paletteComboBox
            // 
            this.paletteComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.paletteComboBox.FormattingEnabled = true;
            this.paletteComboBox.Items.AddRange(new object[] {
            "Light",
            "Dark"});
            this.paletteComboBox.Location = new System.Drawing.Point(3, 3);
            this.paletteComboBox.Name = "paletteComboBox";
            this.paletteComboBox.Size = new System.Drawing.Size(104, 21);
            this.paletteComboBox.TabIndex = 5;
            this.paletteComboBox.SelectedIndexChanged += new System.EventHandler(this.paletteComboBox_SelectedIndexChanged);
            // 
            // undoButton
            // 
            this.undoButton.Enabled = false;
            this.undoButton.Location = new System.Drawing.Point(113, 3);
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new System.Drawing.Size(64, 20);
            this.undoButton.TabIndex = 6;
            this.undoButton.Text = "Undo";
            this.undoButton.UseVisualStyleBackColor = true;
            this.undoButton.Click += new System.EventHandler(this.undoButton_Click);
            // 
            // redoButton
            // 
            this.redoButton.Enabled = false;
            this.redoButton.Location = new System.Drawing.Point(183, 3);
            this.redoButton.Name = "redoButton";
            this.redoButton.Size = new System.Drawing.Size(64, 20);
            this.redoButton.TabIndex = 7;
            this.redoButton.Text = "Redo";
            this.redoButton.UseVisualStyleBackColor = true;
            this.redoButton.Click += new System.EventHandler(this.redoButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.codeEditorBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.executionTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.diagnosticsView, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1323, 885);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.paletteComboBox);
            this.flowLayoutPanel1.Controls.Add(this.undoButton);
            this.flowLayoutPanel1.Controls.Add(this.redoButton);
            this.flowLayoutPanel1.Controls.Add(this.typeCombobox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1323, 32);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // typeCombobox
            // 
            this.typeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeCombobox.FormattingEnabled = true;
            this.typeCombobox.Items.AddRange(new object[] {
            "Class Library",
            "Executable"});
            this.typeCombobox.Location = new System.Drawing.Point(253, 3);
            this.typeCombobox.Name = "typeCombobox";
            this.typeCombobox.Size = new System.Drawing.Size(121, 21);
            this.typeCombobox.TabIndex = 8;
            this.typeCombobox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // codeEditorBox
            // 
            this.codeEditorBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeEditorBox.Font = new System.Drawing.Font("Cascadia Mono", 12F);
            this.codeEditorBox.Location = new System.Drawing.Point(4, 36);
            this.codeEditorBox.Margin = new System.Windows.Forms.Padding(4);
            this.codeEditorBox.Name = "codeEditorBox";
            this.codeEditorBox.Size = new System.Drawing.Size(984, 533);
            this.codeEditorBox.TabIndex = 4;
            // 
            // flowLayoutPanel2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Controls.Add(this.executeButton);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 852);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(1323, 33);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // diagnosticsView
            // 
            this.diagnosticsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.positionHeader,
            this.idHeader,
            this.descriptionHeader});
            this.diagnosticsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagnosticsView.FullRowSelect = true;
            this.diagnosticsView.HideSelection = false;
            this.diagnosticsView.Location = new System.Drawing.Point(3, 576);
            this.diagnosticsView.Name = "diagnosticsView";
            this.diagnosticsView.Size = new System.Drawing.Size(986, 273);
            this.diagnosticsView.TabIndex = 5;
            this.diagnosticsView.UseCompatibleStateImageBehavior = false;
            this.diagnosticsView.View = System.Windows.Forms.View.Details;
            this.diagnosticsView.DoubleClick += new System.EventHandler(this.diagnosticsView_DoubleClick);
            // 
            // positionHeader
            // 
            this.positionHeader.Text = "Position";
            // 
            // idHeader
            // 
            this.idHeader.Text = "ID";
            // 
            // descriptionHeader
            // 
            this.descriptionHeader.Text = "Description";
            this.descriptionHeader.Width = 862;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1323, 885);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Button executeButton;
        private TextBox executionTextBox;
        private CodeEditorBox codeEditorBox;
        private ComboBox paletteComboBox;
        private Button undoButton;
        private Button redoButton;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private ListView diagnosticsView;
        private ColumnHeader positionHeader;
        private ColumnHeader descriptionHeader;
        private ColumnHeader idHeader;
        private ComboBox typeCombobox;
    }
}