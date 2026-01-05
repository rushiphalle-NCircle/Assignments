using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assignment9.Commands.Model;
using Assignment9.Commands.Views;
using Assignment9.Commands.ViewModel;

namespace Assignment9.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var walls = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .WhereElementIsNotElementType()
                        .OfType<Wall>()
                        .Select(w => getWallData(doc, w))
                        .OrderBy(wd => wd.Elivation).ToList();

            var window = new WallBox()
            {
                DataContext = new MyViewModel(walls)
            };

            window.ShowDialog();
            return Result.Failed;
        }

        private WallData getWallData(Document doc, Wall wall)
        {
            var level = doc.GetElement(wall.LevelId) as Level;
            return new WallData()
            {
                Id = wall.Id,
                Name = wall.Name,
                LevelId = wall.LevelId,
                LevelName = level.Name,
                Elivation = level.Elevation
            };
        }
    }

    
}
