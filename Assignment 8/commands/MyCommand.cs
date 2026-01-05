
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

            // Create ExternalEvent + handler
            var handler = new ActivateViewHandler();
            var externalEvent = ExternalEvent.Create(handler);

            // Create ViewModel and pass doc + external event pieces
            var vm = new ViewModel.TreeViewViewModel(doc, handler, externalEvent);

            // Modeless window (so Revit can react while it's open)
            var window = new Views.TreeViewWindow
            {
                DataContext = vm
            };

            // Set Revit window as owner (optional, keeps it on top of Revit)
            new System.Windows.Interop.WindowInteropHelper(window)
                .Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            window.Show(); // modeless

            return Result.Succeeded;
        }
    }
}