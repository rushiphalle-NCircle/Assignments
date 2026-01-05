using Assignment5B.Commands.Model;
using Assignment5B.Commands.Views;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5B.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View av = doc.ActiveView;

            var rooms = new FilteredElementCollector(doc, av.Id)
                        .OfCategory(BuiltInCategory.OST_Rooms)
                        .WhereElementIsNotElementType()
                        .Cast<Room>()
                        .Select(r => new MyModel() { Id = r.Id, Name = r.Name})
                        .ToList();

            var window = new MyView(rooms);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
