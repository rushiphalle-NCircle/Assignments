
using System.Windows;
using System.Windows.Controls;

using Assignment8.Commands.Model;
using Assignment8.Commands.ViewModel;

namespace Assignment8.Commands.Views
{
    public partial class TreeViewWindow : Window
    {
        public TreeViewWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as TreeNode;
            var vm = DataContext as TreeViewViewModel;
            vm?.ActivateView(node);
        }
    }
}
