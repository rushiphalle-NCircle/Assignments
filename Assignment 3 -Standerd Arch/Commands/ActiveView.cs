
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Interop;
using Assignment3S.Commands.ViewModel;
using Assignment3S.Services;

namespace Assignment3S.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ActiveView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;

            // Construct service + VM
            var revitService = new RevitService(uiDoc);
            var vm = new MyViewModel(revitService);
            vm.Load();

            // Show the modal WPF window
            var window = new FloorPlanSelector();
            window.Bind(vm);
            new WindowInteropHelper(window) { Owner = uiApp.MainWindowHandle };
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
