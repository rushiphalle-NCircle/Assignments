
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

            
            if (!(doc.ActiveView is ViewPlan vp) || vp.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Assignment10", "Tip: Open a Floor Plan view for best results.\nContinuing anyway...");
            }

            
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

            
            var openings = OpeningExtractor.GetOpenings(doc, wall);

            
            var vm = new WallOpeningsViewModel(wall, openings);

            
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
