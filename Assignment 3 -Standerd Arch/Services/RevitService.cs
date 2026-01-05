
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Assignment3S.Commands.Model;

namespace Assignment3S.Services
{
    public class RevitService : IRevitService
    {
        private readonly UIDocument _uiDoc;
        private Document Doc => _uiDoc.Document;

        public RevitService(UIDocument uiDoc) => _uiDoc = uiDoc;

        public IList<MyModel> GetFloorPlans()
        {
            var floorPlans = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate && v.ViewType == ViewType.FloorPlan)
                .OrderBy(v => v.Name)
                .Select(v => new MyModel { Id = v.Id.IntegerValue, Name = v.Name })
                .ToList();

            return floorPlans;
        }

        public View GetViewById(int id)
        {
            var eid = new ElementId(id);
            return Doc.GetElement(eid) as View;
        }

        public void ActivateView(View view)
        {
            // For modal dialogs, RequestViewChange is fine.
            _uiDoc.RequestViewChange(view);
        }

        public void SelectAllWallsInView(View view)
        {
            // Collect walls visible in the given view
            var wallIds = new FilteredElementCollector(Doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElementIds();

            _uiDoc.Selection.SetElementIds(wallIds);
        }
    }
}
