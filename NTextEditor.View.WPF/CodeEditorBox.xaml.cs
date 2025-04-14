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
            using SKFont font = new SKFont(SKTypeface.FromFamilyName(FontFamily.Source), (float)FontSize);
            /*font.Subpixel = false;
            font.LinearMetrics = true;
            font.Hinting = SKFontHinting.None;
            font.ForceAutoHinting = false;
            font.Edging = SKFontEdging.Alias;*/

            SkiaCanvas canvas = new SkiaCanvas(e.Surface.Canvas,
                new System.Drawing.Size(e.Info.Width, e.Info.Height),
                font);

            var size = canvas.GetTextSize("A", false);
            _viewManager.CharacterWidth = size.Width;
            _viewManager.LineWidth = (int)size.Height;

            _viewManager.Draw(canvas, new DrawSettings(true, true));
        }

        public void CursorsChanged()
        {
            SkiaSurface.InvalidateVisual();
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
            HandleCoreKeyDownEvent(e);
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

        private void HandleCoreKeyDownEvent(KeyEventArgs e)
        {
            // handles the set of keyboard presses that can't be customised
            bool ensureInView = true;
            switch (e.Key)
            {
                case Key.Escape:
                    break;
                case Key.Back:
                    _viewManager.SourceCode.RemoveCharacterBeforeActivePosition();
                    break;
                case Key.Delete:
                    _viewManager.SourceCode.RemoveCharacterAfterActivePosition();
                    break;
                case Key.Left:
                    _viewManager.SourceCode.ShiftHeadToTheLeft(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.Right:
                    _viewManager.SourceCode.ShiftHeadToTheRight(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.Up:
                    _viewManager.SourceCode.ShiftHeadUpOneLine(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.Down:
                    _viewManager.SourceCode.ShiftHeadDownOneLine(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.End:
                    _viewManager.SourceCode.ShiftHeadToEndOfLine(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.Home:
                    _viewManager.SourceCode.ShiftHeadToStartOfLine(Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.PageUp:
                    _viewManager.SourceCode.ShiftHeadUpLines((int)Height / _viewManager.LineWidth, Keyboard.IsKeyDown(Key.LeftShift));
                    break;
                case Key.PageDown:
                    _viewManager.SourceCode.ShiftHeadDownLines((int)Height / _viewManager.LineWidth, Keyboard.IsKeyDown(Key.LeftShift));
                    break;

                case Key.Enter:
                    _viewManager.SourceCode.InsertLineBreakAtActivePosition(_viewManager.SpecialCharacterHandler);
                    break;
                case Key.Tab:
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        _viewManager.SourceCode.DecreaseIndentAtActivePosition();
                    }
                    else
                    {
                        _viewManager.SourceCode.IncreaseIndentAtActivePosition();
                    }
                    break;
                case Key.Insert:
                    _viewManager.SourceCode.OvertypeEnabled = !_viewManager.SourceCode.OvertypeEnabled;
                    break;
                default:
                    ensureInView = false;
                    break;
            }
            if (ensureInView)
            {
                _viewManager.EnsureActivePositionInView(new System.Drawing.Size((int)Width, (int)Height));
            }
        }

        private void CodeEditorBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // needed so that the KeyDown event picks up the arrowkeys and tab key
            if (e.Key == Key.Right
                || e.Key == Key.Left
                || e.Key == Key.Up
                || e.Key == Key.Down
                || e.Key == Key.Tab)
            {
                e.Handled = true;
                HandleCoreKeyDownEvent(e);
            }
        }
    }
}
