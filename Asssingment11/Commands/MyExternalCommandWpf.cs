
// Assignment11/Commands/MyExternalCommandWpf.cs
using System;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Interop;
using Assignment11.ViewModel;

namespace Assignment11.Commands
{
    
    /// <summary>
    /// ExternalCommand that opens the MVVM WPF window showing level-wise counts.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    public class MyExternalCommandWpf : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Build the ViewModel with current Revit Document

            var vm = new Assignment11.ViewModel.MyViewModel(doc);

            var view = new inmbuild.Views.MyView();
            view.DataContext = vm;   // ✅ CORRECT PLACE

            var helper = new System.Windows.Interop.WindowInteropHelper(view)
            {
                Owner = uiapp.MainWindowHandle
            };

            view.ShowDialog();


            return Result.Succeeded;
        }
    }
}
