
namespace FamilyLoader.Models
{
    public class FamilyLoadRequest
    {
        public string FolderPath { get; set; }
        public bool Recursive { get; set; } = true;
    }
}
