using System;
using System.Collections.Generic;

namespace ItemDrawers.Core
{
    /// <summary>
    /// Decides which build cost a drawer actually gets: the configured one,
    /// or the shipped default with a reason when the configured one cannot
    /// be used.
    ///
    /// Lives in Core because it is now asked twice in a session -- once at
    /// registration from the player's own config file, and again whenever a
    /// server's config arrives (or is taken away again on disconnect) -- and
    /// both answers have to come out the same way for the same text.
    /// </summary>
    public static class RecipeChoice
    {
        /// <param name="configured">The config entry's text, as edited by a person.</param>
        /// <param name="fallback">The shipped default for this tier.</param>
        /// <param name="isKnownItem">
        /// Whether the game knows an item by this prefab name, or null when
        /// nothing can answer yet. Null lets the recipe through rather than
        /// treating "cannot check" as "does not exist" -- reading an empty
        /// database as a typo once rejected every recipe, defaults included.
        /// </param>
        /// <returns>
        /// The requirements to use, and null or the reason the configured
        /// text was set aside for <paramref name="fallback"/>.
        /// </returns>
        public static (IReadOnlyList<(string Item, int Amount)> Requirements, string Problem) Choose(
            string configured,
            IReadOnlyList<(string Item, int Amount)> fallback,
            Func<string, bool> isKnownItem)
        {
            var spec = RecipeSpec.Parse(configured);
            if (!spec.Ok) return (fallback, spec.Error);

            if (isKnownItem != null)
            {
                for (int i = 0; i < spec.Requirements.Count; i++)
                {
                    var r = spec.Requirements[i];
                    if (!isKnownItem(r.Item))
                        return (fallback, $"no item named '{r.Item}'");
                }
            }

            return (spec.Requirements, null);
        }
    }
}
