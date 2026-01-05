
using Autodesk.Revit.DB;

namespace Assignment10.Commands.Model
{
    public class OpeningInfo
    {
        public string IdText { get; set; }
        public string TypeName { get; set; }
        public string CategoryName { get; set; }
        public string LevelName { get; set; }
        public double? WidthMm { get; set; }
        public double? HeightMm { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            string size = (WidthMm.HasValue || HeightMm.HasValue)
                ? $" [{(WidthMm.HasValue ? $"{WidthMm:0}" : "-")} x {(HeightMm.HasValue ? $"{HeightMm:0}" : "-")} mm]"
                : "";
            string lvl = string.IsNullOrEmpty(LevelName) ? "" : $" (Level: {LevelName})";
            string notes = string.IsNullOrEmpty(Notes) ? "" : $" - {Notes}";
            return $"{CategoryName}: {TypeName} (Id {IdText}){size}{lvl}{notes}";
        }
    }
}
