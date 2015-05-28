using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Engine;

namespace Editor
{
    public enum EditorTool
    {
        None,
        Placement
    }

    public class EditorWindow : GameWindow
    {
        protected Renderer Renderer;
        protected Scene Scene;
        protected CameraController CameraController;
        protected ToolWindow ToolWindow;
        protected EditorTool CurrentTool = EditorTool.None;

        public const int SceneSize = 100;

        public EditorWindow()
            : base(1024, 768)
        {
            Title = "Level Editor";
        }

        protected override void OnLoad(EventArgs e)
        {
            ToolWindow = new Editor.ToolWindow(this);
            ToolWindow.Show();

            Renderer = new Renderer();
            Renderer.Initialize(ClientSize);
            Renderer.GridOptions.GridEnabled = true;

            Mouse.ButtonDown += OnMouseButtonDown;
            KeyDown += OnKeyDown;

            CreateScene();
        }

        protected void PlaceObject()
        {
            var ray = Scene.ActiveCamera.GetCameraRay((float)Mouse.X / (float)ClientSize.Width, (float)Mouse.Y / (float)ClientSize.Height, ClientSize);
            var gridPlane = new Plane()
            {
                Normal = Vector3.UnitY,
                Distance = Renderer.GridOptions.GridHeight
            };

            var intersection = Vector3.Zero;

            if (ray.IntersectsPlane(gridPlane, ref intersection))
            {
                var currentNode = ToolWindow.ResourceView.SelectedNode;
                try
                {
                    var path = Scene.Content.ContentRoot + "\\" + currentNode.Text;
                    if (File.Exists(path))
                    {
                        var extension = Path.GetExtension(currentNode.Text).ToLower();
                        if (extension == ".obj" || extension == ".dae")
                        {
                            var mesh = Scene.Content.LoadStaticMesh(currentNode.Text);
                            var material = Scene.Content.GetMaterial("Default");

                            var sceneNode = new StaticMeshNode(mesh, material)
                            {
                                Transform = Matrix4.CreateTranslation(intersection)
                            };

                            Scene.AddNode(sceneNode);
                            Scene.StaticSceneRoot.IsBoundingBoxDirty = true;
                            Scene.ProcessStaticSceneGraph();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs args)
        {
            if (Renderer != null)
            {
                if (args.Key == Key.F1)
                    Renderer.BoundingVolumeModeEnabled = !Renderer.BoundingVolumeModeEnabled;

                if (args.Key == Key.Tilde)
                    Renderer.UseWireframe = !Renderer.UseWireframe;

                if (args.Key == Key.Plus)
                    Renderer.BoundingVolumeDrawDepth++;

                if (args.Key == Key.Minus)
                {
                    Renderer.BoundingVolumeDrawDepth--;
                    Renderer.BoundingVolumeDrawDepth = Math.Max(Renderer.BoundingVolumeDrawDepth, -1);
                }
            }

            if (args.Key == Key.Number1)
                CurrentTool = EditorTool.Placement;
            else if (args.Key == Key.Number0)
                CurrentTool = EditorTool.None;
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left && CurrentTool == EditorTool.Placement)
                PlaceObject();
        }

        protected void CreateScene()
        {
            Scene = new Scene();
            var content = Scene.Content;

            // Load default texture
            var defaultTexture = content.LoadTexture2D("Default.jpg");
            content.LoadMaterial<DiffuseMaterial>("Default", defaultTexture);

            // Set Camera 
            Scene.ActiveCamera.Position = new Vector3(40f, 40f, 40f);
            CameraController = new FirstPersonCameraController(Scene.ActiveCamera, this);
            CameraController.LookAt(Vector3.Zero);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Renderer != null)
                Renderer.OnResize(ClientSize);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Scene.Update(e);

            CameraController.UpdateInput(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Renderer.Render(e, Scene);

            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            ToolWindow.Close();

            Scene.Dispose();
            CameraController.Dispose();
            Renderer.Dispose();
        }
    }
}
