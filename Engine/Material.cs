using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Engine
{
    /// <summary>
    /// A material is a set of parameters for a specific shader
    /// </summary>
    public abstract class Material
    {
        public const string DefaultWorldUniformLocation = "WorldTransform";
        public const string DefaultViewProjUniformLocation = "ViewProjectionTransform";

        /// <summary>
        /// Gets or sets the material name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The shader this material corresonds to
        /// </summary>
        public ShaderProgram Shader { get; set; }
        /// <summary>
        /// The world transform uniform of the shader
        /// </summary>
        public int WorldUniform { get; protected set; }
        /// <summary>
        /// The view projection uniform of the shader
        /// </summary>
        public int ViewProjectionUniform { get; protected set; }
        /// <summary>
        /// A list of textures this material uses
        /// </summary>
        public List<Texture> Textures = new List<Texture>();

        /// <summary>
        /// Load the shader for this material
        /// </summary>
        /// <param name="content">The content manager</param>
        /// <returns>The shader for this material</returns>
        protected abstract ShaderProgram LoadShader(ContentManager content);

        /// <summary>
        /// Make this material the active material, set shader parameters
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Set a parameter of this material
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="value">The value to set it to</param>
        public abstract void SetParameter(string parameterName, object value);

        /// <summary>
        /// Initialize this shader
        /// </summary>
        /// <param name="content">The parent content manager</param>
        public virtual void Initialize(ContentManager content)
        {
            Shader = LoadShader(content);
            LocateTransformUniforms();
        }

        /// <summary>
        /// Find the transform uniforms in the material shader
        /// </summary>
        public void LocateTransformUniforms()
        {
            // Get 'dem uniforms
            WorldUniform = GL.GetUniformLocation(Shader, DefaultWorldUniformLocation);
            ViewProjectionUniform = GL.GetUniformLocation(Shader, DefaultViewProjUniformLocation);

            // Something went wrong
            if (ViewProjectionUniform < 0)
                Debug.WriteLine("Could not find ViewProjection uniform!");
        }

        public void DisposeTextures()
        {
            foreach (var texture in Textures)
                texture.Dispose();
        }

        public void DisposeShader()
        {
            Shader.Dispose();
        }
    }

    /// <summary>
    /// The default material, does absolutely nothing
    /// </summary>
    public class DefaultMaterial : Material
    {
        public const string ShaderProgramName = "DefaultShader";
        public const string VertexShaderSource = "Shaders\\DefaultInstanced.vert";
        public const string FragmentShaderSource = "Shaders\\Default.frag";

        protected override ShaderProgram LoadShader(ContentManager content)
        {
            return content.LoadProgram(ShaderProgramName, VertexShaderSource, FragmentShaderSource);
        }

        public override void Apply()
        {
        }

        public override void SetParameter(string parameterName, object value)
        {
        }
    }
}
