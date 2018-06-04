using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization {
    class TokenRule {
        public Regex Pattern { get; set; }
        public string Type { get; set; }
    }
}
