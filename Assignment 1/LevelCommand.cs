
using System;
using System.Linq;
using System.Windows.Interop;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Asignment1.Commands.Models;
using Asignment1.Commands.Views;

namespace Asignment1.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class LevelCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Collect levels
                var levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .Select(l => new
                    {
                        Id = l.Id,
                        Name = l.Name,
                        ElevationFeet = l.Elevation 
                    })
                    .ToList();

                var trial = new FilteredElementCollector(doc).WhereElementIsElementType();

                var floorPlans = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewPlan))
                    .Cast<ViewPlan>()
                    .Where(vp => !vp.IsTemplate && vp.ViewType == ViewType.FloorPlan)
                    .Select(vp => new
                    {
                        LevelId = vp.GenLevel != null ? vp.GenLevel.Id : ElementId.InvalidElementId,
                        Name = vp.Name
                    })
                    .ToList();
  
                var items = levels.Join(
                        floorPlans,
                        l => l.Id,
                        f => f.LevelId,
                        (l, f) => new LevelPlanInfo
                        {
                            LevelId = l.Id.IntegerValue,
                            LevelName = l.Name,
                            ElevationFeet = l.ElevationFeet,
                            ViewName = f.Name
                        })
                    .OrderBy(x => x.ElevationFeet)
                    .ToList();

                // Create and show WPF window (modal) with Revit as owner
                var window = new LevelPlanWindow(items);

                // Set Revit main window as owner so the dialog behaves properly
                var revitHwnd = app.MainWindowHandle;
                var helper = new WindowInteropHelper(window) { Owner = revitHwnd };

                window.ShowDialog(); // modal — simplest & safest

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
