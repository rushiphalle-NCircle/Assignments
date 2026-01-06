
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using FamilyLoader.ViewModels;
using FamilyLoader.Views;

using System;

namespace FamilyLoader.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class LoadFamiliesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument?.Document;
            if (doc == null)
            {
                TaskDialog.Show("Family Loader", "No active document.");
                return Result.Failed;
            }

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Family Loader", "Please run this from a project document, not a family document.");
                return Result.Cancelled;
            }

            var vm = new MainViewModel(doc);
            var window = new MainWindow { DataContext = vm };

            // Optionally set Revit as owner (kept minimal)
            // new System.Windows.Interop.WindowInteropHelper(window).Owner = uiApp.MainWindowHandle;

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
