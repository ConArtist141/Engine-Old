using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Engine
{
    public class ContentManager : IDisposable
    {
        protected MeshLoader meshLoader = new MeshLoader();
        public string ContentRoot { get; set; }

        protected Dictionary<string, ShaderProgram> programs = new Dictionary<string, ShaderProgram>();
        protected Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        protected Dictionary<string, Material> materials = new Dictionary<string, Material>();
        protected Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        
        public ContentManager()
        {
            ContentRoot = "Content";
        }

        public ShaderProgram ForceLoadProgram(string programName, string vertexShaderSrc, string fragmentShaderSrc)
        {
            var program = ResourceLoader.LoadProgramFromFile(ContentRoot + "\\" + vertexShaderSrc, ContentRoot + "\\" + fragmentShaderSrc);
            if (program < 0)
                throw new Exception("Resource " + programName + "could not be found!");
            programs[programName] = program;
            return program;
        }

        public Texture ForceLoadTexture2D(string textureSource)
        {
            var texture = ResourceLoader.LoadTexture2DFromFile(ContentRoot + "\\" + textureSource);
            if (texture < 0)
                throw new Exception("Resource " + textureSource + "could not be found!");
            textures[textureSource] = texture;
            return texture;
        }

        public Mesh ForceLoadStaticMesh(string meshSource)
        {
            var mesh = meshLoader.LoadStaticMesh(ContentRoot + "\\" + meshSource);
            meshes[meshSource] = mesh;
            return mesh;
        }

        public ShaderProgram LoadProgram(string programName, string vertexShaderSrc, string fragmentShaderSrc)
        {
            ShaderProgram prog;
            if (programs.TryGetValue(programName, out prog))
                return prog;
            else
                return ForceLoadProgram(programName, vertexShaderSrc, fragmentShaderSrc);
        }

        public Texture LoadTexture2D(string textureSource)
        {
            Texture texture;
            if (textures.TryGetValue(textureSource, out texture))
                return texture;
            else
                return ForceLoadTexture2D(textureSource);
        }

        public Mesh LoadStaticMesh(string meshSource)
        {
            Mesh mesh;
            if (meshes.TryGetValue(meshSource, out mesh))
                return mesh;
            else
                return ForceLoadStaticMesh(meshSource);
        }

        public Material GetMaterial(string materialName)
        {
            Material material;
            if (materials.TryGetValue(materialName, out material))
                return material;
            else
                return null;
        }

        public Material ForceLoadMaterial<MaterialType>(string materialName, params Texture[] textures)
            where MaterialType : Material, new()
        {
            var material = new MaterialType();
            material.Name = materialName;
            material.Textures = textures.ToList();
            material.Initialize(this);
            materials[materialName] = material;
            return material;
        }
      
        public Material LoadMaterial<MaterialType>(string materialName, params Texture[] textures)
            where MaterialType : Material, new()
        {
            var material = GetMaterial(materialName);
            if (material == null)
                return ForceLoadMaterial<MaterialType>(materialName, textures);
            else
                return material;
        }

        public ShaderProgram GetProgram(string programName)
        {
            ShaderProgram prog;
            if (programs.TryGetValue(programName, out prog))
                return prog;
            else
                return ShaderProgram.Invalid;
        }

        public void Dispose()
        {
            foreach (var program in programs.Values)
                program.Dispose();
            foreach (var texture in textures.Values)
                texture.Dispose();
            foreach (var mesh in meshes.Values)
                mesh.Dispose();

            programs.Clear();
            textures.Clear();
            meshes.Clear();
            materials.Clear();
        }
    }
}
