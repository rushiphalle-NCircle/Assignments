
using System.Windows;

using FamilyLoader.ViewModels;

namespace FamilyLoader.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // If DataContext was not set by the command, create a blank VM (not typical here)
            if (DataContext is MainViewModel vm)
            {
                vm.RequestClose = () => this.Close();
            }
        }
    }
}
