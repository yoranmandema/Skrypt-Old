namespace Skrypt.Library
{
    public enum Access
    {
        Private,
        Public
    }

    public class SkryptProperty
    {
        public string Name { get; set; }
        public SkryptObject Value { get; set; }
        public Access Accessibility { get; set; } = Access.Public;
    }
}