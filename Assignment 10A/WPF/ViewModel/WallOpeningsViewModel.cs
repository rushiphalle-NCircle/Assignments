
using System.Collections.ObjectModel;
using System.Text;
using Autodesk.Revit.DB;
using Assignment10.Commands.Model;

namespace Assignment10.Commands.ViewModel
{
    public class WallOpeningsViewModel
    {
        public string WallName { get; }
        public string WallLevelName { get; }
        public ObservableCollection<OpeningInfo> Openings { get; }

        public WallOpeningsViewModel(Wall wall, System.Collections.Generic.IList<OpeningInfo> openings)
        {
            WallName = wall.Name;

            // Try get base level name from constraint
            string levelName = null;
            var p = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
            if (p != null)
            {
                var lvlId = p.AsElementId();
                if (lvlId != ElementId.InvalidElementId)
                {
                    var lvl = wall.Document.GetElement(lvlId) as Level;
                    if (lvl != null) levelName = lvl.Name;
                }
            }
            WallLevelName = levelName;

            Openings = new ObservableCollection<OpeningInfo>(openings);
        }

        public string AsSummaryText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Total openings: " + Openings.Count);
            if (Openings.Count == 0)
            {
                sb.AppendLine("No openings found.");
                return sb.ToString();
            }

            sb.AppendLine("-----");
            foreach (var o in Openings)
            {
                sb.AppendLine(o.ToString());
            }

            return sb.ToString();
        }
    }
}
