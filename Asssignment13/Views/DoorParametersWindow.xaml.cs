
// File: DoorParametersWindow.xaml.cs
using System;
using System.Windows;

using Autodesk.Revit.UI;

namespace Assignment13.Commands
{
    public partial class DoorParametersWindow : Window
    {
        public DoorParametersWindow()
        {
            InitializeComponent();
        }

        public void BindVM(DoorParametersViewModel vm, IntPtr ownerHandle)
        {
            DataContext = vm;

            // Ensure modal ownership to Revit
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            helper.Owner = ownerHandle;

            // Wire VM events to WPF
            vm.CloseRequested += () => Dispatcher.Invoke(Close);
            vm.ShowInfoMessage += msg => MessageBox.Show(this, msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            vm.ShowWarningMessage += msg => MessageBox.Show(this, msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            vm.ShowErrorMessage += msg => MessageBox.Show(this, msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}