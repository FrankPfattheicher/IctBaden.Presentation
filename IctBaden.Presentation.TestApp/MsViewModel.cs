using System.ComponentModel;

namespace IctBaden.Presentation.TestApp
{
  public class MsViewModel : INotifyPropertyChanged
  {
    private readonly Customer _customer;

    public MsViewModel(Customer customer)
    {
      _customer = customer;
    }

    public string FirstName
    {
      get { return _customer.FirstName; }
      set
      {
        if (value == _customer.FirstName)
          return;
        _customer.FirstName = value;
        NotifyPropertyChanged("FirstName");
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged(string name)
    {
      var handler = PropertyChanged;
      if (handler == null)
        return;
      handler(this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
