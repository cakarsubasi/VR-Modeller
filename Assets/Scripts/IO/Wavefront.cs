using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Meshes;
using UnityEngine;
using System.Text;

using Unity.Mathematics;

#nullable enable

namespace MeshesIO
{

    public class SceneDescription
    {
        public List<UMesh> objects = new();
        public List<Vector3> worldCoordinates = new();
        public List<Matrix4x4> transforms = new();
    }

    public abstract class WFGrammar
    {

        public sealed class WFComment : WFGrammar
        {
            public readonly string comment;

            private WFComment(String comment)
            {
                this.comment = comment;
            }

            public static Regex rx = new Regex(@"# (?<comment>.+)", RegexOptions.Compiled);

            public static WFComment? Match(string line)
            {
                MatchCollection matches = rx.Matches(line);

                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    String comment =
                    groups["comment"].Value;

                    return new WFComment(comment);
                }
                return null;
            }

            public override string ToString()
            {
                return FormattableString.Invariant($"# {comment}");
            }
        }


        public sealed class WFMTLLib : WFGrammar
        {
            string file;

            private WFMTLLib(string file)
            {
                this.file = file;
            }

            public static Regex rx = new Regex(@"mtllib (.+)", RegexOptions.Compiled);
        }
        public sealed class WFObject : WFGrammar
        {
            public readonly string name;
            public int positionBeginning = 0;
            public int uvBeginning = 0;
            public int normalBeginning = 0;
            public readonly List<WFVertex> positions;
            public readonly List<WFTextureCoordinate> uvs;
            public readonly List<WFVertexNormal> normals;
            public readonly List<WFFace> faces;

            private WFObject(string name)
            {
                this.name = name;
                positions = new();
                uvs = new();
                normals = new();
                faces = new();
            }

            public WFObject(string name, 
                List<WFVertex> positions,
                List<WFTextureCoordinate> uvs,
                List<WFVertexNormal> normals,
                List<WFFace> faces)
            {
                this.name = name;
                this.positions = positions;
                this.uvs = uvs;
                this.normals = normals;
                this.faces = faces;
            }

            public static Regex rx = new Regex(@"o (?<name>.+)", RegexOptions.Compiled);

            public static WFObject? Match(string line)
            {
                MatchCollection matches = rx.Matches(line);

                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    String name =
                    groups["name"].Value;

                    return new WFObject(name);
                }
                return null;
            }

            public UMesh? IntoUMesh()
            {
                UMesh mesh = UMesh.Create();
                mesh.Name = name;

                // create vertices
                foreach (var position in positions)
                {
                    mesh.CreateVertex(position.x, position.y, position.z);
                }
                List<Vertex> verts = new();
                // create faces
                foreach (var face in faces)
                {
                    verts.Clear();
                    foreach (var index in face.indices)
                    {
                        int positionIndex = index.vertex - positionBeginning - 1;
                        verts.Add(mesh.Vertices[positionIndex]);
                        //int textureCoordinateIndex = 
                    }
                    mesh.CreateNGon(verts);
                    // todo apply normals and texture coordinates
                }

                return mesh;
            }

