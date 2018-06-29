using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Parsing {
    class SkryptException : Exception {
        public int urgency = -1;

        public SkryptException() {
        }

        public SkryptException(string message)
            : base(message) {
        }

        public SkryptException(string message, int u)
            : base(message) {
            urgency = u;
        }
    }
}
