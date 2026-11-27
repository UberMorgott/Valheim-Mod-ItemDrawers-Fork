using Xunit;

namespace ItemDrawers.Core.Tests
{
    public class LabelVisibilityTests
    {
        [Fact]
        public void Front_facing_within_distance_is_visible()
        {
            // Drawer face at origin facing +Z, camera 10 m in front.
            Assert.True(LabelVisibility.IsVisible(
                eyeX: 0f, eyeY: 0f, eyeZ: 10f,
                faceX: 0f, faceY: 0f, faceZ: 0f,
                frontNormalX: 0f, frontNormalY: 0f, frontNormalZ: 1f,
                maxDistanceSq: 30f * 30f));
        }

        [Fact]
        public void Beyond_label_distance_is_hidden_even_when_front_facing()
        {
            Assert.False(LabelVisibility.IsVisible(
                eyeX: 0f, eyeY: 0f, eyeZ: 45f,
                faceX: 0f, faceY: 0f, faceZ: 0f,
                frontNormalX: 0f, frontNormalY: 0f, frontNormalZ: 1f,
                maxDistanceSq: 30f * 30f));
        }

        [Fact]
        public void Strictly_behind_drawer_wall_is_backface_culled()
        {
            // Camera 2 m behind a drawer whose front faces +Z.
            Assert.False(LabelVisibility.IsVisible(
                eyeX: 0f, eyeY: 0f, eyeZ: -2f,
                faceX: 0f, faceY: 0f, faceZ: 0f,
                frontNormalX: 0f, frontNormalY: 0f, frontNormalZ: 1f,
                maxDistanceSq: 30f * 30f));
        }

        [Fact]
        public void Grazing_angle_in_front_or_within_corner_margin_stays_visible()
        {
            // Almost edge-on in front (+0.01 m in front, 10 m sideways).
            Assert.True(LabelVisibility.IsVisible(
                eyeX: 10f, eyeY: 0f, eyeZ: 0.01f,
                faceX: 0f, faceY: 0f, faceZ: 0f,
                frontNormalX: 0f, frontNormalY: 0f, frontNormalZ: 1f,
                maxDistanceSq: 30f * 30f));

            // Slightly behind the label plane within BackfaceMargin (rounding a corner).
            Assert.True(LabelVisibility.IsVisible(
                eyeX: 2f, eyeY: 0f, eyeZ: -(LabelVisibility.BackfaceMargin - 0.05f),
                faceX: 0f, faceY: 0f, faceZ: 0f,
                frontNormalX: 0f, frontNormalY: 0f, frontNormalZ: 1f,
                maxDistanceSq: 30f * 30f));
        }

        [Fact]
        public void ComputeTargets_never_activates_count_when_hidden_empty_or_unconfigured()
        {
            LabelVisibility.ComputeTargets(labelVisible: true, hasIcon: true, shownAmount: 42, countTextConfigured: true,
                out bool showIcon, out bool showCount);
            Assert.True(showIcon);
            Assert.True(showCount);

            // Culled by distance/backface: both off.
            LabelVisibility.ComputeTargets(labelVisible: false, hasIcon: true, shownAmount: 42, countTextConfigured: true,
                out showIcon, out showCount);
            Assert.False(showIcon);
            Assert.False(showCount);

            // Assigned but empty (0 items): icon shown, count stays deactivated.
            LabelVisibility.ComputeTargets(labelVisible: true, hasIcon: true, shownAmount: 0, countTextConfigured: true,
                out showIcon, out showCount);
            Assert.True(showIcon);
            Assert.False(showCount);

            // Font not configured yet: count child must stay inactive so TMP.Awake doesn't warn.
            LabelVisibility.ComputeTargets(labelVisible: true, hasIcon: true, shownAmount: 42, countTextConfigured: false,
                out showIcon, out showCount);
            Assert.True(showIcon);
            Assert.False(showCount);

            // Unassigned drawer: neither icon nor count shown.
            LabelVisibility.ComputeTargets(labelVisible: true, hasIcon: false, shownAmount: 42, countTextConfigured: true,
                out showIcon, out showCount);
            Assert.False(showIcon);
            Assert.False(showCount);
        }
    }
}
