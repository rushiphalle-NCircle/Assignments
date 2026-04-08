
using Autodesk.Revit.DB;

namespace Assignment10.Commands.Model
{
    public class OpeningInfo
    {
        public string IdText { get; set; }
        public string TypeName { get; set; }
        public string CategoryName { get; set; }
        public string LevelName { get; set; }
        public double? WidthFt { get; set; }
        public double? HeightFt { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            string size = (WidthFt.HasValue || HeightFt.HasValue)
                ? $" [{(WidthFt.HasValue ? $"{WidthFt:0}" : "-")} x {(HeightFt.HasValue ? $"{HeightFt:0}" : "-")} mm]"
                : "";
            string lvl = string.IsNullOrEmpty(LevelName) ? "" : $" (Level: {LevelName})";
            string notes = string.IsNullOrEmpty(Notes) ? "" : $" - {Notes}";
            return $"{CategoryName}: {TypeName} (Id {IdText}){size}{lvl}{notes}";
        }
    }
}
