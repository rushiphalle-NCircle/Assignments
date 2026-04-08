
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace Assignment7.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            
            var vm = new ViewModel.TreeViewViewModel(doc);

            var window = new View.TreeViewWindow
            {
                DataContext = vm
            };

            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
