namespace VoxelCraft
{
    public interface IUIMenu
    {
        bool ShouldDraw { get; }
        void Update(float timeDelta);
        void Draw();
    }
}
