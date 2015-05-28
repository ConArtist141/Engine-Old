using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Engine
{
    public struct Plane
    {
        public Vector3 Normal;
        public float Distance;

        public static Plane FromPointAndNormal(Vector3 point, Vector3 normal)
        {
            return new Plane()
            {
                Normal = normal,
                Distance = Vector3.Dot(point, normal)
            };
        }

        public static Plane FromPoints(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var normal = Vector3.Cross(p2 - p1, p3 - p1);
            normal.Normalize();

            return new Plane()
            {
                Normal = normal,
                Distance = Vector3.Dot(p1, normal)
            };
        }

        public static bool TestPointAgainstFrustum(Plane[] frustum, Vector3 point)
        {
            if (Vector3.Dot(frustum[0].Normal, point) > frustum[0].Distance)
                return false;
            if (Vector3.Dot(frustum[1].Normal, point) > frustum[1].Distance)
                return false;
            if (Vector3.Dot(frustum[2].Normal, point) > frustum[2].Distance)
                return false;
            if (Vector3.Dot(frustum[3].Normal, point) > frustum[3].Distance)
                return false;
            if (Vector3.Dot(frustum[4].Normal, point) > frustum[4].Distance)
                return false;
            if (Vector3.Dot(frustum[5].Normal, point) > frustum[5].Distance)
                return false;

            return true;
        }
    }
}
