using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NTextEditor.Languages;
using NTextEditor.Languages.CSharp;

namespace NTextEditor.WPFTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new MainViewModel(codeEditorBox);
            DataContext = _model;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedItem is SyntaxDiagnostic sd)
            {
                codeEditorBox.GoToPosition(sd.Start.LineNumber, sd.Start.ColumnNumber);
            }
        }
    }
}