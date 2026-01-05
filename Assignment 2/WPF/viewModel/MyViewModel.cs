using Assignment2.Commands.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Assignment2.Commands.ViewModel
{
    public class MyViewModel
    {
           public List<MyModel> Items { get; set; }
           public MyModel SelectedItem = new MyModel() { Id= 0, Name= "Select Item"};
    }
}
