using System;
using System.Collections.Generic;
using Morgott.Core;
using UnityEngine;

namespace ItemDrawers.Game
{
    /// <summary>
    /// Morgott.Core's IDrawerStore: the typed surface Morgott.Patcher uses
    /// instead of reaching these members by name (a renamed or re-signed
    /// member used to fail only at runtime, with a warning). Every call maps
    /// onto the drawer's own path, so the RPC protocol, its pinning and its
    /// clamps stay exactly what a player's own use runs.
    /// </summary>
    internal sealed class DrawerStore : IDrawerStore, IDrawerDeposit
    {
        public bool TryDeposit(Container drawer, string item, int amount, out int accepted) =>
            Drawer(drawer).TryDepositExternally(item, amount, out accepted);

        internal static DrawerStore Instance;

        internal static Func<ItemDrop, Container, Container> Gate => Instance?.PickupGate;

        private readonly List<DrawerComponent> _near = new List<DrawerComponent>(16);

        public event Action<Container, string, int> WithdrawGranted;

        public Func<ItemDrop, Container, Container> PickupGate { get; set; }

        public bool IsDrawer(Container container) => container is DrawerComponent;

        public string Item(Container drawer) => Drawer(drawer).Snapshot.ItemName;

        public int Amount(Container drawer) => Drawer(drawer).Snapshot.Amount;

        public int Capacity(Container drawer) => Drawer(drawer).Capacity;

        public Vector3 Face(Container drawer)
        {
            var face = Drawer(drawer).Face;
            return face != null ? face.transform.position : drawer.transform.position;
        }

        public void Near(Vector3 at, float radius, List<Container> into)
        {
            var manager = DrawerManager.Instance;
            if (manager == null) return;
            _near.Clear();
            manager.QueryNear(at, radius, _near);
            foreach (var drawer in _near)
                if (drawer != null) into.Add(drawer);
        }

        public bool OwnershipSettled(Container drawer) => Drawer(drawer).OwnershipSettled();

        public float OwnerAge(Container drawer) => Drawer(drawer).OwnerRevisionAge();

        public bool TryWithdraw(Container drawer, int requested, out int taken) => Drawer(drawer).TryWithdrawExternally(requested, out taken);

        public void PlayerWithdraw(Container drawer, Player player, string item, int amount) => Drawer(drawer).RequestWithdraw(player, item, amount);

        public void PlayerDeposit(Container drawer, Player player, string item, int removed) =>
            Drawer(drawer).RequestDeposit(player, item, removed, announce: false);

        public void GiveToPlayer(Player player, string item, int amount) => ItemFacts.GiveToPlayer(player, item, amount);

        public int PendingWithdrawals => DrawerManager.Instance != null ? DrawerManager.Instance.PendingWithdrawalCount : 0;

        public long RequestWithdraw(Container drawer, Player player, string item, int amount, Action<long, string, int> granted, Action<long> givenUp)
        {
            var d = Drawer(drawer);
            var view = d.m_nview;
            var manager = DrawerManager.Instance;
            if (manager == null || player == null || view == null || !view.IsValid() || view.IsOwner()) return 0;
            long owner = view.GetZDO().GetOwner();
            // Owner 0 routes as Everybody (see RequestWithdraw): never sent.
            return owner == 0L ? 0 : d.SendForeignWithdraw(manager, owner, player, item, amount, granted, givenUp);
        }

        public void RunAutoPickup() => DrawerManager.Instance?.RunAutoPickup();

        internal static void Granted(DrawerComponent drawer, string item, int amount)
        {
            var handlers = Instance?.WithdrawGranted;
            if (handlers != null) Reply(() => handlers(drawer, item, amount));
        }

        // Give-ups fire from inside PendingRequestLedger.Tick / ResolvePendingForDrawer, neither of which a callback may
        // re-enter (a reply resolving another due id would throw there): run after them, from DrawerManager.Update.
        private static readonly List<Action> Later = new List<Action>();

        internal static void ReplyLater(Action reply) => Later.Add(reply);

        internal static void FlushLater()
        {
            if (Later.Count == 0) return;
            var batch = Later.ToArray();
            Later.Clear();
            foreach (var reply in batch) Reply(reply);
        }

        /// <summary>A consumer's callback never breaks the drawer's own flow (RPC handler, ledger tick).</summary>
        internal static void Reply(Action reply)
        {
            try { reply(); }
            catch (Exception e) { DrawerPlugin.Log.LogWarning($"IDrawerStore callback failed: {e.GetBaseException().Message}"); }
        }

        private static DrawerComponent Drawer(Container drawer) =>
            drawer as DrawerComponent ?? throw new ArgumentException("not an ItemDrawers drawer", nameof(drawer));
    }
}
