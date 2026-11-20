using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ItemDrawers.Core.Tests
{
    public class DrawerTextsTests
    {
        [Fact]
        public void English_and_Russian_cover_the_same_tokens_with_text()
        {
            Assert.Equal(DrawerTexts.English.Keys.OrderBy(k => k), DrawerTexts.Russian.Keys.OrderBy(k => k));
            Assert.All(DrawerTexts.English.Values.Concat(DrawerTexts.Russian.Values), v => Assert.False(string.IsNullOrWhiteSpace(v)));
        }

        // An untranslated token shows the player "[itemdrawers_x]", so every token the sources use must exist.
        [Fact]
        public void Every_token_used_in_source_is_translated()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ItemDrawers.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);

            var used = Directory.GetFiles(Path.Combine(dir!.FullName, "src"), "*.cs", SearchOption.AllDirectories)
                .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"\$(itemdrawers_\w+)").Select(m => m.Groups[1].Value))
                .Distinct().ToArray();

            Assert.NotEmpty(used);
            Assert.Empty(used.Except(DrawerTexts.English.Keys));
            Assert.Empty(DrawerTexts.English.Keys.Except(used));
        }
    }
}
