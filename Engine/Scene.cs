using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using OpenTK;

namespace Engine
{
    /// <summary>
    /// Represents a scene which can be rendered by the renderer
    /// </summary>
    public class Scene : IDisposable
    {
        public Camera ActiveCamera { get; set; }
        public ContentManager Content { get; protected set; }
        public RootNode StaticSceneRoot { get; set; }
        public RootNode DynamicSceneRoot { get; set; }

        public Scene()
        {
            Content = new ContentManager();
            StaticSceneRoot = new RootNode();
            DynamicSceneRoot = new RootNode();
            ActiveCamera = new Camera();
        }

        public void AddNode(SceneNode node, RootNode parent = null)
        {
            if (parent == null)
                parent = StaticSceneRoot;

            parent.AddChild(node);
        }

        public void ProcessStaticSceneGraph()
        {
            // Update transformations as necessary
            StaticSceneRoot.ForEachDescendentPreOrder(node =>
            {
                if (node.IsTransformDirty)
                    node.RecomputeTransformations();
            });

            // Update bounding boxes as necessary
            StaticSceneRoot.ForEachDescendentPostOrder(node =>
            {
                if (node.IsBoundingBoxDirty)
                    node.RecomputeBoundingBox();
            });
        }

        public void Update(FrameEventArgs e)
        {
            // Update transforms as necessary
            DynamicSceneRoot.ForEachDescendentPreOrder(node =>
            {
                if (node.IsTransformDirty)
                    node.RecomputeTransformations();
            });
        }

        public void Dispose()
        {
            Content.Dispose();
        }
    }
}
