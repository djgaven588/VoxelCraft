using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace VoxelCraft.Rendering
{
    public class Material
    {
        public int ProgramID { get; private set; }

        public int ViewProjectionID { get; private set; }
        public int WorldTransform { get; private set; }

        protected int _textureID;

        public Material(int programID, int texture = 0)
        {
            ProgramID = programID;

            ViewProjectionID = GetUniformLocation("viewProjectionMatrix");
            WorldTransform = GetUniformLocation("worldTransformMatrix");

            _textureID = texture;
        }

        public int GetUniformLocation(string uniformName)
        {
            return GL.GetUniformLocation(ProgramID, uniformName);
        }

        public void LoadMatrix4(int location, Matrix4 matrix)
        {
            GL.ProgramUniformMatrix4(ProgramID, location, false, ref matrix);
        }

        public void LoadVector3(int location, Vector3 vector)
        {
            GL.ProgramUniform3(ProgramID, location, ref vector);
        }

        public virtual void BeforeRenderGroup()
        {
            if (_textureID != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _textureID);
            }
        }

        public virtual void BeforeRenderIndividual()
        {

        }

        public virtual void AfterRenderGroup()
        {

        }

        public virtual void AfterRenderIndividual()
        {

        }
    }
}
