using NTextEditor.Languages;
using NTextEditor.Source;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CodeEditorBox.xaml
    /// </summary>
    public partial class CodeEditorBox : UserControl, ISourceCodeListener, ILanguageManager
    {
        private ViewManager _viewManager;

        public CodeEditorBox()
        {
            InitializeComponent();
            _viewManager = new ViewManager(this, new WpfClipboard());

            _viewManager.CharacterWidth = 12;
            _viewManager.LineWidth = 12;
            _viewManager.SyntaxPalette = SyntaxPalette.GetLightModePalette();

            UpdateSyntaxHighlighting();
        }

        private void SkiaSurface_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SkiaCanvas canvas = new SkiaCanvas(e.Surface.Canvas,
                new System.Drawing.Size(e.Info.Width, e.Info.Height),
                new SKFont(SKTypeface.FromFamilyName(FontFamily.Source), (float)FontSize));

            var size = canvas.GetTextSize("A");
            _viewManager.CharacterWidth = size.Width;
            _viewManager.LineWidth = size.Height;

            _viewManager.Draw(canvas, new DrawSettings(true, true));
        }

        public void CursorsChanged()
        {
        }

        public void HideHoverToolTip()
        {
        }

        public void HideMethodToolTip()
        {
        }

        public void ShowHoverToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, System.Drawing.Point point)
        {
        }

        public void ShowMethodToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, System.Drawing.Point point)
        {
        }

        public void TextChanged()
        {
            UpdateSyntaxHighlighting();
            SkiaSurface.InvalidateVisual();
        }

        public void SetLanguage(ISyntaxHighlighter syntaxHighlighter, ICodeExecutor? codeExecutor, ISpecialCharacterHandler specialCharacterHandler)
        {
            _viewManager.SyntaxHighlighter = syntaxHighlighter;
            //_viewManager.CodeExecutor = codeExecutor;
            _viewManager.SpecialCharacterHandler = specialCharacterHandler;
            UpdateSyntaxHighlighting();
        }

        private void UpdateSyntaxHighlighting()
        {
            if (_viewManager.SyntaxHighlighter == null)
            {
                return;
            }

            _viewManager.SyntaxHighlighter.Update(_viewManager.SourceCode.Lines);
            _viewManager.CurrentHighlighting = _viewManager.SyntaxHighlighter.GetHighlightings(_viewManager.SyntaxPalette);
            //DiagnosticsChanged?.Invoke(this, _viewManager.CurrentHighlighting.Diagnostics);
        }

        private void CodeEditorBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewManager.SourceCode.InsertLineBreakAtActivePosition(_viewManager.SpecialCharacterHandler);
            }
            else if (e.Key == Key.Back)
            {
                _viewManager.SourceCode.RemoveCharacterBeforeActivePosition();
            }
        }

        private void CodeEditorBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
            {
                return;
            }
            char c = e.Text[0];
            if (char.IsControl(c))
            {
                return;
            }
            _viewManager.SourceCode.InsertCharacterAtActivePosition(e.Text[0], _viewManager.SpecialCharacterHandler);
        }

        private void CodeEditorBox_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    }
}
