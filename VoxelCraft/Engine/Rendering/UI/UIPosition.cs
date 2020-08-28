using System.Numerics;

namespace VoxelCraft.Engine.Rendering.UI
{
    /// <summary>
    /// The UI Anchor class is the backbone of dynamic UI elements
    /// </summary>
    public class UIPosition
    {
        public UIPosition(Vector2 center, Vector2 offset)
        {
            Center = center;
            Offset = offset;
        }

        /// <summary>
        /// The center of a UI position is a value from 0 - 1.
        /// It is based on screen percentage, so 0.5 0.5 is center of the screen.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// The offset of a UI position.
        /// It is pixel based, considering the canvas scaling system.
        /// </summary>
        public Vector2 Offset;
    }
}
