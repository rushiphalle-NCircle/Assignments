
using System;
using System.Windows;

using Assignment3S.Commands.ViewModel;

namespace Assignment3S.Commands
{
    public partial class FloorPlanSelector : Window
    {
        public FloorPlanSelector()
        {
            InitializeComponent();
        }

        public void Bind(MyViewModel vm)
        {
            DataContext = vm;
            vm.RequestClose += (_, __) => { DialogResult = true; Close(); };
        }
    }
}
