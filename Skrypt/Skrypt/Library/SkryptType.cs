using Skrypt.Engine;
using System;

namespace Skrypt.Library
{
    public class SkryptType : SkryptObject
    {
        public virtual bool CreateCopyOnAssignment { get; set; } = true;
        public virtual string TypeName => GetType().ToString();
    }
}