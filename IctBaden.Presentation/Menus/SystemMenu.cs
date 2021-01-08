using IctBaden.Presentation.Dialogs;
using IctBaden.Presentation.Framework;

namespace IctBaden.Presentation.Menus
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    public class SystemMenu
    {
        #region PInvoke
        // ReSharper disable InconsistentNaming

        //private const int WM_CREATE = 0x0001;
        // ReSharper disable IdentifierTypo
        private const int WM_SYSCOMMAND = 0x0112;

        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;
        // ReSharper restore IdentifierTypo

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        // ReSharper restore InconsistentNaming
        #endregion

        // ID for the About item on the system menu
        private readonly List<SystemMenuEntry> _menuEntries;
        private const int StartId = 10;

        public SystemMenu(List<SystemMenuEntry> appendMenuEntries)
            : this(HwndSource.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow ?? throw new InvalidOperationException()).Handle), appendMenuEntries)
        {
        }

        public SystemMenu(HwndSource mainWindowHandle, List<SystemMenuEntry> appendMenuEntries)
        {
            // Get a handle to a copy of this form's system (window) menu
            var hSysMenu = GetSystemMenu(mainWindowHandle.Handle, false);

            _menuEntries = appendMenuEntries;
            var id = StartId;
            foreach (var menuEntry in appendMenuEntries)
            {
                if (menuEntry.Text.StartsWith("-"))
                {
                    AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);
                }
                else
                {
                    AppendMenu(hSysMenu, MF_STRING, id, menuEntry.Text);
                }
                id++;
            }
            mainWindowHandle.AddHook(HwndSourceHook);
        }

        public static void AppendDefaultAboutMenuEntry()
        {
            AppendDefaultAboutMenuEntry(AssemblyInfo.Default.Title);
        }
        public static void AppendDefaultAboutMenuEntry(string name)
        {
            AppendEntries(new List<SystemMenuEntry>
            {
                new SystemMenuEntry("-", null),
                new SystemMenuEntry($"About {name}...", AboutDialog.ShowAboutDialog)
            });
        }

        public static void AppendEntries(List<SystemMenuEntry> appendMenuEntries)
        {
            // ReSharper disable once UnusedVariable
            var _ = new SystemMenu(appendMenuEntries);
        }

        // ReSharper disable once IdentifierTypo
        private IntPtr HwndSourceHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_SYSCOMMAND) return IntPtr.Zero;

            var id = StartId;
            foreach (var systemMenuEntry in _menuEntries)
            {
                if ((int)wParam == id)
                {
                    systemMenuEntry.Command();
                }
                id++;
            }
            return IntPtr.Zero;
        }
    }
}