using NTextEditor.Languages;
using NTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NTextEditor.View.WPF
{
    internal class CodeCompletionViewModel : INotifyPropertyChanged
    {
        internal class SuggestionItem(BitmapImage? image, string text)
        {
            public ImageSource Image { get; } = image;

            public string Text { get; } = text;
        }

        private List<CodeCompletionSuggestion> _initialSuggestions;
        private SourceCodePosition _startPosition;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CodeCompletionViewModel()
        {
            _initialSuggestions = [];
        }

        public ObservableCollection<SuggestionItem> CodeCompletionSuggestions { get; set; } = [];

        public SourceCodePosition CodeCompletionSuggestionStart => _startPosition;

        public void SetNewSuggestions(SourceCodePosition startPosition, IEnumerable<CodeCompletionSuggestion> suggestions)
        {
            _initialSuggestions = suggestions.ToList();
            _startPosition = startPosition;
            UpdateShownSuggestions(suggestions);
        }

        private void UpdateShownSuggestions(IEnumerable<CodeCompletionSuggestion> suggestions)
        {
            CodeCompletionSuggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                var image = (new WpfIconCache().GetIcon(suggestion.SymbolType) as WpfCanvasImage)?.Image;
                CodeCompletionSuggestions.Add(new SuggestionItem(image, suggestion.Name));
            }
        }

        public void FilterSuggestions(string textLine, int columnNumber)
        {
            textLine = textLine.Substring(_startPosition.ColumnNumber, columnNumber - _startPosition.ColumnNumber);
            string lowerTextLine = textLine.ToLower();
            IEnumerable<CodeCompletionSuggestion> filteredSuggestions = _initialSuggestions.Where(x => x.Name.ToLower().Contains(lowerTextLine));
            if (!string.IsNullOrEmpty(lowerTextLine))
            {
                filteredSuggestions = filteredSuggestions.OrderBy(x => x.Name.ToLower().StartsWith(lowerTextLine) ? 0 : 1)
                                           .ThenBy(x => x.Name);
            }
            UpdateShownSuggestions(filteredSuggestions);
        }
    }
}
