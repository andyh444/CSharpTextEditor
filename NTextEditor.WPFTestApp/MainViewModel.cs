using CommunityToolkit.Mvvm.Input;
using NTextEditor.Languages;
using NTextEditor.Languages.CSharp;
using NTextEditor.Languages.PlainText;
using NTextEditor.Languages.VisualBasic;
using NTextEditor.View;
using NTextEditor.View.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.WPFTestApp
{
    public class MainViewModel
    {
        public class CodeEditorLanguage(string name, Action<CodeEditorBox> setLanguage)
        {
            public string Name { get; } = name;
            public Action<CodeEditorBox> SetLanguage { get; } = setLanguage;
        }
        public class CodeEditorTheme(string name, SyntaxPalette syntaxPalette)
        {
            public string Name { get; } = name;
            public SyntaxPalette SyntaxPalette { get; } = syntaxPalette;
        }

        private readonly CodeEditorBox _codeEditorBox;
        private CodeEditorLanguage _language;
        private CodeEditorTheme _theme;
        private string _font;
        public IRelayCommand UndoCommand { get; }
        public IRelayCommand RedoCommand { get; }
        public ObservableCollection<CodeEditorLanguage> Languages { get; }
        public ObservableCollection<CodeEditorTheme> Themes { get; }
        public ObservableCollection<string> Fonts { get; }
        public ObservableCollection<SyntaxDiagnostic> Diagnostics => _codeEditorBox.Diagnostics;

        public CodeEditorLanguage Language
        {
            get => _language;
            set
            {
                _language = value;
                UpdateLanguage();
            }
        }

        public CodeEditorTheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                UpdateTheme();
            }
        }

        public string Font
        {
            get => _font;
            set
            {
                _font = value;
                UpdateFont();
            }
        }

        public MainViewModel(CodeEditorBox codeEditorBox)
        {
            _codeEditorBox = codeEditorBox;

            UndoCommand = new RelayCommand(() => codeEditorBox.Undo());
            RedoCommand = new RelayCommand(() => codeEditorBox.Redo());

            Languages = [
                new CodeEditorLanguage("C# Class Library", c => c.SetLanguageToCSharp(true)),
                new CodeEditorLanguage("C# Executable", c => c.SetLanguageToCSharp(false)),
                new CodeEditorLanguage("VB Class Library", c => c.SetLanguageToVisualBasic(true)),
                new CodeEditorLanguage("VB Executable", c => c.SetLanguageToVisualBasic(false)),
                new CodeEditorLanguage("Plain Text", c => c.SetLanguageToPlainText())
                ];
            _language = Languages.First();

            Themes = [
                new CodeEditorTheme("Light", SyntaxPalette.GetLightModePalette()),
                new CodeEditorTheme("Dark", SyntaxPalette.GetDarkModePalette())
                ];
            _theme = Themes.First();

            Fonts = ["Consolas", "Cascadia Mono"];
            _font = Fonts.First();

            UpdateLanguage();
            UpdateTheme();
            UpdateFont();
        }

        private void UpdateLanguage()
        {
            _language.SetLanguage(_codeEditorBox);
        }

        private void UpdateTheme()
        {
            _codeEditorBox.SetPalette(_theme.SyntaxPalette);
        }

        private void UpdateFont()
        {
            _codeEditorBox.FontFamily = new System.Windows.Media.FontFamily(_font);
        }
    }
}