            public override string ToString()
            {
                StringBuilder str = new(FormattableString.Invariant($"o {name}\n"));

                foreach (var pos in positions)
                {
                    str.AppendLine(pos.ToString());
                }

                foreach (var vt in uvs)
                {
                    str.AppendLine(vt.ToString());
                }

                foreach (var vn in normals)
                {
                    str.AppendLine(vn.ToString());
                }

                foreach (var f in faces)
                {
                    str.AppendLine(f.ToString());
                }

                return str.ToString();
            }


        }

        public sealed class WFVertex : WFGrammar
        {
            public readonly float x;
            public readonly float y;
            public readonly float z;
            public readonly float? w;

            public WFVertex(float3 position) : this(position.x, position.y, position.z) { }
            
            public WFVertex(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public WFVertex(float x, float y, float z, float w) : this(x, y, z)
            {
                this.w = w;
            }

            public static Regex rx = new Regex(@"v ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);

            public static WFVertex? Match(string line)
            {
                Match match = rx.Match(line);
                Group coords = match.Groups["coor"];
                
                if (coords.Captures.Count == 3)
                {
                    return new WFVertex(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[2].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }
                else if (coords.Captures.Count == 4)
                {
                    return new WFVertex(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[2].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[3].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }
                
                return null;
            }

            public override string ToString()
            {
                return FormattableString.Invariant($"v {x:F6} {y:F6} {z:F6} {w:F6}");

            }
        }

        public sealed class WFTextureCoordinate : WFGrammar
        {
            public readonly float x;
            public readonly float y;
            public readonly float? w;

            private WFTextureCoordinate(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            private WFTextureCoordinate(float x, float y, float w)
            {
                this.x = x;
                this.y = y;
                this.w = w;
            }

            public static Regex rx = new Regex(@"vt ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);

            public static WFTextureCoordinate? Match(string line)
            {
                Match match = rx.Match(line);
                Group coords = match.Groups["coor"];

                if (coords.Captures.Count == 2)
                {
                    return new WFTextureCoordinate(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }
                else if (coords.Captures.Count == 3)
                {
                    return new WFTextureCoordinate(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[2].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }

                return null;
            }

            public override string ToString()
            {

                return FormattableString.Invariant($"v {x} {y} {w}");
            }
        }

        public sealed class WFVertexNormal : WFGrammar
        {
            public readonly float x;
            public readonly float y;
            public readonly float z;
            public readonly float? w;

            private WFVertexNormal(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            private WFVertexNormal(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public static Regex rx = new Regex(@"vn ((?<coor>-?\d\.\d*) ?)+", RegexOptions.Compiled);

            public static WFVertexNormal? Match(string line)
            {
                Match match = rx.Match(line);
                Group coords = match.Groups["coor"];

                if (coords.Captures.Count == 3)
                {
                    return new WFVertexNormal(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[2].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }
                else if (coords.Captures.Count == 4)
                {
                    return new WFVertexNormal(
                        float.Parse(coords.Captures[0].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[1].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[2].Value, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(coords.Captures[3].Value, System.Globalization.NumberFormatInfo.InvariantInfo));
                }

                return null;
            }

            public override string ToString()
            {
                 return FormattableString.Invariant($"v {x} {y} {z} {w}");
            }
        }

        public sealed class WFFace : WFGrammar
        {
            public struct FaceIndex
            {
                public int vertex;
                public int? coordinate;
                public int? normal;
            }

            public readonly List<FaceIndex> indices;

            public WFFace()
            {
                indices = new();
            }

            public WFFace(List<FaceIndex> indices)
            {
                this.indices = indices;
            }

            public static Regex rx = new Regex(@"f ((?<v>\d+)(/(?<vt>\d*))?(/(?<vn>\d*))? ?)+", RegexOptions.Compiled);

            public static WFFace? Match(String line)
            {
                Match match = rx.Match(line);

                if (!match.Success)
                {
                    return null;
                }

                Group positions = match.Groups["v"];
                Group coordinates = match.Groups["vt"];
                Group normals = match.Groups["vn"];

                List<FaceIndex> indices = new();

                for (int i = 0; i < positions.Captures.Count; ++i)
                {
                    Capture position = positions.Captures[i];
                    int pos = int.Parse(position.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                    // texture coordinate if exists
                    int? vt = null;
                    if (i < coordinates.Captures.Count)
                    {
                        vt = int.Parse(coordinates.Captures[i].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    // vertex normal if exists
                    int? vn = null;
                    if (i < normals.Captures.Count)
                    {
                        vn = int.Parse(normals.Captures[i].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }

                    FaceIndex index = new FaceIndex
                    {
                        vertex = pos,
                        coordinate = vt,
                        normal = vn
                    };
                    indices.Add(index);
                }

                return new WFFace(indices);
            }

            public override string ToString()
            {
                string ret = "f";
                foreach (FaceIndex idx in indices)
                {
                    ret = FormattableString.Invariant($"{ret} {idx.vertex}");
                    if (idx.coordinate != null && idx.normal != null)
                    {
                        ret = FormattableString.Invariant($"{ret}/{idx.coordinate}/{idx.normal}");
                    }
                    else if (idx.coordinate != null)
                    {
                        ret = FormattableString.Invariant($"{ret}/{idx.coordinate}");
                    }
                    else if (idx.normal != null)
                    {
                        ret = FormattableString.Invariant($"{ret}//{idx.normal}");
                    }
                }
                return ret;
            }
        }
    }

    public class WavefrontIO
    {

        /// <summary>
        /// Convert an obj string into a scene description
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static SceneDescription Parse(string str)
        {
            string[] lines = str.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<WFGrammar.WFObject> objects = new();
            int positionBeginning = 0;
            int uvBeginning = 0;
            int normalBeginning = 0;

            foreach (string line in lines)
            {
                var comment = WFGrammar.WFComment.Match(line);
                if (comment != null)
                {
                    // ignore comments
                    continue;
                }

                var meshObj = WFGrammar.WFObject.Match(line);
                if (meshObj != null)
                {
                    // update the indices
                    if (objects.Count != 0)
                    {
                        positionBeginning += objects[^1].positions.Count;
                        uvBeginning += objects[^1].uvs.Count;
                        normalBeginning += objects[^1].normals.Count;
                    }
                    meshObj.positionBeginning = positionBeginning;
                    meshObj.uvBeginning = uvBeginning;
                    meshObj.normalBeginning = normalBeginning;
                    objects.Add(meshObj); // add a new mesh obj
                    continue;
                }

                var vertexPosition = WFGrammar.WFVertex.Match(line);
                if (vertexPosition != null)
                {
                    objects[^1].positions.Add(vertexPosition);
                    continue;
                }

                var vertexTexCoord = WFGrammar.WFTextureCoordinate.Match(line);
                if (vertexTexCoord != null)
                {
                    objects[^1].uvs.Add(vertexTexCoord);
                    continue;
                }

                var vertexNormal = WFGrammar.WFVertexNormal.Match(line);
                if (vertexNormal != null)
                {
                    objects[^1].normals.Add(vertexNormal);
                    continue;
                }

                var face = WFGrammar.WFFace.Match(line);
                if (face != null)
                {
                    objects[^1].faces.Add(face);
                    continue;
                }
            }

            var scene = new SceneDescription();
            foreach (var obj in objects)
            {
                var umeshMaybe = obj.IntoUMesh();
                if (umeshMaybe != null)
                {
                    scene.objects.Add((UMesh)umeshMaybe);
                }
            }

            return scene;
        }

        /// <summary>
        /// Convert a scene description into a 
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static string Unparse(SceneDescription scene)
        {
            int vCount = 1;
            int vtCount = 1;
            int vnCount = 1;

            List<WFGrammar.WFObject> objects = new();

            for (int i = 0; i < scene.objects.Count; ++i)
            {
                UMesh mesh = scene.objects[i];
                WFGrammar.WFObject wfObject = EncodeOneObject(mesh, ref vCount, ref vtCount, ref vnCount);
                objects.Add(wfObject);
            }

            StringBuilder text = new();
            foreach (var obj in objects)
            {
                text.AppendLine(obj.ToString());
            }

            return text.ToString();
        }


        internal static WFGrammar.WFObject EncodeOneObject(UMesh umesh, ref int vCount, ref int vtCount, ref int vnCount)
        {
            List<WFGrammar.WFVertex> vertices = new();
            List<WFGrammar.WFTextureCoordinate> vts = new();
            List<WFGrammar.WFVertexNormal> vns = new();
            List<WFGrammar.WFFace> faces = new();

            // texture coordinates
            Dictionary<float2, int?> coordinateDict = new();
            // vertex normals
            Dictionary<float3, int?> normalDict = new();

            // vertex positions
            foreach (Vertex vertex in umesh.Vertices)
            {
                vertex.Index = vCount;
                vCount++;
                vertices.Add(new WFGrammar.WFVertex(vertex.Position));
            }

            // actually do the faces
            foreach (Face face in umesh.Faces)
            {
                WFGrammar.WFFace wfFace = new();
                
                foreach (var vertInfo in face.GetVertexInfo(umesh.Shading))
                {
                    wfFace.indices.Add(new WFGrammar.WFFace.FaceIndex
                    {
                        vertex = vertInfo.Index,
                        coordinate = coordinateDict.GetValueOrDefault(vertInfo.uv0, null),
                        normal = normalDict.GetValueOrDefault(vertInfo.normal, null)
                    });
                }

                faces.Add(wfFace);
            }

            WFGrammar.WFObject wfObject = new(umesh.Name, vertices, vts, vns, faces);

            vtCount += vts.Count;
            vnCount += vns.Count;

            return wfObject;
        }

    }


}
