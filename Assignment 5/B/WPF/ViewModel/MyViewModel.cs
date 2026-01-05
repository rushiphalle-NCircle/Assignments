using Assignment5B.Commands.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5B.Commands.ViewModel
{
    public class MyViewModel
    {
        public List<MyModel> Items { get; private set; }
        public MyViewModel(List<MyModel> Items)
        {
            this.Items = Items;
        }
    }
}
