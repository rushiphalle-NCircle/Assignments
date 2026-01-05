using Assignment9.Commands.Model;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment9.Commands.ViewModel
{
    public class MyViewModel
    {
        public ObservableCollection<WallData> FilteredWalls { get; set; }
        public MyViewModel(List<WallData> walls)
        {
            FilteredWalls = new ObservableCollection<WallData>(walls);
        }
    }
}
