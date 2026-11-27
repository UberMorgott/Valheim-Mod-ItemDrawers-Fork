namespace ItemDrawers.Core
{
    /// <summary>
    /// Revision-gated decisions for per-frame drawer view/face synchronization.
    /// Idle drawers whose ZDO <c>DataRevision</c> has not moved skip ZDO field
    /// reads, container in-use checks, and ownership queries.
    /// </summary>
    public static class ViewSyncPolicy
    {
        public const uint UnloadedRevision = uint.MaxValue;

        /// <summary>
        /// Pre-check for <c>DrawerView.RefreshFromZdo</c> that compares
        /// <paramref name="zdoRevision"/> against <paramref name="loadedRevision"/>
        /// before touching <c>IsInUse</c> / <c>IsOwner</c>. A dirty view that has
        /// never loaded (<see cref="UnloadedRevision"/>) is still allowed through
        /// so its first load is never permanently blocked.
        /// </summary>
        public static bool ShouldCheckRefresh(bool isDirty, uint zdoRevision, uint loadedRevision)
        {
            if (isDirty && loadedRevision != UnloadedRevision) return false;
            return zdoRevision != loadedRevision;
        }

        /// <summary>True when the per-frame loop should ask the owner-side view whether a reconcile is needed.</summary>
        public static bool NeedsReconcileCheck(bool reconcileDeferred, bool viewDirty, uint zdoRevision, uint checkedRevision) =>
            reconcileDeferred || (!viewDirty && zdoRevision != checkedRevision);

        /// <summary>True when the per-frame loop should refresh a non-dirty view from its ZDO.</summary>
        public static bool NeedsViewRefresh(bool viewDirty, uint zdoRevision, uint loadedRevision) =>
            !viewDirty && zdoRevision != loadedRevision;

        /// <summary>True when the drawer face and item index should re-read the ZDO snapshot.</summary>
        public static bool NeedsFaceSync(uint zdoRevision, uint faceRevision) =>
            zdoRevision != faceRevision;
    }
}
