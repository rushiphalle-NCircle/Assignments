
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Assignment8.Commands.ExternalEvents
{
    /// <summary>
    /// Runs inside Revit's API context to activate a view from its ElementId.
    /// </summary>
    public class ActivateViewHandler : IExternalEventHandler
    {
        public ElementId TargetViewId { get; set; }

        public void Execute(UIApplication app)
        {
            if (TargetViewId == null) return;

            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(TargetViewId) as View;

            // Request to change active view asynchronously (safe from ExternalEvent).
            if (view != null && !view.IsTemplate)
            {
                uidoc.RequestViewChange(view);
            }

            // Clear after use
            TargetViewId = null;
        }

        public string GetName() => "Assignment8 - Activate View Handler";
    }
}
