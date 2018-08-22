using Skrypt.Engine;
using System;
using Skrypt.Library.Native;

namespace Skrypt.Library
{
    public class SkryptType : SkryptObject
    {
        public virtual bool CreateCopyOnAssignment { get; set; } = true;
        public virtual string TypeName => GetType().ToString();
    }
}