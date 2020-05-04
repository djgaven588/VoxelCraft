﻿using OpenToolkit.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkBlockVertexData
    {
        public uint Data;

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(1, 0, VertexAttribIntegerType.UnsignedInt, false, sizeof(ChunkBlockVertexData), 0, true)
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("data")
        };
    }
}
