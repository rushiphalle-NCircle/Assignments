
// Assignment11/Model/MyModel.cs
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace Assignment11.Model
{
    /// <summary>
    /// Base node for tree structure.
    /// </summary>
    public abstract class TreeNode
    {
        public string Name { get; set; }
        public ObservableCollection<TreeNode> Children { get; } = new ObservableCollection<TreeNode>();
        public override string ToString() => Name;
    }

    public class LevelNode : TreeNode
    {
        public ElementId LevelId { get; }
        public LevelNode(string name, ElementId id)
        {
            Name = name;
            LevelId = id;
        }
    }

    public class CategoryCountNode : TreeNode
    {
        public string CategoryName { get; }
        public int Count { get; }

        public string DisplayName => $"{CategoryName} ({Count})";

        public CategoryCountNode(string categoryName, int count)
        {
            CategoryName = categoryName;
            Count = count;
            Name = DisplayName; // so default ToString shows correctly even without templates
        }
    }

    public static class TreeBuilder
    {
        /// <summary>
        /// Builds Level -> CategoryCount nodes from a Revit Document.
        /// </summary>
        public static ObservableCollection<LevelNode> Build(Document doc)
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation) // nice ordering
                .ToList();

            var result = new ObservableCollection<LevelNode>();

            foreach (var level in levels)
            {
                var levelNode = new LevelNode(level.Name, level.Id);

                // --- Walls ---
                int wallCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Count(e =>
                    {
                        var p = e.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                        return p != null && p.AsElementId() == level.Id;
                    });

                if (wallCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Walls", wallCount));

                // --- Doors (FamilyInstances on level) ---
                int doorCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (doorCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Doors", doorCount));

                // You can add more categories similarly:
                // Windows
                int windowCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (windowCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Windows", windowCount));

                // Add the level node only if it has any child category count
                if (levelNode.Children.Any())
                    result.Add(levelNode);
            }

            return result;
        }
    }
}
