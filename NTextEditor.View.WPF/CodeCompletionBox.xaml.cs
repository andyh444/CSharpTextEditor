using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NTextEditor.View.WPF
{
    /// <summary>
    /// Interaction logic for CodeCompletionBox.xaml
    /// </summary>
    public partial class CodeCompletionBox : UserControl
    {
        public CodeCompletionBox()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var item = (sender as ListBox)?.SelectedItem;

            // Hit test to check if an actual item was clicked
            var hit = VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox));
            if (hit == null)
            {
                return;
            }
            Debugger.Break();
        }
    }
}
