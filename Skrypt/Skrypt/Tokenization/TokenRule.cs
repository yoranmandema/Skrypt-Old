using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization {
    /// <summary>
    /// Represents a token rule with a REGEX pattern, and a return type.
    /// </summary>
    class TokenRule {
        public Regex Pattern { get; set; }
        public TokenTypes Type { get; set; }
    }
}
