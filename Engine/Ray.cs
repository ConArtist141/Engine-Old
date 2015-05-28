using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Engine
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public float GetPlaneIntersectionParameter(Plane plane)
        {
            var a = plane.Distance - Vector3.Dot(Origin, plane.Normal);
            var b = Vector3.Dot(Direction, plane.Normal);
            return a / b;
        }

        public void ParameterToVector(float parameter, out Vector3 vec)
        {
            vec = Origin + parameter * Direction;
        }

        public bool IntersectsPlane(Plane plane, ref Vector3 location)
        {
            var p = GetPlaneIntersectionParameter(plane);
            if (p < 0f || Single.IsNaN(p))
                return false;
            else
            {
                ParameterToVector(p, out location);
                return true;
            }
        }
    }
}
