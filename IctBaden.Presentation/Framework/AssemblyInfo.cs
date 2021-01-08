using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace IctBaden.Presentation.Framework
{
    internal class AssemblyInfo
    {
        public static Assembly DefaultAssembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        public static readonly AssemblyInfo Default = new AssemblyInfo();

        public static string CompanyPath = "ICT Baden";
        private readonly Assembly _assembly;
        private readonly AssemblyContactAttribute _contact;

        public AssemblyInfo() : this(DefaultAssembly)
        { }
        public AssemblyInfo(Assembly infoAssembly)
        {
            _assembly = infoAssembly;
            if ((_assembly.GetCustomAttributes(typeof(AssemblyContactAttribute), true) is AssemblyContactAttribute[] contacts) && (contacts.Length > 0))
                _contact = contacts[0];
            else
                _contact = new AssemblyContactAttribute();
        }

        public string Version => _assembly.GetName().Version.ToString();

        public string DisplayVersion
        {
            get
            {
                var raw = Version.Split('.');
                if (raw.Length != 4)
                {
                    return Version;
                }
                var display = new StringBuilder();
                display.Append(raw[0]);
                display.Append(".");
                display.Append($"{int.Parse(raw[1]):D2}");
                display.Append(".");
                display.Append($"{int.Parse(raw[2]):D3}");
                var code = int.Parse(raw[3]);
                if (code > 0)
                    display.Append(char.ConvertFromUtf32('a' - 1 + code));
                return display.ToString();
            }
        }

        public string ExeBaseName => Path.GetFileNameWithoutExtension(_assembly.Location);

        public string ExePath => Path.GetDirectoryName(_assembly.Location);

        private string GetPath(string name)
        {
            var path = ExePath;

            path = Path.GetFileName(path) == CompanyPath
                ? Path.Combine(path, name)
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Path.Combine(CompanyPath, name));

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (UnauthorizedAccessException)
                {
                    // ignore this
                }
            }
            return path;
        }
        public string DataPath => GetPath("Data");

        public string CommonPath => GetPath("Common");

        public string SettingsFileName => Path.ChangeExtension(Path.Combine(DataPath, ExeBaseName), "cfg");
        public string LocalSettingsFileName => Path.ChangeExtension(Path.Combine(ExePath, ExeBaseName), "cfg");

        public DateTime FileTime => File.GetLastWriteTime(_assembly.Location);

        public string Title => (_assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true) is AssemblyTitleAttribute[] title) 
            ? title[0].Title 
            : ExeBaseName;

        public string Description => (_assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true) is AssemblyDescriptionAttribute[] description) 
            ? description[0].Description 
            : string.Empty;

        public string Configuration => (_assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), true) is AssemblyConfigurationAttribute[] configuration) 
            ? configuration[0].Configuration 
            : string.Empty;

        public string CompanyName => (_assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true) is AssemblyCompanyAttribute[] company) 
            ? company[0].Company 
            : string.Empty;

        public string CompanyAddress => _contact.Address;
        public string CompanyCity => _contact.City;
        public string CompanyMail => _contact.Mail;
        public string CompanyPhone => _contact.Phone;
        public string CompanyFax => _contact.Fax;
        public string CompanyMobile => _contact.Mobile;
        public string CompanyUrl => _contact.Url;
        public string Product => (_assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true) is AssemblyProductAttribute[] product) 
            ? product[0].Product 
            : string.Empty;

        public string Copyright => (_assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true) is AssemblyCopyrightAttribute[] copyright) 
            ? copyright[0].Copyright 
            : string.Empty;

        public string Trademark => (_assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), true) is AssemblyTrademarkAttribute[] trademark) 
            ? trademark[0].Trademark 
            : string.Empty;

        public string Culture => ((_assembly.GetCustomAttributes(typeof(AssemblyCultureAttribute), true) is AssemblyCultureAttribute[] attributes) && (attributes.Length > 0)) 
            ? attributes[0].Culture 
            : string.Empty;

        public string NeutralResourcesLanguage => ((_assembly.GetCustomAttributes(typeof(NeutralResourcesLanguageAttribute), true) is NeutralResourcesLanguageAttribute[] attributes) && (attributes.Length > 0)) 
            ? attributes[0].CultureName 
            : string.Empty;
    }
}
