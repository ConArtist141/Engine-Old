using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace Engine
{
    public class EditorGridOptions
    {
        public float CellSize { get; set; }
        public float GridHeight { get; set; }
        public int GridRadius { get; set; }
        public bool GridEnabled { get; set; }

        public static EditorGridOptions Default
        {
            get
            {
                return new EditorGridOptions()
                {
                    CellSize = 16f,
                    GridHeight = 0f,
                    GridRadius = 30,
                    GridEnabled = false
                };
            }
        }
    }

    public class Renderer : IDisposable
    {
        /// <summary>
        /// Gets or sets the use of wireframe
        /// </summary>
        public bool UseWireframe
        {
            get { return bUseWireframe; }
            set
            {
                bUseWireframe = value;
                GL.PolygonMode(MaterialFace.FrontAndBack, (bUseWireframe ? PolygonMode.Line : PolygonMode.Fill));
            }
        }

        /// <summary>
        /// A mode simply for debug purposes
        /// </summary>
        public bool BoundingVolumeModeEnabled { get; set; }

        /// <summary>
        /// Depth at which to draw bounding volumes
        /// </summary>
        public int BoundingVolumeDrawDepth { get; set; }

        /// <summary>
        /// Enabled instanced renderering
        /// </summary>
        public bool InstancedRendereringEnabled { get; set; }

        /// <summary>
        /// Options for the editor grid
        /// </summary>
        public EditorGridOptions GridOptions { get; set; }

        /// <summary>
        /// Gets the size of the client region
        /// </summary>
        public Size ClientSize
        {
            get { return clientSize; }
        }

        public const int DefaultInstanceDataSize = 64;
        public const int DefaultVisibleNodeCacheSize = 128;

        protected Size clientSize;
        protected int vertexArray = -1;
        protected bool bUseWireframe = false;
        protected Plane[] cameraFrustum = new Plane[6];
        protected Camera testCamera = new Camera();
        protected int instanceBuffer = -1;
        protected Matrix4[] instanceDataCache;
        protected ArrayCache<StaticMeshNode> visibleNodeCache;

        public Renderer()
        {
            BoundingVolumeModeEnabled = false;
            BoundingVolumeDrawDepth = -1;
            InstancedRendereringEnabled = true;
            GridOptions = EditorGridOptions.Default;
            visibleNodeCache = new ArrayCache<StaticMeshNode>(DefaultVisibleNodeCacheSize);

            ResetInstanceCache();
        }

        /// <summary>
        /// Free up memory used in the instance cache
        /// </summary>
        public void ResetInstanceCache()
        {
            instanceDataCache = new Matrix4[DefaultInstanceDataSize];
        }

        /// <summary>
        /// Free up memory used in the visible node cache
        /// </summary>
        public void ResetVisibleNodeCache()
        {
            visibleNodeCache.Resize(DefaultVisibleNodeCacheSize);
        }

        /// <summary>
        /// Resizes the instance cache
        /// </summary>
        /// <param name="newSize">The new size of the instance cache</param>
        public void ResizeInstanceCache(int newSize)
        {
            var size = DefaultInstanceDataSize;
            while (size < newSize)
                size *= 2;

            instanceDataCache = new Matrix4[size];
        }

        /// <summary>
        /// Initialize the renderer
        /// </summary>
        /// <param name="window">The parent game window</param>
        public void Initialize(Size clientSize)
        {
            this.clientSize = clientSize;

            // Set our clear color
            GL.ClearColor(Color.Black);

            // Create our vertex array and bind it
            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            // Set our render state
            SetRenderState();

            // Create the instance buffer
            instanceBuffer = GL.GenBuffer();
        }

        public void SetRenderState()
        {
            // Enable required featuers
            GL.Enable(EnableCap.Texture2D);

            // Cull back faces
            GL.Enable(EnableCap.CullFace);

            // Set wireframe if necessary
            GL.PolygonMode(MaterialFace.FrontAndBack, (bUseWireframe ? PolygonMode.Line : PolygonMode.Fill));

            // Enable depth testing
            GL.Enable(EnableCap.DepthTest);
        }

        public void ComputeOcclusion(Scene scene, Camera camera, ArrayCache<StaticMeshNode> visibleMeshes)
        {
            visibleMeshes.Clear();
            camera.GetCameraFrustum(cameraFrustum, clientSize);
            ComputeOcclusionRecursive(scene.StaticSceneRoot, visibleMeshes);
        }

        protected void ComputeOcclusionRecursive(SceneNode node, ArrayCache<StaticMeshNode> visibleMeshes)
        {
            if (!node.Bounds.IsOutsideFrustum(cameraFrustum))
            {
                if (node.Type == SceneNodeType.StaticMesh)
                    visibleMeshes.Add((StaticMeshNode)node);

                if (node.Children != null)
                    foreach (var child in node.Children)
                        ComputeOcclusionRecursive(child, visibleMeshes);
            }
        }

        public void OnResize(Size ClientSize)
        {
            // Window has been resized, change viewport size
            clientSize = ClientSize;
            GL.Viewport(ClientSize);
        }

        public void OnUpdateFrame(FrameEventArgs e)
        {

        }

        protected void DrawBoundingBox(BoundingBox box)
        {
            // Face 1
            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Lower.Z);
            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Upper.Z);

            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Upper.Z);
            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Upper.Z);

            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Upper.Z);
            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Lower.Z);

            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Lower.Z);
            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Lower.Z);

            // Face 2
            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Lower.Z);
            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Upper.Z);

            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Upper.Z);
            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Upper.Z);

            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Upper.Z);
            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Lower.Z);

            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Lower.Z);
            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Lower.Z);

            // Other lines
            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Lower.Z);
            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Lower.Z);

            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Lower.Z);
            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Lower.Z);

            GL.Vertex3(box.Lower.X, box.Upper.Y, box.Upper.Z);
            GL.Vertex3(box.Upper.X, box.Upper.Y, box.Upper.Z);

            GL.Vertex3(box.Lower.X, box.Lower.Y, box.Upper.Z);
            GL.Vertex3(box.Upper.X, box.Lower.Y, box.Upper.Z);
        }

        protected void DrawVolume(SceneNode node, int drawDepth)
        {
            if (!node.Bounds.IsOutsideFrustum(cameraFrustum))
            {
                if (drawDepth < 1 || node.Children == null)
                    DrawBoundingBox(node.Bounds);

                if (drawDepth != 0 && node.Children != null)
                    foreach (var child in node.Children)
                        DrawVolume(child, drawDepth - 1);
            }
        }

        protected void DrawBoundingVolumes(Scene scene, ref Matrix4 ViewProjection)
        {
            GL.UseProgram(0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadMatrix(ref ViewProjection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Begin(PrimitiveType.Lines);

            DrawVolume(scene.StaticSceneRoot, BoundingVolumeDrawDepth);

            GL.End();

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        protected void RenderStaticMeshes(Scene scene, ref Matrix4 ViewProjection, ArrayCache<StaticMeshNode> meshesToRender, bool bUseInstancing)
        {
            // Group by shader
            var shaderIterator = meshesToRender.GroupBy(t => t.Material.Shader);

            foreach (var shaderGroup in shaderIterator)
            {
                // Change shader
                GL.UseProgram(shaderGroup.Key);

                // Group by material
                var materialIterator = shaderGroup.GroupBy(t => t.Material);

                foreach (var materialGroup in materialIterator)
                {
                    // Change material if necessary
                    materialGroup.Key.Apply();

                    // Set View Projection Transform
                    GL.UniformMatrix4(materialGroup.Key.ViewProjectionUniform, false, ref ViewProjection);

                    // Group by mesh
                    var meshIterator = materialGroup.GroupBy(t => t.Mesh);

                    foreach (var meshGroup in meshIterator)
                    {
                        if (bUseInstancing)
                        {
                            // Use instancing, faster, better, cooler
                            var visibleCount = meshGroup.Count();

                            // Resize instance cache if necessary
                            if (visibleCount > instanceDataCache.Length)
                                ResizeInstanceCache(visibleCount);

                            // Fill instance cache
                            var dataCacheLocation = 0;
                            foreach (var entity in meshGroup)
                            {
                                instanceDataCache[dataCacheLocation] = entity.GlobalTransform;
                                ++dataCacheLocation;
                            }

                            // Copy into instance buffer
                            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);
                            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(visibleCount * 4 * Vector4.SizeInBytes), instanceDataCache, BufferUsageHint.DynamicDraw);

                            // Enable mesh
                            meshGroup.Key.EnableInstanced(instanceBuffer);

                            // Draw batch
                            meshGroup.Key.DrawInstanced(visibleCount);

                            // Disable mesh
                            meshGroup.Key.Disable();
                        }
                        else
                        {
                            // Enable mesh
                            meshGroup.Key.Enable();

                            foreach (var entity in meshGroup)
                            {
                                // Actually render
                                RenderStaticMesh(entity);
                            }

                            // Disable mesh
                            meshGroup.Key.Disable();
                        }
                    }
                }
            }
        }

        protected void RenderEditorGrid(Camera activeCamera, ref Matrix4 ViewProjection)
        {
            var xStart = (int)Math.Floor(activeCamera.Position.X / GridOptions.CellSize) - GridOptions.GridRadius;
            var zStart = (int)Math.Floor(activeCamera.Position.Z / GridOptions.CellSize) - GridOptions.GridRadius;
            var xEnd = xStart + 2 * GridOptions.GridRadius;
            var zEnd = zStart + 2 * GridOptions.GridRadius;
            var height = GridOptions.GridHeight;
            var cellSize = GridOptions.CellSize;

            GL.UseProgram(0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadMatrix(ref ViewProjection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Begin(PrimitiveType.Lines);

            for (var x = xStart; x <= xEnd; ++x)
            {
                GL.Vertex3(x * cellSize, height, zStart * cellSize);
                GL.Vertex3(x * cellSize, height, zEnd * cellSize);
            }
            
            for (var z = zStart; z <= zEnd; ++z)
            {
                GL.Vertex3(xStart * cellSize, height, z * cellSize);
                GL.Vertex3(xEnd * cellSize, height, z * cellSize);
            }

            GL.End();

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        public void Render(FrameEventArgs e, Scene scene)
        {
            // Clear the framebuffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render all entities in the scene
            if (scene != null)
            {
                // Get the active camera
                var camera = scene.ActiveCamera;

                Matrix4 Projection;
                Matrix4 View;

                // Compute our occlusion
                ComputeOcclusion(scene, camera, visibleNodeCache);

                // Get view and projection matrices
                camera.GetProjection(out Projection, clientSize.Width, clientSize.Height);
                camera.GetView(out View);

                // Compound the view and projection matrices
                Matrix4 ViewProj = View * Projection;

                // Render the editor grid if requested
                if (GridOptions.GridEnabled)
                    RenderEditorGrid(camera, ref ViewProj);

                // Set the appropriate render state
                SetRenderState();

                // Render static meshes
                if (BoundingVolumeModeEnabled)
                    DrawBoundingVolumes(scene, ref ViewProj);
                else
                    RenderStaticMeshes(scene, ref ViewProj, visibleNodeCache, InstancedRendereringEnabled);
            }
        }

        public void RenderStaticMesh(StaticMeshNode staticMesh)
        {
            // Set transforms
            GL.UniformMatrix4(staticMesh.Material.WorldUniform, false, ref staticMesh.GlobalTransform);

            // Draw the mesh
            staticMesh.Mesh.Draw();
        }

        public void Dispose()
        {
            GL.DeleteBuffer(instanceBuffer);
            GL.DeleteVertexArray(vertexArray);
        }
    }
}
