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
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    public class MyExternalCommandWpf : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var handler = new RevitSelectionEventHandler();
            var externalEvent = ExternalEvent.Create(handler);

            var vm = new MyViewModel(doc, externalEvent, handler);

            var window = new MyView { DataContext = vm };

            var helper = new WindowInteropHelper(window) { Owner = uiapp.MainWindowHandle };

            window.ShowDialog();
            return Result.Succeeded;
        }
    }

    public class RevitSelectionEventHandler : IExternalEventHandler
    {
        public SelectionRequest Request { get; set; }

        public void Execute(UIApplication app)
        {
            if (Request == null || Request.LevelId == null) return;

            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            ActivateLevelPlanView(doc, uidoc, Request.LevelId);

            IEnumerable<Element> elements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(e =>
                {
                    var levelParam = e.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                    if (levelParam != null && levelParam.AsElementId() == Request.LevelId)
                        return true;

                    if (e is FamilyInstance fi && fi.LevelId == Request.LevelId)
                        return true;

                    return false;
                });

            if (Request.Category.HasValue)
            {
                elements = elements.Where(e =>
                {
                    Category cat = e.Category;
                    if (cat == null) return false;
                    return (BuiltInCategory)cat.Id.IntegerValue == Request.Category.Value;
                });
            }

            var ids = elements.Select(e => e.Id).ToList();

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
        }

        public string GetName() => "Assignment12 Selection ExternalEvent Handler";
    }
}
