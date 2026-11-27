using System;
using System.Text;

namespace RossQoL.Core.Crafting
{
    /// <summary>
    /// Whether a recipe's displayed name matches the crafting search box.
    /// Case- and space-insensitive, the same as vanilla's build menu search,
    /// so "bronzeaxe" and "Bronze Axe" both find the Bronze axe.
    /// </summary>
    public static class RecipeSearch
    {
        public static bool IsActive(string term) => Normalize(term).Length > 0;

        /// <param name="displayName">The localized name as shown in the list.</param>
        public static bool Matches(string displayName, string term)
        {
            string needle = Normalize(term);
            if (needle.Length == 0) return true;
            if (displayName == null) return false;

            return Normalize(displayName).Contains(needle, StringComparison.Ordinal);
        }

        /// <summary>
        /// Matches the displayed name or any of the recipe's category words
        /// (item type words and weapon skill), with the same rules.
        /// </summary>
        public static bool Matches(string displayName, System.Collections.Generic.IEnumerable<string> categoryWords, string term)
        {
            if (Matches(displayName, term)) return true;
            if (categoryWords == null) return false;

            string needle = Normalize(term);
            var list = categoryWords as System.Collections.Generic.IReadOnlyList<string>
                ?? new System.Collections.Generic.List<string>(categoryWords);
            for (int i = 0; i < list.Count; i++)
            {
                string word = list[i];
                if (word != null && Normalize(word).Contains(needle, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
                if (!char.IsWhiteSpace(c))
                    sb.Append(char.ToLowerInvariant(c));
            return sb.ToString();
        }
    }
}
