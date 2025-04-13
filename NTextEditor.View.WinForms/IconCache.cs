using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTextEditor.View.WinForms.Properties;

namespace NTextEditor.View.Winforms
{
    internal class IconCache : IIconCache
    {
        // TODO: Consider disposal
        private Dictionary<SymbolType, Bitmap> _icons;
        private static IconCache? instance;

        private IconCache()
        {
            _icons = new Dictionary<SymbolType, Bitmap>();
        }

        public static IconCache Instance
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

        public ICanvasImage? GetIcon(SymbolType symbolType)
        {
            var icon = Instance.GetIconInternal(symbolType);
            if (icon != null)
            {
                return new WinformsCanvasImage(icon);
            }
            return null;
        }

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
                icon = Resources.spanner;
            }
            else if (symbolType == SymbolType.Method)
            {
                icon = Resources.box;
            }
            else if (symbolType == SymbolType.Namespace)
            {
                icon = Resources.brackets;
            }
            else if (symbolType == SymbolType.Class)
            {
                icon = Resources._class;
            }
            else if (symbolType == SymbolType.Interface)
            {
                icon = Resources._interface;
            }
            else if (symbolType == SymbolType.Field)
            {
                icon = Resources.field;
            }
            else if (symbolType == SymbolType.Local)
            {
                icon = Resources.local;
            }
            else if (symbolType == SymbolType.Struct)
            {
                icon = Resources._struct;
            }
            else if (symbolType == SymbolType.EnumMember)
            {
                icon = Resources.enumMember;
            }
            else if (symbolType == SymbolType.Constant)
            {
                icon = Resources.constant;
            }
            return icon;
        }
    }
}
