
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace Assignment10.Commands.Model
{
    public static class OpeningExtractor
    {
        public static IList<OpeningInfo> GetOpenings(Document doc, Wall wall)
        {
            var result = new List<OpeningInfo>();

            // Doors + Windows hosted on the selected wall
            var hostedFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(fi => fi.Host is Wall && fi.Host.Id == wall.Id)
                .Where(fi => fi.Category != null &&
                    (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors ||
                     fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows));

            foreach (var fi in hostedFamilies)
            {
                var info = new OpeningInfo
                {
                    IdText = fi.Id.IntegerValue.ToString(),
                    CategoryName = fi.Category?.Name ?? "-",
                    TypeName = fi.Symbol?.Name ?? fi.Name,
                    LevelName = doc.GetElement(fi.LevelId) is Level lvl ? lvl.Name : null,
                    WidthMm = TryGetSizeMm(doc, fi, isWidth: true),
                    HeightMm = TryGetSizeMm(doc, fi, isWidth: false),
                    Notes = "Hosted family"
                };
                result.Add(info);
            }

            // Opening elements created with the Opening tool
            var openings = new FilteredElementCollector(doc)
                .OfClass(typeof(Opening))
                .WhereElementIsNotElementType()
                .Cast<Opening>()
                .Where(op => op.Host != null && op.Host.Id == wall.Id);

            foreach (var op in openings)
            {
                double? w = null, h = null;
                var bb = op.get_BoundingBox(null);
                if (bb != null)
                {
                    double ftWidth = Math.Abs(bb.Max.X - bb.Min.X);
                    double ftHeight = Math.Abs(bb.Max.Z - bb.Min.Z);
                    w = ConvertToMm(ftWidth);
                    h = ConvertToMm(ftHeight);
                }

                var info = new OpeningInfo
                {
                    IdText = op.Id.IntegerValue.ToString(),
                    CategoryName = "Opening",
                    TypeName = "Wall Opening",
                    LevelName = doc.GetElement(op.LevelId) is Level lvl ? lvl.Name : null,
                    WidthMm = w,
                    HeightMm = h,
                    Notes = "Opening element"
                };
                result.Add(info);
            }

            return result
                .OrderBy(i => i.CategoryName)
                .ThenBy(i => i.TypeName)
                .ThenBy(i => i.LevelName)
                .ToList();
        }

        private static double? TryGetSizeMm(Document doc, FamilyInstance fi, bool isWidth)
        {
            // Prefer door/window built-ins
            BuiltInParameter[] bips = isWidth
                ? new[] { BuiltInParameter.DOOR_WIDTH, BuiltInParameter.WINDOW_WIDTH }
                : new[] { BuiltInParameter.DOOR_HEIGHT, BuiltInParameter.WINDOW_HEIGHT };

            foreach (var bip in bips)
            {
                var p = fi.get_Parameter(bip);
                if (p != null && p.HasValue)
                    return ConvertToMm(p.AsDouble());
            }

            // Fallback: lookup by common parameter names
            // This captures "Width"/"Height" parameters defined in many families
            string[] names = isWidth
                ? new[] { "Width", "W", "Clear Width" }
                : new[] { "Height", "H", "Clear Height" };

            foreach (var name in names)
            {
                var p = fi.LookupParameter(name);
                if (p != null && p.HasValue && p.StorageType == StorageType.Double)
                    return ConvertToMm(p.AsDouble());
            }

            // Final fallback: bounding box estimation
            var bb = fi.get_BoundingBox(null);
            if (bb != null)
            {
                return isWidth
                    ? ConvertToMm(Math.Abs(bb.Max.X - bb.Min.X))
                    : ConvertToMm(Math.Abs(bb.Max.Z - bb.Min.Z));
            }

            return null;
        }

        private static double ConvertToMm(double internalFeet)
        {
            // Revit internal units are feet; convert to millimeters
            return UnitUtils.ConvertFromInternalUnits(internalFeet, UnitTypeId.Millimeters);
        }
    }
}
