namespace IctBaden.Presentation.TestApp
{
    public class EditNameVm : ActiveDialogViewModel
    {
        public string Name { get; set; }

        [ActionMethod]
        public void OnOk()
        {
            DialogResult = true;
        }
    }
}