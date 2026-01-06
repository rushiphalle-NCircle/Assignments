
// File: SetDoorParametersCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Assignment13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SetDoorParametersCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            try
            {
                // Try using current selection first
                var selIds = uidoc.Selection.GetElementIds();
                var doorIds = FilterDoorIds(doc, selIds).ToList();

                // If no doors selected, prompt user to pick doors
                if (doorIds.Count == 0)
                {
                    var picked = uidoc.Selection.PickObjects(ObjectType.Element, new DoorSelectionFilter(),
                        "Select door(s) to edit parameters");
                    doorIds = picked.Select(p => p.ElementId).Where(id => IsDoor(doc.GetElement(id))).Distinct().ToList();
                }

                if (doorIds.Count == 0)
                {
                    TaskDialog.Show("Set Door Parameters", "No doors selected.");
                    return Result.Cancelled;
                }

                // Create VM and show WPF window
                var vm = new DoorParametersViewModel(uidoc, doorIds.ToArray());
                var win = new DoorParametersWindow();
                win.BindVM(vm, uiapp.MainWindowHandle);

                win.ShowDialog(); // Transaction & apply are handled by the VM when the user clicks "Apply"
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private static IEnumerable<ElementId> FilterDoorIds(Document doc, ICollection<ElementId> ids)
        {
            if (ids == null || ids.Count == 0) yield break;

            foreach (var id in ids)
            {
                var el = doc.GetElement(id);
                if (IsDoor(el)) yield return id;
            }
        }

        private static bool IsDoor(Element el)
        {
            if (el == null) return false;
            var cat = el.Category;
            return cat != null && cat.Id.IntegerValue == (int)BuiltInCategory.OST_Doors;
        }
    }

    /// <summary>
    /// Selection filter that accepts only door elements.
    /// </summary>
    public class DoorSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            var cat = elem?.Category;
            return cat != null && cat.Id.IntegerValue == (int)BuiltInCategory.OST_Doors;
        }

        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
