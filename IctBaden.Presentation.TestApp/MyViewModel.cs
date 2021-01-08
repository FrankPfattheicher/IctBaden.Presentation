using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace IctBaden.Presentation.TestApp
{
    class MyViewModel : ActiveViewModel
    {
        private string wait;
        public string Wait
        {
            get { return wait; }
            set { wait = value; NotifyPropertyChanged("Wait"); }
        }

        public string MyProp { get; set; }
        public Customer Kunde { get; set; }

        [DependsOn("FirstName")]
        public int LenOfFirstName => this["FirstName"].ToString().Length;

        public MyViewModel()
        {
            MyProp = "super";
            Wait = "warten";
            this["Test"] = "kuckuck";
            this["Toast"] = new { Scheiben = 1, Getoastet = false };

            Kunde = new Customer();
            SetModel(new Customer());
            SetModel("Neu_", new Customer());

            var person = new Customer { FirstName = "Person" };
            SetModel("Person", person);

            var timer = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .ObserveOn(SynchronizationContext.Current);
            timer.Subscribe(_ => Wait += ".");
        }


        [ActionMethod(Shortcut = "F1")]
        public void OnAbout()
        {
            MessageBox.Show("Hallo");
        }
        [ActionMethod(Shortcut = "F1", ExecuteAsync = true)]
        public void OnAsyncAbout()
        {
            MessageBox.Show("Hallo");
        }

        [ActionMethod(Shortcut = "Ctrl+N")]
        public void OnNewModel()
        {
            var newModel = new Customer { FirstName = "neu!", LastName = "auch neu" };
            SetModel(newModel);
        }

        [ActionMethod(Shortcut = "Ctrl+X")]
        public void OnClearModel()
        {
            SetModel(new Customer());
        }

        [ActionMethod]
        public void OnSetName(object name)
        {
            SetModel(new Customer() { FirstName = name.ToString() });
        }
        [ActionMethod(ExecuteAsync = true)]
        public void OnAsyncSetName(object name)
        {
            Thread.Sleep(1000);
            SetModel(new Customer() { FirstName = "Name" + DateTime.Now.Second });
        }

        [ActionMethod]
        public void StartProcess()
        {
            var dlg = new ProcessDlg();
            dlg.ShowDialog();
        }

        [ActionMethod]
        public void EditName()
        {
            var vm = new EditNameVm {Name = Kunde.FirstName};
            var dlg = new ActiveDialog<EditNameDlg>(vm);
            if (dlg.ShowDialog() == true)
            {
                Kunde.FirstName = vm.Name;
                NotifyPropertyChanged(nameof(Kunde));
            }
        }

    }
}

