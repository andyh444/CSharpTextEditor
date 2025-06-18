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
        public event Action<string>? FindText;

        public FindTextForm()
        {
            InitializeComponent();
        }

        private void findNextButton_Click(object sender, EventArgs e)
        {
            FindText?.Invoke(findTextTextBox.Text);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Visible = false;
        }
    }
}
