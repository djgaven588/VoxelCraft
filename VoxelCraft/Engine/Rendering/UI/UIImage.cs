using System.Numerics;
using System.Text;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Rendering;
using VoxelCraft.Rendering.Standard;

namespace VoxelCraft.Engine.Rendering.UI
{
    public class UIImage : UIElement
    {
        public UIImage(UIPosition position, int textureID, OpenToolkit.Mathematics.Color4 tint) : base(position)
        {
            Texture = textureID;
            material = new UIMaterial(StandardMaterials.WhiteText.ProgramID, Texture);
            material.ChangeColor(tint);
        }

        public int Texture;
        private UIMaterial material;

        public override void Draw()
        {
            Graphics.QueueDraw(material, PrimitiveMeshes.CenteredQuad, Graphics.GetUIMatrix(Vector2.Zero, 25, Position));
        }
    }
}
