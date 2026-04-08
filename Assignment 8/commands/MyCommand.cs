
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Assignment8.Commands.ExternalEvents;

namespace Assignment8.Commands
{


    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var handler = new ActivateViewHandler();
            var externalEvent = ExternalEvent.Create(handler);
            var vm = new ViewModel.TreeViewViewModel(doc, handler, externalEvent);

            var window = new Views.TreeViewWindow
            {
                DataContext = vm
            };

            new System.Windows.Interop.WindowInteropHelper(window)
                .Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            window.Show(); 

            return Result.Succeeded;
        }
    }
}