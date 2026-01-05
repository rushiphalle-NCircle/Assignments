
// File: Views/MyView2.xaml.cs
using System.Windows;

namespace Assignment6.Commands.Views
{
    public partial class MyView2 : Window
    {
        public MyView2(object viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
