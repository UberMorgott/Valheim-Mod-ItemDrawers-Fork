using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RossPortals.Core.Tests
{
    public class ArchitectureTests
    {
        private static readonly string[] Forbidden =
            { "UnityEngine", "Assembly-CSharp", "assembly_valheim", "BepInEx", "Jotunn", "0Harmony" };

        // Core is testable precisely because it knows nothing about the game.
        // If this ever fails, the design has drifted, not the test.
        [Fact]
        public void Core_references_no_game_or_engine_assemblies()
        {
            var core = typeof(PortalListView).Assembly;
            var referenced = core.GetReferencedAssemblies();
            var violations = new List<string>();

            for (int i = 0; i < referenced.Length; i++)
            {
                string name = referenced[i].Name;
                for (int j = 0; j < Forbidden.Length; j++)
                {
                    if (name.StartsWith(Forbidden[j], StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(name);
                        break;
                    }
                }
            }

            Assert.Empty(violations);
        }

        // GetReferencedAssemblies() only reflects assemblies whose types the
        // compiled IL actually uses — a reference added "for later", before any
        // code consumes it, would pass the check above silently. This inspects
        // the csproj text directly so that blind spot is covered too.
        [Fact]
        public void Core_csproj_declares_no_game_or_engine_references()
        {
            var csprojPath = FindCoreCsproj();
            Assert.True(File.Exists(csprojPath), $"Could not locate RossPortals.Core.csproj to inspect (looked at: {csprojPath}).");

            var text = File.ReadAllText(csprojPath);
            var violations = new List<string>();

            for (int i = 0; i < Forbidden.Length; i++)
            {
                if (text.Contains(Forbidden[i], StringComparison.OrdinalIgnoreCase))
                    violations.Add(Forbidden[i]);
            }

            Assert.Empty(violations);
        }

        // Walks up from the test assembly's run location to the mod's own
        // directory (identified by RossPortals.sln), then to the Core csproj.
        // A relative path from bin/Debug/net8.0/... would break the moment the
        // build layout changes; this survives that.
        private static string FindCoreCsproj()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RossPortals.sln")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
            {
                return Path.Combine(AppContext.BaseDirectory, "<RossPortals.sln not found while walking up from here>");
            }

            return Path.Combine(dir.FullName, "src", "RossPortals.Core", "RossPortals.Core.csproj");
        }
    }
}
