using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshReadWrite : MonoBehaviour
{

    [SerializeField]
    public GameObject vertexHighlight;

    GameObject[] highLightObjects;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        highLightObjects = new GameObject[vertices.Length];

        Debug.LogFormat("Number of vertices: {0}", vertices.Length);

        for (var i = 0; i < vertices.Length; i++)
        {
            highLightObjects[i] = Instantiate(vertexHighlight, vertices[i] + transform.position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            highLightObjects[i].transform.position = vertices[i] + transform.position;
        }
    }

    
}
