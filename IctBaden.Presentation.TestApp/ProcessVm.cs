using System.Threading;
using System.Windows.Markup;

namespace IctBaden.Presentation.TestApp
{
    public class ProcessVm : ActiveViewModel
    {
        public int Progress { get; set; }
        [DependsOn(nameof(Progress))]
        public bool Done => Progress > 100;

        // ReSharper disable once NotAccessedField.Local
        private Timer run;

        [ActionMethod]
        public void StartProcessing()
        {
            run = new Timer(_ => { this[nameof(Progress)] = Progress + 1; }, null, 200, 200);
        }
    }
}