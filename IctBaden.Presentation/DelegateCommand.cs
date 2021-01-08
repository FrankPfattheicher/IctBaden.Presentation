using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;

namespace IctBaden.Presentation
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        private readonly Dispatcher _dispatcher;

        // ReSharper disable once UnusedMember.Global
        public DelegateCommand(Action<object> execute)
            : this(null, execute, null)
        {
        }

        public DelegateCommand(Dispatcher dispatcher, Action<object> execute, Predicate<object> canExecute)
        {
            _dispatcher = dispatcher;
            Debug.Assert(execute != null);
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            var handler = _canExecute;
            return (handler == null) || handler(parameter);
        }

        public event EventHandler CanExecuteChanged;

        private void ExecuteHandler(EventHandler handler)
        {
            var dispatcherObject = handler.Target as DispatcherObject;
            var args = new EventArgs();
            // If the subscriber is a DispatcherObject and different thread
            if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
            {
                // Invoke handler in the target dispatcher's thread
                dispatcherObject.Dispatcher.BeginInvoke(handler, DispatcherPriority.DataBind, this, args);
            }
            else if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(handler, DispatcherPriority.DataBind, this, args);
            }
            else // Execute handler as is
            {
                handler(this, args);
            }
        }

        public void SignalCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler == null)
                return;
            ExecuteHandler(handler);
        }

        public void Execute(object parameter)
        {
            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
