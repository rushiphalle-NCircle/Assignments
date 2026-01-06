using System.Collections.Generic;
using System.Windows;
using Asignment1.Commands.Models;
using Asignment1.Commands.ViewModel;

namespace Asignment1.Commands.Views
{
    public partial class LevelPlanWindow : Window
    {
        public LevelPlanWindow(List<LevelPlanInfo> items)
        {
            InitializeComponent();
            DataContext = new MyViewModel { Items = items };
        }
    }
}
