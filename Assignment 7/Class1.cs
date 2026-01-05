
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

            // Create ViewModel and pass current document
            var vm = new ViewModel.TreeViewViewModel(doc);

            // Create window and bind ViewModel
            var window = new View.TreeViewWindow
            {
                DataContext = vm
            };

            // Show modal (recommended for Revit)
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
