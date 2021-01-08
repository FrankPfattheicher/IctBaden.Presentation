using System.Diagnostics;
using System.Windows;

namespace IctBaden.Presentation.Dialogs
{
  /// <summary>
  /// Common About Dialog
  /// SystemMenu.AppendEntries(new List(SystemMenuEntry)
  /// {
  ///   SystemMenuEntry.Separator,
  ///   new SystemMenuEntry("Info über...", AboutDialog.ShowAboutDialog),
  /// });
  /// </summary>
  public partial class AboutDialog
    {
        public AboutDialog()
        {
            if (Owner == null)
            {
                var app = Application.Current;
                if (app != null)
                {
                    var wnd = app.MainWindow;
                    if (wnd != null)
                        Owner = wnd;
                }
            }

            InitializeComponent();
        }

        public static void ShowAboutDialog()
        {
            var dlg = new AboutDialog();
            dlg.ShowDialog();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HyperlinkRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
            Close();
        }
    }
}
