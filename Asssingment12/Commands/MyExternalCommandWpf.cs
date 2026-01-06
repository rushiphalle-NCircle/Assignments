
// Assignment12/Commands/MyExternalCommandWpf.cs
using System.Reflection;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Interop;
using Assignment12.View;
using Assignment12.ViewModel;
using Assignment12.Model;
using System.Collections.Generic;

namespace Assignment12.Commands
{
    /// <summary>
    /// ExternalCommand that opens the MVVM window.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    public class MyExternalCommandWpf : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Revit ExternalEvent handler to run API safely from WPF
            var handler = new RevitSelectionEventHandler();
            var externalEvent = ExternalEvent.Create(handler);

            // ViewModel
            var vm = new MyViewModel(doc, externalEvent, handler);

            // View
            var window = new MyView { DataContext = vm };

            // Make Revit the owner (keeps dialog on top)
            var helper = new WindowInteropHelper(window) { Owner = uiapp.MainWindowHandle };

            window.ShowDialog();
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Handles selection requests from the ViewModel on Revit's UI thread.
    /// Activates the level floor plan view and selects (highlights) elements.
    /// </summary>
    public class RevitSelectionEventHandler : IExternalEventHandler
    {
        public SelectionRequest Request { get; set; }

        public void Execute(UIApplication app)
        {
            if (Request == null || Request.LevelId == null) return;

            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 1) Activate the floor plan view for the level
            ActivateLevelPlanView(doc, uidoc, Request.LevelId);

            // 2) Collect elements on that level as IEnumerable<Element> (LINQ-safe)
            IEnumerable<Element> elements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(e =>
                {
                    // Many elements are keyed to level via LEVEL_PARAM
                    var levelParam = e.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                    if (levelParam != null && levelParam.AsElementId() == Request.LevelId)
                        return true;

                    // FamilyInstances (Doors/Windows etc.) reliably expose LevelId
                    if (e is FamilyInstance fi && fi.LevelId == Request.LevelId)
                        return true;

                    return false;
                });

            // Optional category filter (still LINQ on IEnumerable<Element>)
            if (Request.Category.HasValue)
            {
                elements = elements.Where(e =>
                {
                    Category cat = e.Category;
                    if (cat == null) return false;
                    // Compare BuiltInCategory via category integer id
                    return (BuiltInCategory)cat.Id.IntegerValue == Request.Category.Value;
                });
            }

            var ids = elements.Select(e => e.Id).ToList();

            // 3) Highlight (select) elements in active view
            uidoc.Selection.SetElementIds(ids);
        }

        private void ActivateLevelPlanView(Document doc, UIDocument uidoc, ElementId levelId)
        {
            var floorPlan = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .FirstOrDefault(v =>
                    v.ViewType == ViewType.FloorPlan &&
                    v.GenLevel != null &&
                    v.GenLevel.Id == levelId);

            if (floorPlan != null)
            {
                uidoc.ActiveView = floorPlan;
            }
            // If there is no floor plan for that level, we silently skip.
            // (Optionally: show a TaskDialog to inform the user.)
        }

        public string GetName() => "Assignment12 Selection ExternalEvent Handler";
    }
}
