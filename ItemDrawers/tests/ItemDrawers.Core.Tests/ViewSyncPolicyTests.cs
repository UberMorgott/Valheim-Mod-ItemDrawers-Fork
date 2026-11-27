using Xunit;

namespace ItemDrawers.Core.Tests
{
    public class ViewSyncPolicyTests
    {
        [Fact]
        public void Unchanged_revision_skips_refresh_reconcile_and_face_sync()
        {
            const uint rev = 42u;
            Assert.False(ViewSyncPolicy.ShouldCheckRefresh(isDirty: false, zdoRevision: rev, loadedRevision: rev));
            Assert.False(ViewSyncPolicy.NeedsReconcileCheck(reconcileDeferred: false, viewDirty: false, zdoRevision: rev, checkedRevision: rev));
            Assert.False(ViewSyncPolicy.NeedsViewRefresh(viewDirty: false, zdoRevision: rev, loadedRevision: rev));
            Assert.False(ViewSyncPolicy.NeedsFaceSync(zdoRevision: rev, faceRevision: rev));
        }

        [Fact]
        public void Changed_revision_triggers_refresh_reconcile_and_face_sync()
        {
            Assert.True(ViewSyncPolicy.ShouldCheckRefresh(isDirty: false, zdoRevision: 43u, loadedRevision: 42u));
            Assert.True(ViewSyncPolicy.NeedsReconcileCheck(reconcileDeferred: false, viewDirty: false, zdoRevision: 43u, checkedRevision: 42u));
            Assert.True(ViewSyncPolicy.NeedsViewRefresh(viewDirty: false, zdoRevision: 43u, loadedRevision: 42u));
            Assert.True(ViewSyncPolicy.NeedsFaceSync(zdoRevision: 43u, faceRevision: 42u));
        }

        [Fact]
        public void Dirty_loaded_view_blocks_refresh_but_dirty_unloaded_view_still_loads_once()
        {
            Assert.False(ViewSyncPolicy.ShouldCheckRefresh(isDirty: true, zdoRevision: 10u, loadedRevision: 9u));
            Assert.True(ViewSyncPolicy.ShouldCheckRefresh(isDirty: true, zdoRevision: 10u, loadedRevision: ViewSyncPolicy.UnloadedRevision));
        }

        [Fact]
        public void Deferred_reconcile_still_runs_even_when_revision_did_not_change()
        {
            Assert.True(ViewSyncPolicy.NeedsReconcileCheck(reconcileDeferred: true, viewDirty: false, zdoRevision: 42u, checkedRevision: 42u));
            Assert.True(ViewSyncPolicy.NeedsReconcileCheck(reconcileDeferred: true, viewDirty: true, zdoRevision: 42u, checkedRevision: 42u));
        }

        [Fact]
        public void Dirty_view_skips_per_frame_refresh_and_non_deferred_reconcile()
        {
            Assert.False(ViewSyncPolicy.NeedsViewRefresh(viewDirty: true, zdoRevision: 43u, loadedRevision: 42u));
            Assert.False(ViewSyncPolicy.NeedsReconcileCheck(reconcileDeferred: false, viewDirty: true, zdoRevision: 43u, checkedRevision: 42u));
        }
    }
}
