using System;
using System.ComponentModel;
using System.Windows;

namespace IctBaden.Presentation
{
    public class ActiveDialog<TDlgWindow>
        where TDlgWindow : Window, new()
    {
        private Window _dlg;
        public ActiveDialogViewModel ViewModel;

        public Window Owner
        {
            get => _dlg.Owner;
            set => _dlg.Owner = value;
        }

        public ActiveDialog(ActiveDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _dlg = new TDlgWindow {Owner = Application.Current.MainWindow};
                _dlg.Closed += DlgOnClosed;
                _dlg.Loaded += OnLoaded;
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _dlg.DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "DialogResult")
            {
                _dlg.DialogResult = ViewModel.DialogResult;
            }
        }

        private void DlgOnClosed(object sender, EventArgs eventArgs)
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _dlg.Closed -= DlgOnClosed;
        }

        public bool? ShowDialog()
        {
            return _dlg.ShowDialog();
        }
    }
}