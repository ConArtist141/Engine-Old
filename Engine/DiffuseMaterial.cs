using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public abstract class TextureMaterial : Material
    {
        public int[] SamplerUniformLocations { get; protected set; }

        protected void LocateSamplerUniforms(params string[] samplerUniformNames)
        {
            SamplerUniformLocations = (from name in samplerUniformNames
                                       select GL.GetUniformLocation(Shader, name)).ToArray();

            if (SamplerUniformLocations.Contains(-1))
                Debug.WriteLine("Failed to find a sampler uniform!");
        }

        protected void BindTextures()
        {
            // Bind 'dem textures
            for (int i = 0; i < Textures.Count; ++i)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, Textures[i]);
                GL.Uniform1(SamplerUniformLocations[i], i);
            }
        }
    }

    public class DiffuseMaterial : TextureMaterial
    {
        public const string ShaderName = "DiffuseMaterial";
        public const string VertexShaderSource = "Shaders\\DefaultInstanced.vert";
        public const string FragmentShaderSource = "Shaders\\Diffuse.frag";

        protected override ShaderProgram LoadShader(ContentManager content)
        {
            return content.LoadProgram(ShaderName, VertexShaderSource, FragmentShaderSource);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            LocateSamplerUniforms("Diffuse");
        }

        public override void Apply()
        {
            BindTextures();
        }

        public override void SetParameter(string parameterName, object value)
        {
        }
    }
}
