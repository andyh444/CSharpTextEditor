﻿using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View
{
    public interface IToolTipSource
    {
        (string toolTip, List<SyntaxHighlighting> highlightings) GetToolTip();
    }
}
