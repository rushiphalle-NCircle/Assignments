using Assignment3.Commands;
using Assignment3.Commands.Model;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Assignments3.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ActiveView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;


            var floorPlans = new FilteredElementCollector(doc)
                               .OfClass(typeof(ViewPlan))
                               .Cast<ViewPlan>()
                               .Where(f => !f.IsTemplate && f.ViewType == ViewType.FloorPlan)
                               .OrderBy(f => f.Name)
                               .Select(f => new MyModel { Id = f.Id.IntegerValue, Name = f.Name })
                               .ToList();

            var window = new FloorPlanSelector(uiDoc, floorPlans);
            new WindowInteropHelper(window) { Owner = uiApp.MainWindowHandle };
            window.ShowDialog();



            return Result.Succeeded;
        }
    }
}
