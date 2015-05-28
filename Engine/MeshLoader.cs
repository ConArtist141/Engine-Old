using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Assimp;
using Assimp.Configs;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public class MeshLoader : IDisposable
    {
        protected AssimpContext importer = new AssimpContext();

        public MeshLoader()
        {
            NormalSmoothingAngleConfig config = new NormalSmoothingAngleConfig(66.0f);
            importer.SetConfig(config);
        }

        public Mesh LoadStaticMesh(string filePath)
        {
            return LoadStaticMesh(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);
        }

        public Mesh LoadStaticMesh(string filePath, PostProcessSteps postProcess)
        {
            Assimp.Scene model = importer.ImportFile(filePath, postProcess | PostProcessSteps.PreTransformVertices | PostProcessSteps.FlipUVs);

            if (model.Meshes.Exists(m => !m.HasVertices))
            {
                Debug.WriteLine("Warning - Mesh " + filePath + " does not have vertices!");
                return null;
            }

            if (model.Meshes.Exists(m => !m.HasNormals))
            {
                Debug.WriteLine("Warning - Mesh " + filePath + " does not have normals!");
                return null;
            }

            if (model.Meshes.Exists(m => m.TextureCoordinateChannelCount == 0))
            {
                Debug.WriteLine("Warning - Mesh " + filePath + " does not have texture coordinates!");
                return null;
            }

            if (model.Meshes.Exists(m => !m.HasFaces))
            {
                Debug.WriteLine("Warning - Mesh " + filePath + " does not have faces!");
                return null;
            }

            var indexOffsets = (from i in Enumerable.Range(0, model.Meshes.Count)
                                select model.Meshes.Take(i).Select(m => m.VertexCount).Sum()).ToArray();

            var vertices = from mesh in model.Meshes
                           from vert in mesh.Vertices
                           select new Vector3(vert.X, vert.Y, vert.Z);

            var uvs = from mesh in model.Meshes
                      from uv in mesh.TextureCoordinateChannels[0]
                      select new Vector2(uv.X, uv.Y);

            var normals = from mesh in model.Meshes
                          from normal in mesh.Normals
                          select new Vector3(normal.X, normal.Y, normal.Z);

            var indices = from i in Enumerable.Range(0, model.Meshes.Count)
                          let mesh = model.Meshes[i]
                          let offset = indexOffsets[i]
                          from face in mesh.Faces
                          where face.IndexCount == 3
                          from index in face.Indices
                          select (ushort)(index + offset);

            var vertexArray = vertices.ToArray();
            var uvArray = uvs.ToArray();
            var normalsArray = normals.ToArray();
            var indicesArray = indices.ToArray();

            var vertexPositionBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
            var vertexUVBuffer = new VertexBuffer(GL.GenBuffer(), 2, VertexAttribPointerType.Float);
            var vertexNormalBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexPositionBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertexArray.Length), vertexArray, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexUVBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * uvArray.Length), uvArray, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexNormalBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * normalsArray.Length), normalsArray, BufferUsageHint.StaticDraw);

            var indexBuffer = new IndexBuffer(GL.GenBuffer(), DrawElementsType.UnsignedShort);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indicesArray.Length), indicesArray, BufferUsageHint.StaticDraw);

            var outMesh = new Mesh();
            outMesh.IsIndexed = true;
            outMesh.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
            outMesh.PrimitiveCount = indicesArray.Length;
            outMesh.IndexBuffer = indexBuffer;
            outMesh.VertexAttributeBuffers = new[] { vertexPositionBuffer, vertexUVBuffer, vertexNormalBuffer };
            outMesh.BoundingBox = ComputeBoundingBox(vertexArray);

            return outMesh;
        }

        public BoundingBox ComputeBoundingBox(Vector3[] vertexArray)
        {
            Vector3 lower = new Vector3(Single.PositiveInfinity);
            Vector3 upper = new Vector3(Single.NegativeInfinity);
            Vector3 currentVec;

            foreach (var vec in vertexArray)
            {
                currentVec = vec;
                Vector3.ComponentMin(ref lower, ref currentVec, out lower);
                Vector3.ComponentMax(ref upper, ref currentVec, out upper);
            }

            return new BoundingBox()
            {
                Lower = lower,
                Upper = upper
            };
        }

        public void Dispose()
        {
            Debug.WriteLine("Disposing Mesh Importer...");

            importer.Dispose();
        }
    }
}