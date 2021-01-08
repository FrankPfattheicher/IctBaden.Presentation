using System;
using System.Linq;

namespace IctBaden.Presentation.Framework
{
    /// <summary>
    /// Ermöglicht die einfache Konvertierung zwischen enum- / string- / und numerischer Darstellung
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class ValidatedEnum<TEnum> where TEnum : struct
    {
        /// <summary>
        /// Zeigt an, ob der angegebene Wert gültig ist
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// Zeigt an, ob ein Wert vorhanden ist (nicht zwingend valid)
        /// </summary>
        public readonly bool HasValue;

        /// <summary>
        /// enum-Wert
        /// </summary>
        public readonly TEnum Enumeration;

        /// <summary>
        /// Numerischer Wert
        /// </summary>
        public readonly long Numeric;

        /// <summary>
        /// String-Repräsentation
        /// </summary>
        public readonly string Text;


        /// <summary>
        /// Constructor with automatic type inference
        /// </summary>
        /// <param name="data"></param>
        public ValidatedEnum(object data) : this(data, false)
        {
        }

        /// <summary>
        /// Constructor with automatic type inference and optional case handling
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreCase">Ignore case comparing enum values</param>
        public ValidatedEnum(object data, bool ignoreCase)
        {
            HasValue = false;

            switch (data)
            {
                case null:
                    IsValid = false;
                    return;
                case int intData:
                    data = (long)intData;
                    break;
            }

            if (long.TryParse(data.ToString(), out Numeric))
            {
                data = Numeric;
            }

            if (data is long longData)
            {
                HasValue = true;
                Numeric = longData;
                Text = Enum.GetNames(typeof(TEnum)).FirstOrDefault(name => Convert.ToInt32(Enum.Parse(typeof(TEnum), name)) == Numeric);
                if (Text != null)
                {
                    Enumeration = (TEnum)Enum.Parse(typeof(TEnum), Text);
                    IsValid = true;
                    return;
                }
            }

            Text = data.ToString();
            HasValue = !string.IsNullOrEmpty(Text);
            Text = Enum.GetNames(typeof(TEnum)).FirstOrDefault(name => string.Equals(name, Text, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
            if (Enum.TryParse(Text, out Enumeration))
            {
                Numeric = Convert.ToInt32(Enumeration);
                IsValid = true;
                return;
            }

            IsValid = false;
        }

    }
}
