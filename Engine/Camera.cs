using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    /// <summary>
    /// Camera type enumeration
    /// </summary>
    public enum CameraType
    {
        Perspective,
        Orthographic
    }

    /// <summary>
    /// A camera entity class, used for rendering
    /// </summary>
    public class Camera
    {
        public const float DefaultFieldOfView = (float)Math.PI / 3f;
        public const float DefaultNearPlane = 1.0f;
        public const float DefaultFarPlane = 1000.0f;

        protected float fieldOfView = DefaultFieldOfView;
        protected float nearPlane = DefaultNearPlane;
        protected float farPlane = DefaultFarPlane;

        protected Vector3 target;
        protected Vector3 position;
        protected Vector3 up = Vector3.UnitY;
        protected CameraType type = CameraType.Perspective;

        public float FieldOfView 
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;
            }
        }

        public float NearPlane
        {
            get
            {
                return nearPlane;
            }
            set
            {
                nearPlane = value;
            }
        }

        public float FarPlane
        {
            get
            {
                return farPlane;
            }
            set
            {
                farPlane = value;
            }
        }

        public void CopyTo(Camera other)
        {
            other.fieldOfView = fieldOfView;
            other.nearPlane = nearPlane;
            other.farPlane = farPlane;
            other.target = target;
            other.up = up;
            other.position = position;
            other.type = type;
        }

        /// <summary>
        /// Get the view transformation of this camera
        /// </summary>
        /// <param name="matrix">Matrix output</param>
        public void GetView(out OpenTK.Matrix4 matrix)
        {
            matrix = Matrix4.LookAt(position, target, up);
        }

        /// <summary>
        /// Get the projection transformation of this camera
        /// </summary>
        /// <param name="projection">The projection output</param>
        /// <param name="renderWidth">The render width</param>
        /// <param name="renderHeight">The render height</param>
        public void GetProjection(out OpenTK.Matrix4 projection, int renderWidth, int renderHeight)
        {
            if (type == CameraType.Perspective)
            {
                float aspectRatio = (float)renderWidth / (float)renderHeight;

                Matrix4.CreatePerspectiveFieldOfView(fieldOfView,
                    aspectRatio, nearPlane, farPlane, out projection);
            }
            else
            {
                Matrix4.CreateOrthographic((float)renderWidth, (float)renderHeight, 
                    nearPlane, farPlane, out projection);
            }
        }

        /// <summary>
        /// Gets a ray eminating from the camera where x and y are the positions on screen
        /// </summary>
        /// <param name="x">The x position on screen in the range [0, 1]</param>
        /// <param name="y">The y position on screen in the range [0, 1]</param>
        /// <param name="viewportSize">The viewport size</param>
        /// <returns>The resulting ray</returns>
        public Ray GetCameraRay(float x, float y, Size viewportSize)
        {
            var forward = Target - Position;

            var left = Vector3.Cross(Up, forward);
            var up = Vector3.Cross(forward, left);

            forward.Normalize();
            left.Normalize();
            up.Normalize();

            var aspectRatio = (float)viewportSize.Width / (float)viewportSize.Height;

            var nearCenter = Position + forward * nearPlane;
            var a = 2f * (float)Math.Tan(fieldOfView * 0.5f);
            var nearHeight = nearPlane * a;
            var nearWidth = aspectRatio * nearHeight;

            var nearTopLeft = nearCenter + 0.5f * nearWidth * left + 0.5f * nearHeight * up;
            var result = nearTopLeft - x * nearWidth * left - y * nearHeight * up;
            result = result - Position;
            result.Normalize();

            return new Ray()
            {
                Origin = Position,
                Direction = result
            };
        }

        public void GetCameraFrustum(Plane[] frustumPlanes, Size viewportSize)
        {
            var forward = Target - Position;

            var left = Vector3.Cross(Up, forward);
            var up = Vector3.Cross(forward, left);

            forward.Normalize();
            left.Normalize();
            up.Normalize();

            var nearCenter = Position + forward * nearPlane;
            var farCenter = Position + forward * farPlane;

            // Front plane
            frustumPlanes[0] = Plane.FromPointAndNormal(nearCenter, -forward);
            // Back plane
            frustumPlanes[1] = Plane.FromPointAndNormal(farCenter, forward);

            var aspectRatio = (float)viewportSize.Width / (float)viewportSize.Height;

            var a = 2f * (float)Math.Tan(fieldOfView * 0.5f);
            var nearHeight = nearPlane * a;
            var farHeight = farPlane * a;
            var nearWidth = aspectRatio * nearHeight;
            var farWidth = aspectRatio * farHeight;

            var farTopLeft = farCenter + 0.5f * farWidth * left + 0.5f * farHeight * up;
            var farBottomLeft = farTopLeft - farHeight * up;
            var farTopRight = farTopLeft - farWidth * left;
            var farBottomRight = farTopRight - farHeight * up;

            var nearTopLeft = nearCenter + 0.5f * nearWidth * left + 0.5f * nearHeight * up;
            var nearBottomLeft = nearTopLeft - nearHeight * up;
            var nearTopRight = nearTopLeft - nearWidth * left;
            var nearBottomRight = nearTopRight - nearHeight * up;

            // Top plane
            frustumPlanes[2] = Plane.FromPoints(farTopLeft, nearTopLeft, farTopRight);
            // Bottom plane
            frustumPlanes[3] = Plane.FromPoints(farBottomLeft, farBottomRight, nearBottomLeft);
            // Left plane
            frustumPlanes[4] = Plane.FromPoints(farBottomLeft, nearBottomLeft, farTopLeft);
            // Right plane
            frustumPlanes[5] = Plane.FromPoints(farBottomRight, farTopRight, nearBottomRight);
        }

        /// <summary>
        /// The object the camera is looking at
        /// </summary>
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// The up direction of the camera
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary>
        /// The position of the camera
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// The type of camera this is
        /// </summary>
        public CameraType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
    }
}
