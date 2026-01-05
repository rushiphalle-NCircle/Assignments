using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Assignment6.Commands.Model;


namespace Assignment5A.Commands
{
    
    public class ChangeViewHandler : IExternalEventHandler
    {
        public ElementId TargetViewId { get; set; }
      

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            View view = uiDoc.Document.GetElement(TargetViewId) as View;

            if (view != null)
            {
                uiDoc.RequestViewChange(view);

            }
        }

        public string GetName() => "Change View Handler";
    }
}
