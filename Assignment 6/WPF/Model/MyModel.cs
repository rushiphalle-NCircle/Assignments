
// File: Models.cs
using Autodesk.Revit.DB;

namespace Assignment6.Commands.Model
{
    public class FloorModel
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
    }

    public class RoomModel
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
    }

    public class Walls
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
    }
}