using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignments4.Commands.AVClass
{
    public class MyAvailabilityClass : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            UIDocument uiDoc = applicationData.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var ids = uiDoc.Selection.GetElementIds();

            foreach (var id in ids)
            {
                Element e = doc.GetElement(id);
                if (e is Wall) return true;
            }

            return false;
        }
    }
}
