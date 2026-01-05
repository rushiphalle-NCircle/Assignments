using Assignments4.Commands.model;

using Autodesk.Revit.DB;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Assignments.Assignment_4.utils
{
    internal class Fetcher
    {
        public static Dictionary<string, string> getWallInfo(Wall wall, Document doc)
        {
            var Properties = new Dictionary<string, string>();

            // ---- Helpful metadata ----
            Properties["ElementId"] = wall.Id.IntegerValue.ToString();

            if (wall.Category != null) Properties["Category"] = wall.Category.Name;

            var wallType = doc.GetElement(wall.GetTypeId()) as WallType;
            if (wallType != null) Properties["Type Name"] = wallType.Name;

            var baseLevel = doc.GetElement(wall.LevelId) as Level;
            if (baseLevel != null) Properties["Base Level"] = baseLevel.Name;

            // ---- Instance parameters ----
            foreach (Parameter p in wall.Parameters)
            {
                string name = p.Definition?.Name ?? $"Param {p.Id.IntegerValue}";

                // Prefer formatted value, then fallbacks
                string value = null;
                try
                {
                    value = p.AsValueString();
                }
                catch { /* some params don't support AsValueString */ }

                if (string.IsNullOrWhiteSpace(value))
                {
                    switch (p.StorageType)
                    {
                        case StorageType.String:
                            value = p.AsString() ?? string.Empty;
                            break;
                        case StorageType.Double:
                            value = p.AsDouble().ToString(CultureInfo.InvariantCulture);
                            break;
                        case StorageType.Integer:
                            value = p.AsInteger().ToString();
                            break;
                        case StorageType.ElementId:
                            var eid = p.AsElementId();
                            value = (eid == ElementId.InvalidElementId) ? string.Empty : eid.IntegerValue.ToString();
                            break;
                        default:
                            value = string.Empty;
                            break;
                    }
                }

                // Avoid duplicate keys
                if (Properties.ContainsKey(name)) name = $"{name} ({p.Id.IntegerValue})";
                Properties[name] = value;
            }

            // ---- Type parameters ----
            if (wallType != null)
            {
                foreach (Parameter p in wallType.Parameters)
                {
                    string name = p.Definition?.Name ?? $"Type Param {p.Id.IntegerValue}";
                    name = $"Type - {name}";

                    string value = null;
                    try
                    {
                        value = p.AsValueString();
                    }
                    catch { }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        switch (p.StorageType)
                        {
                            case StorageType.String:
                                value = p.AsString() ?? string.Empty;
                                break;
                            case StorageType.Double:
                                value = p.AsDouble().ToString(CultureInfo.InvariantCulture);
                                break;
                            case StorageType.Integer:
                                value = p.AsInteger().ToString();
                                break;
                            case StorageType.ElementId:
                                var eid = p.AsElementId();
                                value = (eid == ElementId.InvalidElementId) ? string.Empty : eid.IntegerValue.ToString();
                                break;
                            default:
                                value = string.Empty;
                                break;
                        }
                    }

                    if (Properties.ContainsKey(name)) name = $"{name} ({p.Id.IntegerValue})";
                    Properties[name] = value;
                }
            }

            return Properties;
        }
    }
}
