using System;

namespace IctBaden.Presentation.Menus
{
    public class SystemMenuEntry
    {
        public string Text { get; private set; }
        public Action Command { get; private set; }

        public SystemMenuEntry(string text, Action command)
        {
            Text = text;
            Command = command;
        }

        public static SystemMenuEntry Separator
        {
            get
            {
                return new SystemMenuEntry("---", () => { });
            }
        }
    }
}