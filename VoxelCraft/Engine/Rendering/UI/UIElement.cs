namespace VoxelCraft.Engine.Rendering.UI
{
    public class UIElement
    {
        public bool IsDirty { get; protected set; }

        public UIPosition Position;
        public float Scale = 1;

        public UIElement(UIPosition position)
        {
            Position = position;

            IsDirty = true;
        }

        public virtual void Draw()
        {

        }
    }
}
