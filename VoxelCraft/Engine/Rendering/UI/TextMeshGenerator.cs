using OpenTK.Mathematics;
using System.Linq;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering
{
    public static class TextMeshGenerator
    {
        public static Mesh RegenerateMesh(string text, FontData font, float fontSize, float lineHeight, int whiteSpaceAdvance = 18, float advanceScale = 0.9f, Mesh mesh = null)
        {
            if(mesh == null)
            {
                mesh = Mesh.GenerateMesh(UIVertexData.Attributes);
            }

            char[] chars = text.ToCharArray();
            int totalValidChars = chars.Where(e => e != '\n' || !font.characterData.ContainsKey(e)).Count();

            UIVertexData[] vertexData = new UIVertexData[totalValidChars * 4];
            uint[] triangles = new uint[totalValidChars * 6];

            float cursorPos = 0;
            int currentLine = 0;

            uint currentVertex = 0;
            uint currentTriangle = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\n')
                {
                    cursorPos = 0;
                    currentLine++;
                }
                else if (font.characterData.ContainsKey(chars[i]))
                {
                    FontData.CharacterData charData = font.characterData[chars[i]];
                    triangles[currentTriangle++] = currentVertex;
                    triangles[currentTriangle++] = currentVertex + 1;
                    triangles[currentTriangle++] = currentVertex + 2;
                    triangles[currentTriangle++] = currentVertex + 2;
                    triangles[currentTriangle++] = currentVertex + 3;
                    triangles[currentTriangle++] = currentVertex;

                    vertexData[currentVertex++] =
                        new UIVertexData(
                            new Vector3(cursorPos - charData.Offset.X + charData.Width, -currentLine * lineHeight - charData.Offset.Y, 0),
                            new Vector2(charData.BottomRight.X, charData.TopLeft.Y));

                    vertexData[currentVertex++] =
                        new UIVertexData(
                            new Vector3(cursorPos - charData.Offset.X + charData.Width, -currentLine * lineHeight - charData.Offset.Y - charData.Height, 0), 
                            charData.BottomRight);

                    vertexData[currentVertex++] =
                        new UIVertexData(
                            new Vector3(cursorPos - charData.Offset.X, -currentLine * lineHeight - charData.Offset.Y - charData.Height, 0),
                            new Vector2(charData.TopLeft.X, charData.BottomRight.Y));

                    vertexData[currentVertex++] =
                        new UIVertexData(
                            new Vector3(cursorPos - charData.Offset.X, -currentLine * lineHeight - charData.Offset.Y, 0),
                            charData.TopLeft);

                    cursorPos += charData.Advance * advanceScale;
                }
                else
                {
                    cursorPos += whiteSpaceAdvance;
                }
            }

            for (int i = 0; i < vertexData.Length; i++)
            {
                vertexData[i].Position *= fontSize / 10;
            }

            mesh.UploadMeshData(vertexData, (int)currentVertex, triangles, (int)currentTriangle);
            return mesh;
        }
    }
}
