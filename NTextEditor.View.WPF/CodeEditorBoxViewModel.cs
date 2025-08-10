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
using System.Windows.Media.Imaging;

namespace NTextEditor.View.WPF
{
    internal class CodeEditorBoxViewModel : INotifyPropertyChanged
    {
        private string _lineAndColumnNumberText;
        private int _verticalScrollMax;
        private int _horizontalScrollMax;
        private int _verticalScrollValue;
        private int _horizontalScrollValue;
        private bool _isCodeCompletePopupShown;
        private Rect _codeCompletePopupRect;

        private readonly ViewManager _viewManager;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CodeEditorBoxViewModel(ViewManager viewManager)
        {
            _viewManager = viewManager;
            _lineAndColumnNumberText = _viewManager.GetLineAndCharacterLabel();

            UpdateScrollBarMaxima();
        }

        public bool IsCodeCompletePopupShown
        {
            get => _isCodeCompletePopupShown;
            set
            {
                _isCodeCompletePopupShown = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCodeCompletePopupShown)));
            }
        }

        public Rect CodeCompletePopupRect
        {
            get => _codeCompletePopupRect;
            set
            {
                _codeCompletePopupRect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CodeCompletePopupRect)));
            }
        }

        public string LineAndColumnNumberText
        {
            get => _lineAndColumnNumberText;
            set
            {
                _lineAndColumnNumberText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineAndColumnNumberText)));
            }
        }

        public int VerticalScrollValue
        {
            get => _verticalScrollValue;
            set
            {
                if (_verticalScrollValue != value)
                {
                    _verticalScrollValue = value;
                    if (_verticalScrollMax == 0)
                    {
                        _viewManager.VerticalScrollPositionPX = 0;
                    }
                    else
                    {
                        _viewManager.VerticalScrollPositionPX = (_verticalScrollValue * _viewManager.GetMaxVerticalScrollPosition()) / _verticalScrollMax;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerticalScrollValue)));
                }
            }
        }

        public int VerticalScrollMax
        {
            get => _verticalScrollMax;
            set
            {
                _verticalScrollMax = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerticalScrollMax)));
            }
        }

        public int HorizontalScrollValue
        {
            get => _horizontalScrollValue;
            set
            {
                if (_horizontalScrollValue != value)
                {
                    _horizontalScrollValue = value;
                    _viewManager.HorizontalScrollPositionPX = (_horizontalScrollValue * _viewManager.GetMaxHorizontalScrollPosition()) / _horizontalScrollMax;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HorizontalScrollValue)));
                }
            }
        }

        public int HorizontalScrollMax
        {
            get => _horizontalScrollMax;
            set
            {
                _horizontalScrollMax = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HorizontalScrollMax)));
            }
        }

        public void UpdateScrollBarMaxima()
        {
            VerticalScrollMax = _viewManager.GetMaxVerticalScrollPosition() / _viewManager.LineWidth;
            HorizontalScrollMax = _viewManager.GetMaxHorizontalScrollPosition();
        }
    }
}
