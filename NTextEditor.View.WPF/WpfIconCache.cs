using NTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NTextEditor.View.WPF
{
    internal class WpfCanvasImage : ICanvasImage
    {
        public int Width => Image.PixelWidth;

        public int Height => Image.PixelHeight;

        public BitmapImage Image { get; }

        public WpfCanvasImage(BitmapImage image)
        {
            Image = image;
        }

        public void DrawToCanvas(ICanvas canvas, Point point)
        {
            throw new NotImplementedException();
        }
    }

    internal class WpfIconCache : IIconCache
    {
        public ICanvasImage? GetIcon(SymbolType symbolType)
        {
            // TODO: Cache
            BitmapImage? bitmapImage = GetIconFromResources(symbolType);
            if (bitmapImage == null)
            {
                return null;
            }
            return new WpfCanvasImage(bitmapImage);
        }

        private static BitmapImage? GetIconFromResources(SymbolType symbolType)
        {
            string? imageName = null;
            if (symbolType == SymbolType.Property)
            {
                imageName = "spanner.png";
            }
            else if (symbolType == SymbolType.Method)
            {
                imageName = "box.png";
            }
            else if (symbolType == SymbolType.Namespace)
            {
                imageName = "brackets.png";
            }
            else if (symbolType == SymbolType.Class)
            {
                imageName = "class.png";
            }
            else if (symbolType == SymbolType.Interface)
            {
                imageName = "interface.png";
            }
            else if (symbolType == SymbolType.Field)
            {
                imageName = "field.png";
            }
            else if (symbolType == SymbolType.Local)
            {
                imageName = "local.png";
            }
            else if (symbolType == SymbolType.Struct)
            {
                imageName = "struct.png";
            }
            else if (symbolType == SymbolType.EnumMember)
            {
                imageName = "enumMember.png";
            }
            else if (symbolType == SymbolType.Constant)
            {
                imageName = "constant.png";
            }
            if (imageName == null)
            {
                return null;
            }
            string? assembly = Assembly.GetExecutingAssembly().GetName().Name;
            if (assembly == null)
            {
                return null;
            }
            Uri uri = new Uri($"pack://application:,,,/{assembly};component/Resources/{imageName}");

            return new BitmapImage(uri);
        }
    }
}
