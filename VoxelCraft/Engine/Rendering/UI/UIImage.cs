using System.Numerics;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Rendering;
using VoxelCraft.Rendering.Standard;
using Color4 = OpenTK.Mathematics.Color4;

namespace VoxelCraft.Engine.Rendering.UI
{
    public class UIImage : UIElement
    {
        public UIImage(UIPosition position, int textureID, Color4 tint) : base(position)
        {
            Texture = textureID;
            material = new UIMaterial(StandardMaterials.WhiteText.ProgramID, Texture);
            material.ChangeColor(tint);
        }

        public int Texture;
        private readonly UIMaterial material;

        public override void Draw()
        {
            Graphics.QueueDraw(material, PrimitiveMeshes.CenteredQuad, Graphics.GetUIMatrix(Vector2.Zero, 25, Position));
        }
    }
}
