using System.Text.RegularExpressions;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     Represents a token rule with a REGEX pattern, and a return type.
    /// </summary>
    internal class TokenRule
    {
        public Regex Pattern { get; set; }
        public TokenTypes Type { get; set; }
    }
}