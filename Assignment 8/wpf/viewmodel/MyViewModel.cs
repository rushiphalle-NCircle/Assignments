
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Assignment8.Commands.Model;
using Assignment8.Commands.ExternalEvents;

namespace Assignment8.Commands.ViewModel
{
    public class TreeViewViewModel
    {
        public ObservableCollection<TreeNode> Nodes { get; }

        private readonly UIDocument _uidoc;
        private readonly ActivateViewHandler _handler;
        private readonly ExternalEvent _externalEvent;

        public TreeViewViewModel(Document doc, ActivateViewHandler handler, ExternalEvent externalEvent)
        {
            _uidoc = new UIDocument(doc);
            _handler = handler;
            _externalEvent = externalEvent;

            Nodes = new ObservableCollection<TreeNode>();

            // Root node = document title
            var root = new TreeNode(doc.Title);
            Nodes.Add(root);

            // Floor Plans as children
            var plans = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan));

            foreach (ViewPlan vp in plans)
            {
                if (vp.ViewType == ViewType.FloorPlan && !vp.IsTemplate)
                {
                    root.Children.Add(new TreeNode(vp.Name, vp.Id));
                }
            }
        }

        // Called by the View when a plan node is clicked
        public void ActivateView(TreeNode node)
        {
            if (node?.ViewId == null) return;

            // Set target and raise external event to change active view
            _handler.TargetViewId = node.ViewId;
            _externalEvent.Raise();
        }
    }
}