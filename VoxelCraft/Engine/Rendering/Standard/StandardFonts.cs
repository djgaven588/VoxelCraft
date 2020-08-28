using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelCraft.Engine.Rendering.Standard
{
    public static class StandardFonts
    {
        public static FontData Arial { get; private set; }

        public static void Init()
        {
            Arial = new FontData("./Font/Arial.png", "./Font/Arial.fnt", 1024);
        }
    }
}
