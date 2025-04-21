using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View.WinForms
{
    internal static class KeysExtensions
    {
        public static NTextEditorKey ToTextEditorKey(this Keys keys)
        {
            return keys switch
            {
                Keys.A => NTextEditorKey.A,
                Keys.B => NTextEditorKey.B,
                Keys.C => NTextEditorKey.C,
                Keys.D => NTextEditorKey.D,
                Keys.E => NTextEditorKey.E,
                Keys.F => NTextEditorKey.F,
                Keys.G => NTextEditorKey.G,
                Keys.H => NTextEditorKey.H,
                Keys.I => NTextEditorKey.I,
                Keys.J => NTextEditorKey.J,
                Keys.K => NTextEditorKey.K,
                Keys.L => NTextEditorKey.L,
                Keys.M => NTextEditorKey.M,
                Keys.N => NTextEditorKey.N,
                Keys.O => NTextEditorKey.O,
                Keys.P => NTextEditorKey.P,
                Keys.Q => NTextEditorKey.Q,
                Keys.R => NTextEditorKey.R,
                Keys.S => NTextEditorKey.S,
                Keys.T => NTextEditorKey.T,
                Keys.U => NTextEditorKey.U,
                Keys.V => NTextEditorKey.V,
                Keys.W => NTextEditorKey.W,
                Keys.X => NTextEditorKey.X,
                Keys.Y => NTextEditorKey.Y,
                Keys.Z => NTextEditorKey.Z,
                Keys.Left => NTextEditorKey.Left,
                Keys.Right => NTextEditorKey.Right,
                Keys.Up => NTextEditorKey.Up,
                Keys.Down => NTextEditorKey.Down,
                Keys.Home => NTextEditorKey.Home,
                Keys.End => NTextEditorKey.End,
                Keys.Back => NTextEditorKey.Back,
                Keys.Delete => NTextEditorKey.Delete,
                _ => NTextEditorKey.None
            };
        }
    }
}
