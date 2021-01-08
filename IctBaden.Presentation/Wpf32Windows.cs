using System;
using System.Windows.Interop;

namespace IctBaden.Presentation
{
    /// <inheritdoc />
    /// <summary>
    /// Bietet ein IWin32Window Interface für WPF Fenster.
    /// Wird benötigt um einen Owner an WinForms Dialoge zu übergeben.
    /// </summary>
    public class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IntPtr Handle { get; private set; }

        public Wpf32Window(System.Windows.Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }
    }
}
