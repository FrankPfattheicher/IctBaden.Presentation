using System.Threading;
using System.Windows.Media;
using IctBaden.Presentation.Framework;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Presentation.Dialogs
{
    class AboutViewModel : ActiveViewModel
    {
        public string DialogTitle
        {
            get
            {
                var cult = Thread.CurrentThread.CurrentUICulture.ToString().Substring(0, 2);

                var format = Properties.Resources.ResourceManager.GetString("Title_" + cult);

                if (string.IsNullOrEmpty(format))
                    format = Properties.Resources.ResourceManager.GetString("Title_en");

                return string.IsNullOrEmpty(format) ? AssemblyInfo.Default.Product : string.Format(format, AssemblyInfo.Default.Product);
            }
        }

        public string NetFrameworkVersion => "TODO";// $".NET Framework {IctBaden.Framework.FrameworkVersion.DisplayText}";
        public string NetFrameworkRelease => "TODO";// $"Version {FrameworkVersion.Version} - Release {FrameworkVersion.Release}";

        public ImageSource Symbol { get; private set; }

        public AboutViewModel()
        {
            SetModel(AssemblyInfo.Default);

            var app = System.Windows.Application.Current;

            var wnd = app?.MainWindow;

            if (wnd?.Icon == null)
                return;

            Symbol = wnd.Icon;

            //var decoder = new IconBitmapDecoder(new Uri(wnd.Icon.ToString()), BitmapCreateOptions.None, BitmapCacheOption.Default);
            //var size = 0.0;
            //foreach (var frame in decoder.Frames)
            //{
            //  if(frame.Width <= size)
            //    continue;

            //  Symbol = frame;
            //  size = frame.Width;
            //}

        }


    }
}
