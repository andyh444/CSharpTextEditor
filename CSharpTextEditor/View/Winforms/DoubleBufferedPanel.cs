﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor.View.Winforms
{
    internal class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
            : base()
        {
            DoubleBuffered = true;
        }

        public void ToggleDoubleBuffer(bool on)
        {
            DoubleBuffered = on;
        }
    }
}
