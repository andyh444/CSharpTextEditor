﻿using NTextEditor.Languages;
using NTextEditor.Languages.PlainText;
using NTextEditor.Source;
using NTextEditor.View.ToolTips;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace NTextEditor.View.WPF
{
    /// <summary>
    /// Interaction logic for CodeEditorBox.xaml
    /// </summary>
    public partial class CodeEditorBox : UserControl, ISourceCodeListener, ILanguageManager
    {
        private ViewManager _viewManager;
        private KeyboardShortcutManager _keyboardShortcutManager;
        private CodeEditorBoxViewModel _viewModel;
        private bool _cursorVisible;
        private DispatcherTimer _timer;
        private ToolTip? _methodToolTip;
        private ToolTip? _hoverToolTip;

        public static readonly DependencyProperty DiagnosticsProperty = DependencyProperty.Register(
            "Diagnostics",
            typeof(ObservableCollection<SyntaxDiagnostic>),
            typeof(CodeEditorBox),
            new PropertyMetadata(new ObservableCollection<SyntaxDiagnostic>()));

        public ObservableCollection<SyntaxDiagnostic> Diagnostics
        {
            get => (ObservableCollection<SyntaxDiagnostic>)GetValue(DiagnosticsProperty);
            private set => SetValue(DiagnosticsProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(CodeEditorBox),
            new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                //_viewManager.SourceCode.Text = value;
                //TextChanged();
            }
        }

        public CodeEditorBox()
        {
            InitializeComponent();

            _viewManager = new ViewManager(this, new WpfClipboard());

            _viewManager.SyntaxPalette = SyntaxPalette.GetLightModePalette();
            _viewManager.LineWidth = 1;

            _keyboardShortcutManager = KeyboardShortcutManager.CreateDefault();

            _viewManager.HorizontalScrollChanged += _viewManager_HorizontalScrollChanged;
            _viewManager.VerticalScrollChanged += _viewManager_VerticalScrollChanged;

            _viewModel = new CodeEditorBoxViewModel(_viewManager);

            DataContext = _viewModel;

            _timer = new DispatcherTimer();
            _timer.Interval = NativeMethods.CaretBlinkTime;
            _timer.IsEnabled = true;
            _timer.Tick += Timer_Tick;

            UpdateSyntaxHighlighting();

            this.SetLanguageToPlainText();
        }

        public void GoToPosition(int line, int column)
        {
            _viewManager.SourceCode.SetActivePosition(line, column);
            _viewManager.EnsureActivePositionInView(new System.Drawing.Size((int)SkiaSurface.ActualWidth, (int)SkiaSurface.ActualHeight));
            Focus();
        }

        public void SetPalette(SyntaxPalette syntaxPalette)
        {
            _viewManager.SyntaxPalette = syntaxPalette;
            UpdateSyntaxHighlighting();
            SkiaSurface.InvalidateVisual();
        }

        private void _viewManager_VerticalScrollChanged()
        {
            int maxScrollPosition = _viewManager.GetMaxVerticalScrollPosition();
            _viewModel.VerticalScrollValue = maxScrollPosition == 0
                ? 0
                : (int)((_viewModel.VerticalScrollMax * (long)_viewManager.VerticalScrollPositionPX) / maxScrollPosition);
            CursorsChanged();
        }

        private void _viewManager_HorizontalScrollChanged()
        {
            int maxScrollPosition = _viewManager.GetMaxHorizontalScrollPosition();
            _viewModel.HorizontalScrollValue = maxScrollPosition == 0
                ? 0
                : (int)((_viewModel.HorizontalScrollMax * (long)_viewManager.HorizontalScrollPositionPX) / maxScrollPosition);
            CursorsChanged();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _cursorVisible = !_cursorVisible;
            if (IsFocused)
            {
                SkiaSurface.InvalidateVisual();
            }
        }

        private void ResetCursorBlinkStatus()
        {
            _cursorVisible = true;
            _timer.Stop();
            _timer.Start();
        }

        private void SkiaSurface_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            using SKFont font = new SKFont(SKTypeface.FromFamilyName(FontFamily.Source), (float)FontSize);

            SkiaCanvas canvas = new SkiaCanvas(e.Surface.Canvas,
                new System.Drawing.Size(e.Info.Width, e.Info.Height),
                font);

            var size = canvas.GetTextSize("A", false);
            _viewManager.CharacterWidth = size.Width;
            _viewManager.LineWidth = (int)size.Height;

            _viewManager.Draw(canvas, new DrawSettings(IsFocused, _cursorVisible));
        }

        public void CursorsChanged()
        {
            _viewModel.LineAndColumnNumberText = _viewManager.GetLineAndCharacterLabel();
            ResetCursorBlinkStatus();
            SkiaSurface.InvalidateVisual();
        }

        public void HideHoverToolTip()
        {
            if (_hoverToolTip != null)
            {
                _hoverToolTip.IsOpen = false;
            }
        }

        public void HideMethodToolTip()
        {
            if (_methodToolTip != null)
            {
                _methodToolTip.IsOpen = false;
            }
        }

        public void ShowHoverToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, System.Drawing.Point point)
        {
            ShowToolTip(ref _hoverToolTip, palette, toolTipContents, point);
        }

        public void ShowMethodToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, System.Drawing.Point point)
        {
            ShowToolTip(ref _methodToolTip, palette, toolTipContents, point);
        }

        private void ShowToolTip(ref ToolTip? toolTip, SyntaxPalette palette, IToolTipContents toolTipContents, System.Drawing.Point point)
        {
            if (toolTip == null)
            {
                toolTip = new ToolTip()
                {
                    PlacementTarget = this,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse
                };
            }
            Color colour = palette.BackColour.ToWpfColour();
            toolTip.Background = new SolidColorBrush(colour);
            ToolTip = toolTip;

            // TODO: Does the stackpanel need the background setting?
            StackPanel stackPanel = new StackPanel();

            StackPanelToolTipDrawBuilder drawBuilder = new StackPanelToolTipDrawBuilder(stackPanel);
            drawBuilder.Add(toolTipContents.GetElements(new WpfIconCache(), palette));

            toolTip.Content = stackPanel;
            toolTip.IsOpen = true;
        }

        public void TextChanged()
        {
            Text = _viewManager.SourceCode.Text;
            UpdateSyntaxHighlighting();
            _viewModel.UpdateScrollBarMaxima();
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
            Diagnostics.Clear();
            foreach (var d in _viewManager.CurrentHighlighting.Diagnostics)
            {
                Diagnostics.Add(d);
            }
            //DiagnosticsChanged?.Invoke(this, _viewManager.CurrentHighlighting.Diagnostics);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var point = e.GetPosition(this);
            if (e.ClickCount == 2)
            {
                _viewManager.HandleLeftMouseDoubleClick(new System.Drawing.Point((int)point.X, (int)point.Y));
                return;
            }
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            _viewManager.HandleLeftMouseDown(new System.Drawing.Point((int)point.X, (int)point.Y), ctrlPressed, altPressed);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            var point = e.GetPosition(this);
            _viewManager.HandleLeftMouseUp(new System.Drawing.Point((int)point.X, (int)point.Y));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var p = e.GetPosition(this);
            var point  = new System.Drawing.Point((int)p.X, (int)p.Y);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
                _viewManager.HandleLeftMouseDrag(point, altPressed, new System.Drawing.Size((int)SkiaSurface.ActualWidth, (int)SkiaSurface.ActualHeight));
            }
            else if (e.RightButton == MouseButtonState.Released
                && e.MiddleButton == MouseButtonState.Released)
            {
                _viewManager.HandleMouseMove(point);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // TODO: Update font size
            }
            else
            {
                _viewManager.ScrollView(-3 * Math.Sign(e.Delta));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            HandleKeyPress(e.Key);
        }

        private void HandleKeyPress(Key key)
        {
            bool shortcutProcessed = _keyboardShortcutManager.ProcessShortcut(
                controlPressed: Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                shiftPressed: Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                altPressed: Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt),
                keyCode: key.ToTextEditorKey(),
                viewManager: _viewManager,
                out bool ensureInView);
            if (shortcutProcessed)
            {
                if (ensureInView)
                {
                    //HideCodeCompletionForm();
                    _viewManager.EnsureActivePositionInView(new System.Drawing.Size((int)SkiaSurface.ActualWidth, (int)SkiaSurface.ActualHeight));
                }
            }
            else
            {
                HandleCoreKeyDownEvent(key);
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
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

        private void HandleCoreKeyDownEvent(Key key)
        {
            // handles the set of keyboard presses that can't be customised
            bool ensureInView = true;
            switch (key)
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
                _viewManager.EnsureActivePositionInView(new System.Drawing.Size((int)SkiaSurface.ActualWidth, (int)SkiaSurface.ActualHeight));
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (!IsKeyboardFocusWithin)
            {
                Focus();
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            // needed so that the KeyDown event picks up the arrowkeys and tab key
            if (e.Key == Key.Right
                || e.Key == Key.Left
                || e.Key == Key.Up
                || e.Key == Key.Down
                || e.Key == Key.Tab)
            {
                e.Handled = true;
                HandleKeyPress(e.Key);
            }
        }

        public void Undo() => _viewManager.Undo();

        public void Redo() => _viewManager.Redo();
    }
}
