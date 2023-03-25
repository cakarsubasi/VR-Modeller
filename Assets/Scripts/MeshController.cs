using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    public GameObject vertexHighlight;

    Mesh mesh;
    MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public Mesh Mesh { get => mesh; set => mesh = value; }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        Vertices = mesh.vertices.ToList();

        //Debug.Log("Vertices: " + Vertices.Count);

        List<Vector3> updatedVertices = new List<Vector3>();

        for (var i = 0; i < Vertices.Count; i++)
        {
            List<Vector3> sameVertices = Vertices.FindAll(x => x == Vertices[i]);

            if (!updatedVertices.Contains(sameVertices[0]))
            {
                updatedVertices.Add(sameVertices[0]);

                List<int> vertexIndexes = new List<int>();
                for (int k = 0; k < Vertices.Count; k++)
                {
                    if (Vertices[k] == sameVertices[0])
                    {
                        vertexIndexes.Add(k);
                        //Debug.Log(sameVertices[0] + " " + k);
                    }
                }
                GameObject vertex = Instantiate(vertexHighlight, gameObject.transform);
                vertex.transform.localPosition = vertices[i];
                vertex.transform.localScale = transform.lossyScale / (Mathf.Pow(transform.lossyScale.x, 2) * 10);
                vertex.GetComponent<VertexController>().AssignedVertices = vertexIndexes.ToArray();
            }
        }

    }
}
