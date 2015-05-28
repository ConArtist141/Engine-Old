using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Engine;

namespace Application
{
    class RenderWindow : GameWindow
    {
        protected Renderer Renderer;
        protected Scene Scene;
        protected CameraController CameraController;

        public const int SceneSize = 100;

        public RenderWindow()
            : base(1024, 768, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8), 24, 0, 4))
        {
            Title = "Graphics Engine";

            Keyboard.KeyDown += (obj, args) =>
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
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            Renderer = new Renderer();
            Renderer.Initialize(ClientSize);

            CreateScene();
        }

        protected void CreateScene()
        {
            Scene = new Scene();
            var content = Scene.Content;

            // Load content
            var diffuseTexture = content.LoadTexture2D("CrateDiffuse.png");
            var material = content.LoadMaterial<DiffuseMaterial>("DiffuseCrate", diffuseTexture);
            var mesh = content.LoadStaticMesh("Crate.DAE");

            var diffuseTexture2 = content.LoadTexture2D("statue_d.png");
            var mesh2 = content.ForceLoadStaticMesh("statue.obj");
            var material2 = content.LoadMaterial<DiffuseMaterial>("DiffuseStatue", diffuseTexture2);

            // Create static meshes
            for (var x = -SceneSize; x <= SceneSize; ++x)
                for (var z = -SceneSize; z <= SceneSize; ++z)
                    if (x != 0 || z != 0)
                        Scene.AddNode(new StaticMeshNode(mesh, material)
                        {
                            Transform = Matrix4.CreateTranslation(40f * x, 0f, 40f * z)
                        });

            Scene.AddNode(new StaticMeshNode(mesh2, material2)
            {
                Transform = Matrix4.CreateScale(20f) * Matrix4.CreateTranslation(0f, -10f, 0f)
            });

            // Generate our bounding volume hierarchy
            Scene.ProcessStaticSceneGraph();
            BoundingVolumeHierarchy.Generate(Scene.StaticSceneRoot);
            Scene.ProcessStaticSceneGraph();

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
            if (Keyboard[Key.Escape])
                Exit();

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
            Scene.Dispose();
            CameraController.Dispose();
            Renderer.Dispose();
        }
    }
}
