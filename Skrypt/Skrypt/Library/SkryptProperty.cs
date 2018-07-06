using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library {
    public enum Access {
        Private,
        Public
    }

    public class SkryptProperty {
        public string Name;
        public SkryptObject Value;
        public Access Accessibility;
    }
}
