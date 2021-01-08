using System;

namespace IctBaden.Presentation
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionMethodAttribute : Attribute
    {
        public string Name { get; set; }
        public string EnabledPredicate { get; set; }
        /// <summary>
        /// see KeyGestureConverter
        /// </summary>
        public string Shortcut { get; set; }

        public string Trigger { get; set; }

        public bool ExecuteAsync { get; set; }

        //public bool AppGlobal { get; set; }
        //public Predicate<object> EnabledPredicate;
    }
}