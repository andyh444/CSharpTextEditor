using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NTextEditor.WinformsTestApp
{
    public partial class FindTextForm : Form
    {
        public class FindTextEventArgs(string text, bool matchCase, bool wraparound) : EventArgs
        {
            public string Text { get; } = text;

            public bool MatchCase { get; } = matchCase;

            public bool Wraparound { get; } = wraparound;
        }

        public event EventHandler<FindTextEventArgs>? FindText;

        public FindTextForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            findTextTextBox.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // don't actually close; just hide
            e.Cancel = true;
            Visible = false;
        }

        private void findNextButton_Click(object sender, EventArgs e)
        {
            FindText?.Invoke(this, new FindTextEventArgs(findTextTextBox.Text, matchCaseCheckBox.Checked, wraparoundCheckBox.Checked));
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void findTextTextBox_Validated(object sender, EventArgs e)
        {
            findNextButton.PerformClick();
        }

        private void findTextTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // prevent the beep sound
                findNextButton.PerformClick();
            }
        }
    }
}
