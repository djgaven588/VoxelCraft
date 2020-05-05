using OpenToolkit.Mathematics;

namespace VoxelCraft.Rendering
{
    public static class PrimitiveMeshes
    {
        public static Mesh Quad { get; private set; }
        public static Mesh Skybox { get; private set; }

        public static void Init()
        {
            Quad = Mesh.GenerateMesh(StandardMeshVertexData.Attributes);

            Quad.UploadMeshData(new StandardMeshVertexData[] {
                new StandardMeshVertexData(new Vector3d(-0.5,  0.5, 0), new Vector2d(0, 0)),
                new StandardMeshVertexData(new Vector3d( 0.5,  0.5, 0), new Vector2d(1, 0)),
                new StandardMeshVertexData(new Vector3d( 0.5, -0.5, 0), new Vector2d(1, 1)),
                new StandardMeshVertexData(new Vector3d(-0.5, -0.5, 0), new Vector2d(0, 1))
            }, 4, new uint[] { 0, 1, 2, 2, 3, 0 }, 6);

            Skybox = Mesh.GenerateMesh(PositionOnlyMeshVertexData.Attributes);

            Skybox.UploadMeshData(new PositionOnlyMeshVertexData[] {
                // Front
                new PositionOnlyMeshVertexData(new Vector3d(-1,  1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1,  1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),

                // Back
                new PositionOnlyMeshVertexData(new Vector3d( 1,  1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1,  1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1, -1, 1)),

                // Left
                new PositionOnlyMeshVertexData(new Vector3d(-1,  1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1,  1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1,  1)),

                // Right
                new PositionOnlyMeshVertexData(new Vector3d(1,  1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(1,  1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, -1)),

                // Top
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1, 1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, -1)),

                // Bottom
                new PositionOnlyMeshVertexData(new Vector3d( 1, -1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1,  1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d( 1, -1, -1)),
            }, 24, new uint[] {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,

                8, 9, 10, 10, 11, 8,
                12, 13, 14, 14, 15, 12,

                16, 17, 18, 18, 19, 16,
                20, 21, 22, 22, 23, 20
            }, 36);
        }
    }
}
