
using Autodesk.Revit.DB;

namespace FamilyLoader.Models
{
    /// <summary>
    /// Accepts existing families/types silently (avoids duplicate dialogs).
    /// </summary>
    public class SimpleFamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            // If family exists, keep existing parameter values
            overwriteParameterValues = false;
            return true; // accept existing
        }

        public bool OnSharedFamilyFound(
            Family sharedFamily, bool familyInUse,
            out FamilySource source, out bool overwriteParameterValues)
        {
            // Prefer the existing shared family in the project
            source = FamilySource.Family;
            overwriteParameterValues = false;
            return true;
        }
    }
}

