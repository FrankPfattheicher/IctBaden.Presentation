using System.ComponentModel;

namespace IctBaden.Presentation.TestApp
{
  public class Customer
  {
    public string FirstName { get; set; }
    public string BirthPlace { get; set; }

    [Browsable(false)]
    public string LastName { get; set; }

    public Customer()
    {
      FirstName = "Frank";
    }

    public override string ToString()
    {
      return "Customer(" + FirstName + ")";
    }
  }
}
