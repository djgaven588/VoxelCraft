using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using Color4 = OpenTK.Mathematics.Color4;

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

        public void LoadMatrix4(int location, Matrix4x4 matrix)
        {
            float[] matrixTemp = new float[16];
            matrixTemp[0] = matrix.M11;
            matrixTemp[1] = matrix.M12;
            matrixTemp[2] = matrix.M13;
            matrixTemp[3] = matrix.M14;
            matrixTemp[4] = matrix.M21;
            matrixTemp[5] = matrix.M22;
            matrixTemp[6] = matrix.M23;
            matrixTemp[7] = matrix.M24;
            matrixTemp[8] = matrix.M31;
            matrixTemp[9] = matrix.M32;
            matrixTemp[10] = matrix.M33;
            matrixTemp[11] = matrix.M34;
            matrixTemp[12] = matrix.M41;
            matrixTemp[13] = matrix.M42;
            matrixTemp[14] = matrix.M43;
            matrixTemp[15] = matrix.M44;
            GL.ProgramUniformMatrix4(ProgramID, location, 1, false, matrixTemp);
        }

        public void LoadVector3(int location, Vector3 vector)
        {
            GL.ProgramUniform3(ProgramID, location, vector.X, vector.Y, vector.Z);
        }

        public void LoadColor4(int location, Color4 color)
        {
            GL.ProgramUniform4(ProgramID, location, color.R, color.G, color.B, color.A);
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

        public override bool Equals(object obj)
        {
            return obj is Material material &&
                   ProgramID == material.ProgramID &&
                   _textureID == material._textureID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProgramID, _textureID);
        }
    }
}
