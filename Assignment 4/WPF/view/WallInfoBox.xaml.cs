
using Assignments4.Commands.model;
using Assignments4.Commands.viewModel;

using System.Collections.Generic;
using System.Windows;

namespace Assignments4.Commands.View
{
    public partial class WallInfoBox : Window
    {
        private MyViewModel _vm;

        public WallInfoBox(List<WallInfo> list)
        {
            InitializeComponent();
            _vm = new MyViewModel(list);
            DataContext = _vm;
        }
    }
}
