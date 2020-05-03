using OpenToolkit.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VoxelCraft
{
    public static class RenderDataHandler
    {
        private static readonly List<int> _storedVAOs = new List<int>();
        private static readonly List<int> _storedVBOs = new List<int>();
        private static readonly List<int> _storedTextures = new List<int>();

        /// <summary>
        /// Deletes all stored OpenGL references. Used for cleanup before exiting or changing scenes for instance.
        /// </summary>
        public static void ClearAllRenderData()
        {
            for (int i = 0; i < _storedVAOs.Count; i++)
            {
                DeleteVAO(_storedVAOs[i]);
            }
            _storedVAOs.Clear();

            for (int i = 0; i < _storedVBOs.Count; i++)
            {
                DeleteVBO(_storedVBOs[i]);
            }
            _storedVBOs.Clear();

            for (int i = 0; i < _storedTextures.Count; i++)
            {
                DeleteTexture(_storedTextures[i]);
            }
            _storedTextures.Clear();
        }

        /// <summary>
        /// Create a vertex array object (VAO)
        /// </summary>
        /// <returns>New VAO ID</returns>
        public static int GenerateVAO()
        {
            int id = GL.GenVertexArray();
            _storedVAOs.Add(id);
            return id;
        }

        /// <summary>
        /// Delete a vertex array object (VAO)
        /// </summary>
        /// <param name="id">VAO ID to delete</param>
        public static void DeleteVAO(int id)
        {
            _storedVAOs.Remove(id);
            GL.DeleteVertexArray(id);
        }

        /// <summary>
        /// Generates a vertex buffer object or generic buffer, your choice. (VBO)
        /// </summary>
        /// <returns></returns>
        public static int GenerateVBO()
        {
            int id = GL.GenBuffer();
            _storedVBOs.Add(id);
            return id;
        }

        /// <summary>
        /// Deletes a vertex buffer object or generic buffer, your choice. (VBO)
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteVBO(int id) 
        {
            GL.DeleteBuffer(id);
            _storedVBOs.Remove(id);
        }

        /// <summary>
        /// Loads a file from the specified path for use in rendering.
        /// </summary>
        /// <param name="filepath">The file path to the texture, starts at root directory. Supports PNGs, other types not tested.</param>
        /// <returns></returns>
        public static int LoadTexture(string filepath)
        {
            try
            {
                Bitmap bitmap = new Bitmap(filepath);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int textureID = GenerateTexture();
                GL.BindTexture(TextureTarget.Texture2D, textureID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenToolkit.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                bitmap.UnlockBits(data);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                bitmap.Dispose();

                return textureID;
            }
            catch (IOException e)
            {
                Debug.Log(e);
                return 0;
            }
        }

        /// <summary>
        /// Generates a texture, this texture is not initialized.
        /// </summary>
        /// <returns></returns>
        public static int GenerateTexture()
        {
            int textureID = GL.GenTexture();
            _storedTextures.Add(textureID);

            return textureID;
        }

        /// <summary>
        /// Deletes the provided texture
        /// </summary>
        /// <param name="texture"></param>
        public static void DeleteTexture(int texture)
        {
            GL.DeleteTexture(texture);
            _storedTextures.Remove(texture);
        }
    }
}
