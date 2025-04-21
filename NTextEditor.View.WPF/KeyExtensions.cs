using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NTextEditor.View.WPF
{
    internal static class KeyExtensions
    {
        public static NTextEditorKey ToTextEditorKey(this Key key)
        {
            return key switch
            {
                Key.A => NTextEditorKey.A,
                Key.B => NTextEditorKey.B,
                Key.C => NTextEditorKey.C,
                Key.D => NTextEditorKey.D,
                Key.E => NTextEditorKey.E,
                Key.F => NTextEditorKey.F,
                Key.G => NTextEditorKey.G,
                Key.H => NTextEditorKey.H,
                Key.I => NTextEditorKey.I,
                Key.J => NTextEditorKey.J,
                Key.K => NTextEditorKey.K,
                Key.L => NTextEditorKey.L,
                Key.M => NTextEditorKey.M,
                Key.N => NTextEditorKey.N,
                Key.O => NTextEditorKey.O,
                Key.P => NTextEditorKey.P,
                Key.Q => NTextEditorKey.Q,
                Key.R => NTextEditorKey.R,
                Key.S => NTextEditorKey.S,
                Key.T => NTextEditorKey.T,
                Key.U => NTextEditorKey.U,
                Key.V => NTextEditorKey.V,
                Key.W => NTextEditorKey.W,
                Key.X => NTextEditorKey.X,
                Key.Y => NTextEditorKey.Y,
                Key.Z => NTextEditorKey.Z,
                Key.Left => NTextEditorKey.Left,
                Key.Right => NTextEditorKey.Right,
                Key.Up => NTextEditorKey.Up,
                Key.Down => NTextEditorKey.Down,
                Key.Home => NTextEditorKey.Home,
                Key.End => NTextEditorKey.End,
                Key.Back => NTextEditorKey.Back,
                Key.Delete => NTextEditorKey.Delete,
                _ => NTextEditorKey.None
            };
        }
    }
}
