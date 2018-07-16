namespace Skrypt.Library
{
    public enum Access
    {
        Private,
        Public
    }

    public enum GetOrSet {
        None,
        Getter,
        Setter
    }

    public class SkryptProperty
    {
        public string Name { get; set; }
        public SkryptObject Value { get; set; }
        public Access Accessibility { get; set; } = Access.Public;
        public bool IsStatic { get; set; } = false;
        public bool IsConstant { get; set; } = false;
        public bool IsGetter { get; set; } = false;
        public bool IsSetter { get; set; } = false;
    }
}