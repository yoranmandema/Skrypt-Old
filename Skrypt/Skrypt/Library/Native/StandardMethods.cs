using Skrypt.Engine;

namespace Skrypt.Library.Native
{
    public partial class StandardMethods
    {
        private SkryptEngine _engine;

        public StandardMethods(SkryptEngine e)
        {
            _engine = e;
        }

        public void AddMethodsToEngine()
        {
            //var Methods = this.GetType().GetMethods().Where((m) => {
            //    if (!m.IsStatic) {
            //        return false;
            //    }

            //    return true;
            //});

            //foreach (MethodInfo M in Methods) {
            //    SharpMethod Method = new SharpMethod();

            //    Method.method = (SkryptDelegate) Delegate.CreateDelegate(typeof(SkryptDelegate), M);
            //    Method.Name = M.Name;

            //    engine.Methods.Add(Method);
            //}
        }
    }
}