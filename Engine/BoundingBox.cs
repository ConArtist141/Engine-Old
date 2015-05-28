using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Engine
{
    public struct BoundingBox
    {
        public Vector3 Lower;
        public Vector3 Upper;

        public Vector3 this[int i]
        {
            get
            {
                return new Vector3((i & 0x1) == 0 ? Lower.X : Upper.X,
                    (i & 0x2) == 0 ? Lower.Y : Upper.Y,
                    (i & 0x4) == 0 ? Lower.Z : Upper.Z);
            }
        }

        public Vector3 Center
        {
            get
            {
                return (Lower + Upper) * 0.5f;
            }
        }

        public float SizeX
        {
            get { return Upper.X - Lower.X; }
        }

        public float SizeY
        {
            get { return Upper.Y - Lower.Y; }
        }
        
        public float SizeZ
        {
            get { return Upper.Z - Lower.Z; }
        }

        public float CenterX
        {
            get { return (Lower.X + Upper.X) * 0.5f; }
        }

        public float CenterY
        {
            get { return (Lower.Y + Upper.Y) * 0.5f; }
        }

        public float CenterZ
        {
            get { return (Lower.Z + Upper.Z) * 0.5f; }
        }

        public BoundingBox TransformBox(ref Matrix4 transform)
        {
            var lower = new Vector3(Single.PositiveInfinity);
            var upper = new Vector3(Single.NegativeInfinity);

            Vector3 result;

            for (int i = 0; i < 8; ++i)
            {
                var bound = this[i];

                Vector3.Transform(ref bound, ref transform, out result);
                Vector3.ComponentMin(ref lower, ref result, out lower);
                Vector3.ComponentMax(ref upper, ref result, out upper);
            }

            return new BoundingBox()
            {
                Lower = lower,
                Upper = upper
            };
        }

        public static BoundingBox ZeroBox
        {
            get
            {
                return new BoundingBox()
                {
                    Lower = Vector3.Zero,
                    Upper = Vector3.Zero
                };
            }
        }

        public static BoundingBox Combine(IEnumerable<BoundingBox> boxes)
        {
            Vector3 lower = new Vector3(Single.PositiveInfinity);
            Vector3 upper = new Vector3(Single.NegativeInfinity);

            foreach (var box in boxes)
            {
                var boxLower = box.Lower;
                var boxUpper = box.Upper;
                Vector3.ComponentMin(ref lower, ref boxLower, out lower);
                Vector3.ComponentMax(ref upper, ref boxUpper, out upper);
            }

            return new BoundingBox()
            {
                Lower = lower,
                Upper = upper
            };
        }

        public bool IsOutsideFrustum(Plane[] frustum)
        {
            for (int i = 0; i < 6; ++i)
            {
                var plane = frustum[i];
                var sum = 0;

                for (int j = 0; j < 8; ++j)
                {
                    var point = this[j];
                    sum += Convert.ToInt32(Vector3.Dot(point, plane.Normal) > plane.Distance);

                    if (sum == 8)
                        return true;
                }
            }

            return false;
        }
    }
}
