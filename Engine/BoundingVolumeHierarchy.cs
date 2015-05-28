using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Engine
{
    public static class BoundingVolumeHierarchy
    {
        public enum SplitAxis
        {
            AxisX,
            AxisY,
            AxisZ
        }

        /// <summary>
        /// Generates a bounding volume heirarchy for this node. The user must call ProcessStaticSceneGraph before and after this
        /// </summary>
        /// <param name="node">The base node</param>
        public static void Generate(SceneNode node)
        {
            if (node.Children != null && node.Children.Count > 2)
            {
                // Compute bounds for the centers of the bounding boxes of the children
                var lower = new Vector3(Single.PositiveInfinity);
                var upper = new Vector3(Single.NegativeInfinity);

                var center = Vector3.Zero;

                foreach (var child in node.Children)
                {
                    Vector3.Add(ref child.Bounds.Lower, ref child.Bounds.Upper, out center);
                    Vector3.Multiply(ref center, 0.5f, out center);
                    Vector3.ComponentMax(ref center, ref upper, out upper);
                    Vector3.ComponentMin(ref center, ref lower, out lower);
                }

                var bounds = new BoundingBox()
                {
                    Lower = lower,
                    Upper = upper
                };

                // Split into two groups
                var group1 = new List<SceneNode>();
                var group2 = new List<SceneNode>();
                var maximumAxisLength = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);

                var splitAxis = SplitAxis.AxisX;
                if (maximumAxisLength == bounds.SizeY)
                    splitAxis = SplitAxis.AxisY;
                else if (maximumAxisLength == bounds.SizeZ)
                    splitAxis = SplitAxis.AxisZ;

                switch (splitAxis)
                {
                    case SplitAxis.AxisX:
                        {
                            var centerX = bounds.CenterX;

                            foreach (var child in node.Children)
                                if (child.Bounds.CenterX < centerX)
                                    group1.Add(child);
                                else
                                    group2.Add(child);

                            break;
                        }
                    case SplitAxis.AxisY:
                        {
                            var centerY = bounds.CenterY;

                            foreach (var child in node.Children)
                                if (child.Bounds.CenterY < centerY)
                                    group1.Add(child);
                                else
                                    group2.Add(child);

                            break;
                        }
                    case SplitAxis.AxisZ:
                        {
                            var centerZ = bounds.CenterZ;

                            foreach (var child in node.Children)
                                if (child.Bounds.CenterZ < centerZ)
                                    group1.Add(child);
                                else
                                    group2.Add(child);

                            break;
                        }
                }

                node.Children.Clear();

                if (group1.Count == 1)
                    node.Children.Add(group1[0]);
                if (group2.Count == 1)
                    node.Children.Add(group2[0]);

                if (group1.Count > 1)
                {
                    var newChild = new SceneNode()
                        {
                            Children = group1
                        };

                    node.Children.Add(newChild);
                    Generate(newChild);
                }

                if (group2.Count > 1)
                {
                    var newChild = new SceneNode()
                    {
                        Children = group2
                    };

                    node.Children.Add(newChild);
                    Generate(newChild);
                }
            }
        }
    }
}
