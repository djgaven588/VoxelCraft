using System.Numerics;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.UI
{
    public class UIText : UIElement
    {
        private Mesh mesh;

        public string Text = "";
        private string _lastDisplayedText = "";

        public bool Outline = true;
        public Vector2 OutlineOffset = Vector2.One * 2;

        public UIText(FontData font, UIPosition position) : base(position)
        {
            Font = font;
        }

        public FontData Font { get { return _font; } set { _font = value; IsDirty = true; } }
        private FontData _font;

        public override void Draw()
        {
            if (Text != _lastDisplayedText)
            {
                _lastDisplayedText = Text;

                mesh = TextMeshGenerator.RegenerateMesh(_lastDisplayedText, _font, 6, 50, 18, 0.9f, mesh);
            }

            Graphics.QueueDraw(StandardMaterials.WhiteText, mesh, Graphics.GetUIMatrix(Vector2.Zero, Scale, Position));

            if (Outline)
            {
                Graphics.QueueDraw(StandardMaterials.BlackText, mesh, Graphics.GetUIMatrix(OutlineOffset, Scale, Position));
            }
        }

        ~UIText()
        {
            mesh.CleanUp();
        }
    }
}
