using Assignment5A.Commands.Model;
using Assignment5A.Commands.ViewModel;
using Assignment5A.Commands.Views;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5A.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MyCommand : IExternalCommand
    {
        public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate && v.ViewType == ViewType.FloorPlan)
                .Select(v => new ViewItem
                {
                    ViewId = v.Id,
                    Name = v.Name
                })
                .ToList();

            var handler = new ChangeViewHandler();
            var externalEvent = ExternalEvent.Create(handler);

            var vm = new MyViewModel(views, externalEvent, handler);
            var window = new MyView(vm);

            window.ShowDialog();


            return Result.Succeeded;
        }
    }
}
