using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace Assignment11.Model
{
    
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
            Name = DisplayName; 
        }
    }

    public static class TreeBuilder
    {
        public static ObservableCollection<LevelNode> Build(Document doc)
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            var result = new ObservableCollection<LevelNode>();

            foreach (var level in levels)
            {
                var levelNode = new LevelNode(level.Name, level.Id);

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

                int doorCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (doorCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Doors", doorCount));

                int windowCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (windowCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Windows", windowCount));

                if (levelNode.Children.Any())
                    result.Add(levelNode);
            }

            return result;
        }
    }
}
