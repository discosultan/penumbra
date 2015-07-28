using System.Text;

namespace Sandbox.Utilities
{
    static class StringExtensions
    {
        public static string SeparateTitleCases(this string source, string separator = " ")
        {
            var result = new StringBuilder();
            char previousChar = default(char);

            for (int i = 0; i < source.Length; i++)
            {
                char currentChar = source[i];

                if (char.IsLower(previousChar) && // Previous char is lowercase
                    char.IsUpper(currentChar)) // Current char is uppercase
                {
                    result.Append(separator); // Append separator
                }
                result.Append(currentChar);

                previousChar = currentChar;
            }

            return result.ToString();
        }
    }
}
