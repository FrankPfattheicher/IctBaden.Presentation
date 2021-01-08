using System;
using System.Runtime.InteropServices;

namespace IctBaden.Presentation.Framework
{
  [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
  internal sealed class AssemblyReleaseAttribute : Attribute
  {
    public string Date { get; set; }
  }
}