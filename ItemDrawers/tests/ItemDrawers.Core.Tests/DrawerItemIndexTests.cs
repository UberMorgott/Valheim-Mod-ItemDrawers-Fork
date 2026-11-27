using System;
using System.Collections.Generic;
using Xunit;

namespace ItemDrawers.Core.Tests
{
    public class DrawerItemIndexTests
    {
        [Fact]
        public void Finds_only_drawers_assigned_to_matching_item_within_radius()
        {
            var index = new DrawerItemIndex<string>();
            index.Upsert("wood-near", "Wood", 5f, 0f, 0f);
            index.Upsert("wood-far", "Wood", 50f, 0f, 0f);
            index.Upsert("stone-near", "Stone", 1f, 0f, 0f);
            index.Upsert("unassigned-near", "", 0f, 0f, 0f);

            var hits = new List<string>();
            index.QueryNear("Wood", 0f, 0f, 0f, 40f, hits);

            Assert.Equal(new[] { "wood-near" }, hits);
            Assert.Equal(4, index.Count);
            Assert.Equal(2, index.DistinctItemCount);
        }

        [Fact]
        public void Unmatched_item_returns_nothing()
        {
            var index = new DrawerItemIndex<string>();
            index.Upsert("d1", "Wood", 0f, 0f, 0f);

            var hits = new List<string>();
            index.QueryNear("Iron", 0f, 0f, 0f, 40f, hits);

            Assert.Empty(hits);
        }

        [Fact]
        public void SetItem_reindexes_and_drops_empty_buckets()
        {
            var index = new DrawerItemIndex<string>();
            index.Upsert("d1", "Wood", 2f, 0f, 0f);

            index.SetItem("d1", "Stone");
            Assert.Equal(1, index.DistinctItemCount);

            var woodHits = new List<string>();
            index.QueryNear("Wood", 0f, 0f, 0f, 10f, woodHits);
            Assert.Empty(woodHits);

            var stoneHits = new List<string>();
            index.QueryNear("Stone", 0f, 0f, 0f, 10f, stoneHits);
            Assert.Equal(new[] { "d1" }, stoneHits);

            // Clearing unassigns the drawer while keeping its cached position for later reassignment.
            index.SetItem("d1", "");
            Assert.Equal(0, index.DistinctItemCount);
            Assert.Equal(1, index.Count);

            index.SetItem("d1", "Iron");
            var ironHits = new List<string>();
            index.QueryNear("Iron", 0f, 0f, 0f, 10f, ironHits);
            Assert.Equal(new[] { "d1" }, ironHits);
        }

        [Fact]
        public void Remove_drops_drawer_from_item_bucket_and_radius_check()
        {
            var index = new DrawerItemIndex<string>();
            index.Upsert("d1", "Wood", 1f, 0f, 0f);
            index.Upsert("d2", "Wood", 2f, 0f, 0f);

            Assert.True(index.Remove("d1"));
            Assert.False(index.Remove("d1"));

            var hits = new List<string>();
            index.QueryNear("Wood", 0f, 0f, 0f, 10f, hits);
            Assert.Equal(new[] { "d2" }, hits);
            Assert.False(index.IsWithinRadius("d1", 0f, 0f, 0f, 10f));
            Assert.True(index.IsWithinRadius("d2", 0f, 0f, 0f, 10f));
        }

        [Fact]
        public void IsWithinRadius_checks_spherical_distance_against_cached_position()
        {
            var index = new DrawerItemIndex<string>();
            index.Upsert("d1", "Wood", 3f, 4f, 0f); // distance 5 from origin

            Assert.True(index.IsWithinRadius("d1", 0f, 0f, 0f, 5f));
            Assert.False(index.IsWithinRadius("d1", 0f, 0f, 0f, 4.99f));
            Assert.False(index.IsWithinRadius("d1", 0f, 0f, 0f, 0f));
            Assert.False(index.IsWithinRadius("missing", 0f, 0f, 0f, 100f));
        }

        [Fact]
        public void Agrees_with_brute_force_over_random_assignments_and_reassignments()
        {
            var rng = new Random(20261128);
            var items = new[] { "", "Wood", "Stone", "Iron", "Coal", "FineWood", "BlackMarble" };
            var index = new DrawerItemIndex<int>();
            var expected = new Dictionary<int, (string Item, float X, float Y, float Z)>();

            for (int i = 0; i < 400; i++)
            {
                string item = items[rng.Next(items.Length)];
                float x = (float)(rng.NextDouble() * 100 - 50);
                float y = (float)(rng.NextDouble() * 20 - 10);
                float z = (float)(rng.NextDouble() * 100 - 50);
                index.Upsert(i, item, x, y, z);
                expected[i] = (item, x, y, z);
            }

            // Reassign and remove a mix of drawers.
            for (int op = 0; op < 200; op++)
            {
                int id = rng.Next(400);
                if (!expected.ContainsKey(id)) continue;

                if (rng.Next(4) == 0)
                {
                    index.Remove(id);
                    expected.Remove(id);
                }
                else
                {
                    string nextItem = items[rng.Next(items.Length)];
                    index.SetItem(id, nextItem);
                    var cur = expected[id];
                    expected[id] = (nextItem, cur.X, cur.Y, cur.Z);
                }
            }

            var hits = new List<int>();
            var brute = new List<int>();
            for (int q = 0; q < 50; q++)
            {
                string item = items[1 + rng.Next(items.Length - 1)];
                float qx = (float)(rng.NextDouble() * 100 - 50);
                float qy = (float)(rng.NextDouble() * 20 - 10);
                float qz = (float)(rng.NextDouble() * 100 - 50);
                float r = 40f;
                float r2 = r * r;

                hits.Clear();
                index.QueryNear(item, qx, qy, qz, r, hits);
                hits.Sort();

                brute.Clear();
                foreach (var kv in expected)
                {
                    if (!string.Equals(kv.Value.Item, item, StringComparison.Ordinal)) continue;
                    float dx = kv.Value.X - qx, dy = kv.Value.Y - qy, dz = kv.Value.Z - qz;
                    if (dx * dx + dy * dy + dz * dz <= r2) brute.Add(kv.Key);
                }
                brute.Sort();

                Assert.Equal(brute, hits);
            }
        }
    }
}
