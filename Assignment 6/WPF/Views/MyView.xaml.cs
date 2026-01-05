
// File: Views/MyView.xaml.cs
using Assignment6.Commands.ViewModel;

using System.Windows;

namespace Assignment6.Commands.Views
{
    public partial class MyView : Window
    {
        public MyView(MyViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}