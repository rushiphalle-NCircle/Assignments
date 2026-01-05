using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Assignment5A.Commands.ViewModel;

namespace Assignment5A.Commands.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MyView : Window
    {
        public MyView(MyViewModel vm)
        {
            InitializeComponent();
            vm.closeEvent += OnClose;
            DataContext = vm;
        }

        public void OnClose()
        {
            Close();
        }
    }
}

