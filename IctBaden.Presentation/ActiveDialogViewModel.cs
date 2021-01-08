namespace IctBaden.Presentation
{
    public class ActiveDialogViewModel : ActiveViewModel
    {
        private bool? dialogResult;

        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                NotifyPropertyChanged(nameof(DialogResult));
            }
        }
    }
}