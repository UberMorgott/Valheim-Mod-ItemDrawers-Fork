using System;
using System.Collections.Generic;

namespace ItemDrawers.Core
{
    /// <summary>
    /// O(1) lookup of drawers by assigned item prefab name, with cached world
    /// positions. Used by auto-pickup so each ground drop checks only drawers
    /// assigned to that exact item rather than scanning every spatial cell in a
    /// 40 m radius and reading every nearby drawer's ZDO.
    /// </summary>
    public sealed class DrawerItemIndex<T>
    {
        private readonly struct DrawerEntry
        {
            public readonly string ItemName;
            public readonly float X, Y, Z;

            public DrawerEntry(string itemName, float x, float y, float z)
            {
                ItemName = itemName;
                X = x;
                Y = y;
                Z = z;
            }
        }

        private readonly struct BucketEntry
        {
            public readonly T Drawer;
            public readonly float X, Y, Z;

            public BucketEntry(T drawer, float x, float y, float z)
            {
                Drawer = drawer;
                X = x;
                Y = y;
                Z = z;
            }
        }

        private readonly Dictionary<string, List<BucketEntry>> _byItem =
            new Dictionary<string, List<BucketEntry>>(StringComparer.Ordinal);
        private readonly Dictionary<T, DrawerEntry> _drawers = new Dictionary<T, DrawerEntry>();

        public int Count => _drawers.Count;

        public int DistinctItemCount => _byItem.Count;

        public void Upsert(T drawer, string itemName, float x, float y, float z)
        {
            string normalized = string.IsNullOrEmpty(itemName) ? "" : itemName;
            if (_drawers.TryGetValue(drawer, out var existing))
            {
                bool sameItem = string.Equals(existing.ItemName, normalized, StringComparison.Ordinal);
                bool samePos = existing.X == x && existing.Y == y && existing.Z == z;
                if (sameItem && samePos) return;

                if (existing.ItemName.Length > 0) RemoveFromBucket(existing.ItemName, drawer);
            }

            _drawers[drawer] = new DrawerEntry(normalized, x, y, z);
            if (normalized.Length > 0) AddToBucket(normalized, drawer, x, y, z);
        }

        public void SetItem(T drawer, string itemName)
        {
            if (!_drawers.TryGetValue(drawer, out var existing)) return;

            string normalized = string.IsNullOrEmpty(itemName) ? "" : itemName;
            if (string.Equals(existing.ItemName, normalized, StringComparison.Ordinal)) return;

            if (existing.ItemName.Length > 0) RemoveFromBucket(existing.ItemName, drawer);
            _drawers[drawer] = new DrawerEntry(normalized, existing.X, existing.Y, existing.Z);
            if (normalized.Length > 0) AddToBucket(normalized, drawer, existing.X, existing.Y, existing.Z);
        }

        public bool Remove(T drawer)
        {
            if (!_drawers.TryGetValue(drawer, out var existing)) return false;

            if (existing.ItemName.Length > 0) RemoveFromBucket(existing.ItemName, drawer);
            _drawers.Remove(drawer);
            return true;
        }

        /// <summary>Appends every drawer assigned to <paramref name="itemName"/> within <paramref name="radius"/>. Does not clear the list.</summary>
        public void QueryNear(string itemName, float x, float y, float z, float radius, List<T> results)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(results);
#else
            if (results == null) throw new ArgumentNullException(nameof(results));
#endif
            if (radius <= 0f || string.IsNullOrEmpty(itemName)) return;
            if (!_byItem.TryGetValue(itemName, out var bucket)) return;

            float r2 = radius * radius;
            for (int i = 0; i < bucket.Count; i++)
            {
                var entry = bucket[i];
                float dx = entry.X - x;
                float dy = entry.Y - y;
                float dz = entry.Z - z;
                if (dx * dx + dy * dy + dz * dz <= r2) results.Add(entry.Drawer);
            }
        }

        /// <summary>True when <paramref name="drawer"/> is indexed and within <paramref name="radius"/> of the given point.</summary>
        public bool IsWithinRadius(T drawer, float x, float y, float z, float radius)
        {
            if (radius <= 0f || !_drawers.TryGetValue(drawer, out var entry)) return false;

            float dx = entry.X - x;
            float dy = entry.Y - y;
            float dz = entry.Z - z;
            return dx * dx + dy * dy + dz * dz <= radius * radius;
        }

        private void AddToBucket(string itemName, T drawer, float x, float y, float z)
        {
            if (!_byItem.TryGetValue(itemName, out var bucket))
            {
                bucket = new List<BucketEntry>(2);
                _byItem[itemName] = bucket;
            }
            bucket.Add(new BucketEntry(drawer, x, y, z));
        }

        private void RemoveFromBucket(string itemName, T drawer)
        {
            if (!_byItem.TryGetValue(itemName, out var bucket)) return;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < bucket.Count; i++)
            {
                if (!comparer.Equals(bucket[i].Drawer, drawer)) continue;

                int last = bucket.Count - 1;
                if (i < last) bucket[i] = bucket[last];
                bucket.RemoveAt(last);
                if (bucket.Count == 0) _byItem.Remove(itemName);
                return;
            }
        }
    }
}
