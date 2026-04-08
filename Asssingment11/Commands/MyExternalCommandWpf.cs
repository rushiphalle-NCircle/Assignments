
// Assignment11/Commands/MyExternalCommandWpf.cs
using System;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Interop;
using Assignment11.ViewModel;

namespace Assignment11.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    public class MyExternalCommandWpf : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;


            var vm = new MyViewModel(doc);

            var view = new inmbuild.Views.MyView();
            view.DataContext = vm;   

            var helper = new WindowInteropHelper(view)
            {
                Owner = uiapp.MainWindowHandle
            };

            view.ShowDialog();


            return Result.Succeeded;
        }
    }
}
