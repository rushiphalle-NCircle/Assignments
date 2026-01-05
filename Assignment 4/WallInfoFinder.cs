using Assignments.Assignment_4.utils;

using Assignments4.Commands.model;
using Assignments4.Commands.View;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Assignments4.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class WallInfoFinder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var selectedIds = uiDoc.Selection.GetElementIds();

            //Filtering for walls

            var selectedWalls = selectedIds
                .Select(id => doc.GetElement(id))
                .OfType<Wall>()
                .Select(wall => new WallInfo() { Name = wall.Name, WallProperties = Fetcher.getWallInfo(wall, doc) })
                .ToList();


            var window = new WallInfoBox(selectedWalls);
            new WindowInteropHelper(window) { Owner = app.MainWindowHandle };
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
