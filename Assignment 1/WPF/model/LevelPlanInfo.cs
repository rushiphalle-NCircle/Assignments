
// Models/LevelPlanInfo.cs
using Autodesk.Revit.DB;

namespace Asignment1.Commands.Models
{
    public class LevelPlanInfo
    {
        public int LevelId { get; set; }          // for display (IntegerValue)
        public string LevelName { get; set; }
        public double ElevationFeet { get; set; } // internal units in feet
        public string ViewName { get; set; }      // floor plan name
    }
}
