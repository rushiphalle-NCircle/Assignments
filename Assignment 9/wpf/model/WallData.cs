using Autodesk.Revit.DB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment9.Commands.Model
{
    public class WallData
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
        public ElementId LevelId { get; set; }
        public string LevelName { get; set; }
        public double Elivation { get; set; }
    }
}
