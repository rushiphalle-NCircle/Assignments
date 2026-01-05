
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using Assignment7.Commands.Model;

namespace Assignment7.Commands.ViewModel
{
    public class TreeViewViewModel
    {
        public ObservableCollection<TreeNode> Nodes { get; }

        public TreeViewViewModel(Document doc)
        {
            Nodes = new ObservableCollection<TreeNode>();

            // Root: document title
            var root = new TreeNode(doc.Title);
            Nodes.Add(root);

            // Children: Floor Plans
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan));

            foreach (ViewPlan vp in collector)
            {
                if (vp.ViewType == ViewType.FloorPlan && !vp.IsTemplate)
                {
                    root.Children.Add(new TreeNode(vp.Name));
                }
            }
        }
    }
}
