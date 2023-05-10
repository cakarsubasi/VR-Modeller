using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Meshes;

using Unity.Mathematics;

namespace MeshesIO
{

    public struct SceneDescription
    {
        public List<UMesh> objects;
    }

    public class WFGrammar
    {
        public sealed class WFComment : WFGrammar
        {
            string comment;

            static Regex rx = new Regex(@"# (.+)", RegexOptions.Compiled);
        }


        public sealed class WFMTLLib : WFGrammar
        {
            string file;

            static Regex rx = new Regex(@"mtllib (.+)", RegexOptions.Compiled);
        }
        public sealed class WFObject : WFGrammar
        {
            string name;
            List<WFVertex> positions;
            List<WFTextureCoordinate> uvs;
            List<WFVertexNormal> normals;
            List<WFFace> faces;

            static Regex rx = new Regex(@"o (.+)", RegexOptions.Compiled);
        }

        public sealed class WFVertex : WFGrammar
        {
            float x;
            float y;
            float z;
            float? w;

            public static WFVertex Create(Vertex vertex)
            {
                return new WFVertex
                {
                    x = vertex.Position.x,
                    y = vertex.Position.y,
                    z = vertex.Position.z
                };
            }

            static Regex rx = new Regex(@"v ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);
        }

        public sealed class WFTextureCoordinate : WFGrammar
        {
            float x;
            float y;
            float? w;

            static Regex rx = new Regex(@"vt ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);
        }

        public sealed class WFVertexNormal : WFGrammar
        {
            float x;
            float y;
            float z;
            float? w;

            static Regex rx = new Regex(@"vn ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);
        }

        public sealed class WFFace : WFGrammar
        {
            public struct FaceIndex
            {
                int vertex;
                int coordinate;
                int normal;
            }

            List<FaceIndex> indices;

            static Regex rx = new Regex(@"f ((?<v>\d*)/(?<vt>\d*)/(?<vn>\d*) ?)+", RegexOptions.Compiled);
        }
    }

    public class WavefrontIO
    {
        public static void Parse(string str)
        {
            string[] lines = str.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {

            }

        }

        public static WFGrammar ParseLine(string line)
        {
            Regex rx = new Regex(@"# ", RegexOptions.Compiled);
            string[] words = line.Split(' ');
            if (words.Length > 0)
            {
                switch (words[0])
                {
                    case "#":
                        break;
                    case "mtllib":
                        break;
                    case "o":
                        break;
                    case "v":
                        break;
                    case "vt":
                        break;
                    case "vn":
                        break;
                    case "usemtl":
                        break;
                    case "s":
                        break;
                    case "f":
                        break;
                }
            }
            return null;
        }

        public static string Encode(ref SceneDescription objects)
        {
            String str = "";
            foreach (var obj in objects.objects)
            {
                str += EncodeOneObject(obj);
            }
            return str;
        }

        public static string EncodeOneObject(UMesh umesh)
        {
            List<WFGrammar.WFVertex> vertices = new();
            // vertex positions
            foreach (Vertex vertex in umesh.Vertices)
            {
                vertices.Add(WFGrammar.WFVertex.Create(vertex));
            }
            // texture coordinates
            Dictionary<float2, int> coordinateDict = new();
            // vertex normals
            Dictionary<float3, int> normalDict = new();

            int vtIndex = 0;
            int vnIndex = 0;
            foreach (Face face in umesh.Faces)
            {

                if (face.shading == ShadingType.Flat)
                // one averaged value
                {

                } else
                // all vertices separate
                {

                }
            }

            // actually do the faces


            throw new NotImplementedException { };
        }

        public static void Decode()
        {

            throw new NotImplementedException { };
        }



        string TestString = @"
# Blender v3.0.1 OBJ File: ''
# www.blender.org
mtllib test_export4.mtl
o Cube_Cube.001
v -1.000000 -1.000000 1.000000
v -1.000000 1.000000 1.000000
v -1.000000 -1.000000 -1.000000
v -1.000000 1.000000 -1.000000
v 1.000000 -1.000000 1.000000
v 1.000000 1.000000 1.000000
v 1.000000 -1.000000 -1.000000
v 1.000000 1.000000 -1.000000
vt 0.375000 0.000000
vt 0.625000 0.000000
vt 0.625000 0.250000
vt 0.375000 0.250000
vt 0.625000 0.500000
vt 0.375000 0.500000
vt 0.625000 0.750000
vt 0.375000 0.750000
vt 0.625000 1.000000
vt 0.375000 1.000000
vt 0.125000 0.500000
vt 0.125000 0.750000
vt 0.875000 0.500000
vt 0.875000 0.750000
vn -1.0000 0.0000 0.0000
vn 0.0000 0.0000 -1.0000
vn 1.0000 0.0000 0.0000
vn 0.0000 0.0000 1.0000
vn 0.0000 -1.0000 0.0000
vn 0.0000 1.0000 0.0000
usemtl None
s off
f 1/1/1 2/2/1 4/3/1 3/4/1
f 3/4/2 4/3/2 8/5/2 7/6/2
f 7/6/3 8/5/3 6/7/3 5/8/3
f 5/8/4 6/7/4 2/9/4 1/10/4
f 3/11/5 7/6/5 5/8/5 1/12/5
f 8/5/6 4/13/6 2/14/6 6/7/6
o Cube.001_Cube.002
v -2.511939 -1.534362 5.081775
v -2.511939 0.465638 5.081775
v -2.511939 -1.534362 3.081775
v -2.511939 0.465638 3.081775
v -0.511938 -1.534362 5.081775
v -0.511938 0.465638 5.081775
v -0.511938 -1.534362 3.081775
v -0.511938 0.465638 3.081775
vt 0.375000 0.000000
vt 0.625000 0.000000
vt 0.625000 0.250000
vt 0.375000 0.250000
vt 0.625000 0.500000
vt 0.375000 0.500000
vt 0.625000 0.750000
vt 0.375000 0.750000
vt 0.625000 1.000000
vt 0.375000 1.000000
vt 0.125000 0.500000
vt 0.125000 0.750000
vt 0.875000 0.500000
vt 0.875000 0.750000
vn -1.0000 0.0000 0.0000
vn 0.0000 0.0000 -1.0000
vn 1.0000 0.0000 0.0000
vn 0.0000 0.0000 1.0000
vn 0.0000 -1.0000 0.0000
vn 0.0000 1.0000 0.0000
usemtl None
s off
f 9/15/7 10/16/7 12/17/7 11/18/7
f 11/18/8 12/17/8 16/19/8 15/20/8
f 15/20/9 16/19/9 14/21/9 13/22/9
f 13/22/10 14/21/10 10/23/10 9/24/10
f 11/25/11 15/20/11 13/22/11 9/26/11
f 16/19/12 12/27/12 10/28/12 14/21/12
";

    }


}
