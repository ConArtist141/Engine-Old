using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Engine
{
    /// <summary>
    /// A class which represents a mesh object composed of multiple vertex buffers and an optional index buffer
    /// </summary>
    public class Mesh : IDisposable
    {
        public VertexBuffer[] VertexAttributeBuffers;
        public IndexBuffer IndexBuffer;
        public bool IsIndexed;
        public PrimitiveType PrimitiveType;
        public int PrimitiveCount;
        public BoundingBox BoundingBox;

        /// <summary>
        /// Enable this mesh for rendering
        /// </summary>
        public void Enable()
        {
            // Bind each attribute buffer
            for (int i = 0, length = VertexAttributeBuffers.Length; i < length; ++i)
            { 
                GL.EnableVertexAttribArray(i);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexAttributeBuffers[i].Handle);
                GL.VertexAttribPointer(i, VertexAttributeBuffers[i].ComponentsPerAttribute,
                    VertexAttributeBuffers[i].AttributeType, VertexAttributeBuffers[i].ShouldNormalize,
                    VertexAttributeBuffers[i].Stride, VertexAttributeBuffers[i].Offset);
            }

            // Bind the index buffer if necessary
            if (IsIndexed)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.Handle);
        }

        /// <summary>
        /// Enable this mesh for instanced rendering
        /// </summary>
        public void EnableInstanced(int instanceBuffer)
        {
            // Bind each attribute buffer
            for (int i = 0, length = VertexAttributeBuffers.Length; i < length; ++i)
            {
                GL.EnableVertexAttribArray(i);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexAttributeBuffers[i].Handle);
                GL.VertexAttribPointer(i, VertexAttributeBuffers[i].ComponentsPerAttribute,
                    VertexAttributeBuffers[i].AttributeType, VertexAttributeBuffers[i].ShouldNormalize,
                    VertexAttributeBuffers[i].Stride, VertexAttributeBuffers[i].Offset);
                GL.VertexAttribDivisor(i, 0);
            }

            // Bind the index buffer if necessary
            if (IsIndexed)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.Handle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);

            // Bind instance buffer
            var vertexAttributesLength = VertexAttributeBuffers.Length;
            for (int i = 0; i < 4; ++i)
            {
                GL.EnableVertexAttribArray(vertexAttributesLength + i);
                GL.VertexAttribPointer(vertexAttributesLength + i, 4, VertexAttribPointerType.Float, false,
                    Vector4.SizeInBytes * 4, Vector4.SizeInBytes * i);
                GL.VertexAttribDivisor(vertexAttributesLength + i, 1);
            }
        }

        /// <summary>
        /// Draw the mesh, must be enabled first
        /// </summary>
        public void Draw()
        {
            if (IsIndexed)
                GL.DrawElements(PrimitiveType, PrimitiveCount, IndexBuffer.Type, 0);
            else
                GL.DrawArrays(PrimitiveType, 0, PrimitiveCount);
        }

        public void DrawInstanced(int instanceCount)
        {
            if (IsIndexed)
                GL.DrawElementsInstanced(PrimitiveType, PrimitiveCount, IndexBuffer.Type, (IntPtr)0, instanceCount);
            else
                GL.DrawArraysInstanced(PrimitiveType, 0, PrimitiveCount, instanceCount);
        }

        /// <summary>
        /// Disable this mesh
        /// </summary>
        public void Disable()
        {
            for (int i = 0, length = VertexAttributeBuffers.Length; i < length; ++i)
                GL.DisableVertexAttribArray(i);
        }
    
        /// <summary>
        /// Dispose the buffers in this mesh
        /// </summary>
        public void Dispose()
        {
            Debug.WriteLine("Disposing Mesh...");

            foreach (var vertBuffer in VertexAttributeBuffers)
                vertBuffer.Dispose();

            if (IsIndexed)
                IndexBuffer.Dispose();
        }

        /// <summary>
        /// Create a simple test triangle
        /// </summary>
        /// <returns>A triangle mesh</returns>
        public static Mesh CreateTestTriangle()
        {
            // Vertex data
            var vertexArray = new[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f)
            };

            // Bind vertex data
            var vertBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertexArray.Length), vertexArray, BufferUsageHint.StaticDraw);

            return new Mesh()
            {
                IsIndexed = false,
                PrimitiveCount = vertexArray.Length,
                PrimitiveType = PrimitiveType.Triangles,
                VertexAttributeBuffers = new[] { vertBuffer }
            };
        }
    }
}
