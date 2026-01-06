
// Assignment12/Model/MyModel.cs
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace Assignment12.Model
{
    // ---------- Tree nodes ----------
    public abstract class TreeNode
    {
        public string Name { get; set; }
        public ObservableCollection<TreeNode> Children { get; } = new ObservableCollection<TreeNode>();
    }

    public class LevelNode : TreeNode
    {
        public ElementId LevelId { get; }
        public LevelNode(Level level)
        {
            Name = level.Name;
            LevelId = level.Id;
        }
    }

    public class CategoryCountNode : TreeNode
    {
        public string CategoryName { get; }
        public BuiltInCategory Category { get; }
        public int Count { get; }
        public ElementId LevelId { get; }

        public string DisplayName => $"{CategoryName} ({Count})";
        public CategoryCountNode(string categoryName, BuiltInCategory category, int count, ElementId levelId)
        {
            CategoryName = categoryName;
            Category = category;
            Count = count;
            LevelId = levelId;
            Name = DisplayName;
        }
    }

    // ---------- Tree builder ----------
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
                var levelNode = new LevelNode(level);

                // Walls (LEVEL_PARAM)
                int wallCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Count(e => e.get_Parameter(BuiltInParameter.LEVEL_PARAM)?.AsElementId() == level.Id);

                if (wallCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Walls", BuiltInCategory.OST_Walls, wallCount, level.Id));

                // Doors (FamilyInstance.LevelId)
                int doorCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (doorCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Doors", BuiltInCategory.OST_Doors, doorCount, level.Id));

                // Windows (FamilyInstance.LevelId)
                int windowCount = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Count(fi => fi.LevelId == level.Id);

                if (windowCount > 0)
                    levelNode.Children.Add(new CategoryCountNode("Windows", BuiltInCategory.OST_Windows, windowCount, level.Id));

                if (levelNode.Children.Any())
                    result.Add(levelNode);
            }

            return result;
        }
    }

    // ---------- Selection request ----------
    public class SelectionRequest
    {
        public ElementId LevelId { get; set; }
        public BuiltInCategory? Category { get; set; } // null = select all elements on level
    }
}
