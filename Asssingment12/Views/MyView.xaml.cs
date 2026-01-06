
// Assignment12/View/MyView.xaml.cs
using System.Windows;

using Assignment12.ViewModel;

namespace Assignment12.View
{
    public partial class MyView : Window
    {
        public MyView()
        {
            InitializeComponent();
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MyViewModel vm && e?.NewValue != null)
            {
                vm.SelectedItem = e.NewValue;
            }
        }
    }
}
