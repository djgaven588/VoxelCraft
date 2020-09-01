using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoxelCraft.Engine.Rendering;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class DebugUI : IUIMenu
    {
        public bool ShouldDraw { get; private set; }

        private UIText mainText = new UIText(StandardFonts.Arial, new UIPosition(Vector2.Zero, Vector2.One * 48));

        public void AddDebugData(string data)
        {
            mainText.Text += data + '\n';
        }

        public void Clear()
        {
            mainText.Text = "";
        }

        public DebugUI()
        {

        }

        public void Draw()
        {
            // This should be checked by the caller, but needs to be checked anyways.
            if (!ShouldDraw)
            {
                return;
            }

            mainText.Draw();

            mainText.Text = "";
        }

        public void Update(float timeDelta)
        {
            if (InputManager.IsKeyNowDown(OpenToolkit.Windowing.Common.Input.Key.F3))
            {
                ShouldDraw = !ShouldDraw;
            }
        }
    }
}
