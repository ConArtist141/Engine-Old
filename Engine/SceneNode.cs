using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Engine
{
    public enum SceneNodeType
    {
        StaticMesh,
        Transform,
        Region,
        Root
    }

    public class SceneNode
    {
        public SceneNodeType Type { get; set; }
        public List<SceneNode> Children { get; set; }
        public bool IsTransformDirty { get; set; }
        public bool IsBoundingBoxDirty { get; set; }

        public BoundingBox Bounds;
        public SceneNode Parent;
        public Matrix4 Transform = Matrix4.Identity;
        public Matrix4 GlobalTransform = Matrix4.Identity;

        public virtual Material Material { get { return null; } set { } }
        public virtual Mesh Mesh { get { return null; } set { } }

        public SceneNode()
        {
            Type = SceneNodeType.Region;

            IsTransformDirty = true;
            IsBoundingBoxDirty = true;
        }

        public void AddChild(SceneNode node)
        {
            if (Children == null)
                Children = new List<SceneNode>();

            node.Parent = this;
            Children.Add(node);
        }

        public void ForEachDescendentPreOrder(Action<SceneNode> action)
        {
            action(this);

            if (Children != null)
                foreach (var child in Children)
                    child.ForEachDescendentPreOrder(action);
        }

        public void ForEachDescendentPostOrder(Action<SceneNode> action)
        {
            if (Children != null)
                foreach (var child in Children)
                    child.ForEachDescendentPostOrder(action);

            action(this);
        }

        public void ForEachDescendent(Action<SceneNode> preOrder, Action<SceneNode> postOrder)
        {
            preOrder(this);

            if (Children != null)
                foreach (var child in Children)
                    child.ForEachDescendent(preOrder, postOrder);

            postOrder(this);
        }

        public void RecomputeTransformations()
        {
            IsTransformDirty = false;

            if (Parent == null)
                GlobalTransform = Transform;
            else
                Matrix4.Mult(ref Transform, ref Parent.GlobalTransform, out GlobalTransform);

            if (Children != null)
                foreach (var child in Children)
                    child.RecomputeTransformations();
        }

        public virtual void RecomputeBoundingBox()
        {
            IsBoundingBoxDirty = false;

            if (Children == null || Children.Count == 0)
                Bounds = BoundingBox.ZeroBox;
            else
                Bounds = ComputeChildrenBoundingBox();
        }

        protected BoundingBox ComputeChildrenBoundingBox()
        {
            var childrenEnumerator = from child in Children
                                     select child.Bounds;

            return BoundingBox.Combine(childrenEnumerator);
        }

        public void MakeBoundingBoxDirty()
        {
            IsBoundingBoxDirty = true;

            if (Parent != null && !Parent.IsBoundingBoxDirty)
                Parent.MakeBoundingBoxDirty();
        }
    }

    public class RootNode : SceneNode
    {
        public RootNode()
        {
            Type = SceneNodeType.Root;
        }
    }

    public class StaticMeshNode : SceneNode
    {
        private Material material;
        private Mesh mesh;

        public override Material Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
            }
        }

        public override Mesh Mesh
        {
            get
            {
                return mesh;
            }
            set
            {
                mesh = value;
            }
        }

        public override void RecomputeBoundingBox()
        {
            IsBoundingBoxDirty = false;
            Bounds = mesh.BoundingBox.TransformBox(ref GlobalTransform);

            if (Children != null && Children.Count > 0)
            {
                var childrenBound = ComputeChildrenBoundingBox();
                Vector3.ComponentMin(ref Bounds.Lower, ref childrenBound.Upper, out Bounds.Lower);
                Vector3.ComponentMax(ref Bounds.Upper, ref childrenBound.Upper, out Bounds.Upper);
            }
        }

        public StaticMeshNode(Mesh mesh, Material material)
        {
            this.material = material;
            this.mesh = mesh;

            Type = SceneNodeType.StaticMesh;
        }
    }
}
