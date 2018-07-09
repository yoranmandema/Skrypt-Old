using Skrypt.Engine;

namespace Skrypt.Library
{
    public class SkryptType : SkryptObject
    {
        public bool CreateCopyOnAssignment = false;

        public string TypeName => GetType().ToString();

        public void Init(SkryptEngine engine)
        {
            var baseObject = engine.Types[TypeName];

            SetPropertiesTo(baseObject);
        }
    }
}