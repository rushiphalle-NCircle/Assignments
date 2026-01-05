
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

namespace Assignment8.Commands.Model
{
    /// <summary>
    /// Simple node with a name, a Revit ElementId (for views), and children.
    /// </summary>
    public class TreeNode
    {
        public string Name { get; set; }
        public ElementId ViewId { get; set; } // null for root

        public ObservableCollection<TreeNode> Children { get; set; }

        public TreeNode(string name, ElementId viewId = null)
        {
            Name = name;
            ViewId = viewId;
            Children = new ObservableCollection<TreeNode>();
        }
    }
}
