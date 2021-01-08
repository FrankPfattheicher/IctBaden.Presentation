using System;
using System.Runtime.InteropServices;

namespace IctBaden.Presentation.Framework
{
  [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
  internal sealed class AssemblyContactAttribute : Attribute
  {
    public string Address { get; set; }
    public string City { get; set; }
    public string Mail { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string Mobile { get; set; }
    public string Url { get; set; }
  }
}
