using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View.Winforms
{
    internal class IconCache
    {
        // TODO: Consider disposal
        private Dictionary<SymbolType, Bitmap> _icons;
        private static IconCache? instance;

        private IconCache()
        {
            _icons = new Dictionary<SymbolType, Bitmap>();
        }

        private static IconCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IconCache();
                }
                return instance;
            }
        }

        public static Bitmap? GetIcon(SymbolType symbolType) => Instance.GetIconInternal(symbolType);

        private Bitmap? GetIconInternal(SymbolType symbolType)
        {
            if (_icons.TryGetValue(symbolType, out Bitmap? cachedIcon))
            {
                return cachedIcon;
            }
            Bitmap? icon = GetIconFromResources(symbolType);
            if (icon != null)
            {
                _icons.Add(symbolType, icon);
                return icon;
            }
            return null;
        }

        private static Bitmap? GetIconFromResources(SymbolType symbolType)
        {
            Bitmap? icon = null;
            if (symbolType == SymbolType.Property)
            {
                icon = Properties.Resources.spanner;
            }
            else if (symbolType == SymbolType.Method)
            {
                icon = Properties.Resources.box;
            }
            else if (symbolType == SymbolType.Namespace)
            {
                icon = Properties.Resources.brackets;
            }
            else if (symbolType == SymbolType.Class)
            {
                icon = Properties.Resources._class;
            }
            else if (symbolType == SymbolType.Interface)
            {
                icon = Properties.Resources._interface;
            }
            else if (symbolType == SymbolType.Field)
            {
                icon = Properties.Resources.field;
            }
            else if (symbolType == SymbolType.Local)
            {
                icon = Properties.Resources.local;
            }
            else if (symbolType == SymbolType.Struct)
            {
                icon = Properties.Resources._struct;
            }
            else if (symbolType == SymbolType.EnumMember)
            {
                icon = Properties.Resources.enumMember;
            }
            else if (symbolType == SymbolType.Constant)
            {
                icon = Properties.Resources.constant;
            }
            return icon;
        }
    }
}
