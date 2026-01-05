
using Assignment10.Commands.Model;
using Assignment10.Commands.ViewModel;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using System.Linq;

namespace Assignment10.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class GetWallOpeningsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // (Optional) Warn if not in a plan view
            if (!(doc.ActiveView is ViewPlan vp) || vp.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Assignment10", "Tip: Open a Floor Plan view for best results.\nContinuing anyway...");
            }

            // Get selected wall or prompt to pick
            Wall wall = null;
            var selIds = uidoc.Selection.GetElementIds();
            if (selIds.Count == 1)
            {
                var el = doc.GetElement(selIds.First());
                wall = el as Wall;
            }
            if (wall == null)
            {
                var pickedRef = uidoc.Selection.PickObject(ObjectType.Element, new WallSelectionFilter(), "Pick a wall");
                wall = doc.GetElement(pickedRef.ElementId) as Wall;
            }
            if (wall == null)
            {
                message = "No wall selected.";
                return Result.Cancelled;
            }

            // Model: extract openings
            var openings = OpeningExtractor.GetOpenings(doc, wall);

            // ViewModel: shape data
            var vm = new WallOpeningsViewModel(wall, openings);

            // Show message box (TaskDialog)
            //TaskDialog td = new TaskDialog("Wall Openings");
            //td.MainInstruction = $"Openings in wall: \"{vm.WallName}\" (Level: {vm.WallLevelName ?? "-"})";
            //td.MainContent = vm.AsSummaryText();
            //td.Show();

            // (Optional) WPF view (uncomment if you want rich UI)
             var win = new Assignment10.Commands.Views.WallOpeningsWindow { DataContext = vm };
            win.ShowDialog();

            return Result.Succeeded;
        }
    }

    internal class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) => elem is Wall;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
