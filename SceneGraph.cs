using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK;

namespace Template
{
    class SceneGraph
    {
        public List<Mesh> meshes = new List<Mesh>();
        Matrix4 ViewPort = Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);
        public SceneGraph() { }
        public void Render(Matrix4 camera)
        {
            foreach(Mesh mesh in meshes)
            {
                mesh.Render(mesh.Shader, mesh.ModelMatrix * camera * ViewPort, mesh.Texture);
            }
        }
    }
}
