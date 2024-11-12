using System.Text.RegularExpressions;

namespace AonFreelancing.Utilities
{
    public class StringUtils
    {
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string ReplaceWhitespace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }
    }
}
