namespace ItemDrawers.Core
{
    /// <summary>
    /// Distance and backface visibility checks for drawer labels, plus the
    /// resolved icon/count active targets so hidden drawers skip rendering and
    /// culling altogether.
    /// </summary>
    public static class LabelVisibility
    {
        // Conservative margin behind the label plane (in metres) so a player
        // rounding the edge of a drawer wall never sees labels pop in late
        // between 0.5 s culling ticks, while drawers behind a wall or facing
        // away still cull cleanly.
        public const float BackfaceMargin = 0.25f;

        /// <summary>
        /// True when the camera at <c>(eyeX, eyeY, eyeZ)</c> is within
        /// <paramref name="maxDistanceSq"/> of the drawer's label face and not
        /// strictly behind its outward <c>(frontNormalX, frontNormalY, frontNormalZ)</c> plane.
        /// </summary>
        public static bool IsVisible(
            float eyeX, float eyeY, float eyeZ,
            float faceX, float faceY, float faceZ,
            float frontNormalX, float frontNormalY, float frontNormalZ,
            float maxDistanceSq)
        {
            if (maxDistanceSq < 0f) return false;

            float dx = eyeX - faceX;
            float dy = eyeY - faceY;
            float dz = eyeZ - faceZ;
            if (dx * dx + dy * dy + dz * dz > maxDistanceSq) return false;

            float dot = dx * frontNormalX + dy * frontNormalY + dz * frontNormalZ;
            return dot >= -BackfaceMargin;
        }

        /// <summary>
        /// Resolves whether the icon renderer and count TMP object should be
        /// active. Count is never activated before its font has been configured
        /// (activating it early would run TextMeshPro.Awake with no font).
        /// </summary>
        public static void ComputeTargets(
            bool labelVisible,
            bool hasIcon,
            int shownAmount,
            bool countTextConfigured,
            out bool showIcon,
            out bool showCount)
        {
            showIcon = labelVisible && hasIcon;
            showCount = showIcon && shownAmount > 0 && countTextConfigured;
        }
    }
}
