namespace DDW.Enums
{
    public enum NameVisibilityRestriction
    {
        Everyone, // No restriction - public / internal / protected internal
        Family,   // Only in the family - protected
        Self      // Only in self - private
    }
}
