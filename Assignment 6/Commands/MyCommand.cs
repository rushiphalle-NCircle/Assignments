
// File: MyCommand.cs
using Assignment6.Commands.Model;
using Assignment6.Commands.ViewModel;
using Assignment6.Commands.Views;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System.Linq;

namespace Assignment6.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var floorPlans = new FilteredElementCollector(doc)
                                .OfClass(typeof(ViewPlan))
                                .Cast<ViewPlan>()
                                .Where(v => !v.IsTemplate && v.ViewType == ViewType.FloorPlan)
                                .Select(v => new FloorModel { Id = v.Id, Name = v.Name })
                                .ToList();

            var roomsHandler = new MyExternalCommand();
            var roomsEvent = ExternalEvent.Create(roomsHandler);

            var window = new MyView(new MyViewModel(floorPlans, roomsEvent, roomsHandler));

            // Optional: set owner to Revit main window for proper modality
            // var revitHandle = uiApp.MainWindowHandle;
            // new System.Windows.Interop.WindowInteropHelper(window).Owner = revitHandle;

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}