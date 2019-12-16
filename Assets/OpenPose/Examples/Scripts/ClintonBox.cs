using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClintonBox : MonoBehaviour
{
    private void Start()
    {        

    }

    public void UpdateMesh(Vector2[] points)
    {
        var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(points, v => v);

        // Use the triangulator to get indices for creating triangles
        var triangulator = new Triangulator(points);
        var indices = triangulator.Triangulate();

        // Generate a color for each vertex
        var colors = Enumerable.Range(0, vertices3D.Length)
            .Select(i => new Color(1, 1, 1, 0.7f)) // white with transparency
            .ToArray();

        // Create the mesh
        var mesh = new Mesh
        {
            vertices = vertices3D,
            triangles = indices,
            colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Set up game object with material and mesh
        gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    public bool Contains(Vector3 point)
    {
        return gameObject.GetComponent<MeshFilter>().mesh.bounds.Contains(point);
    }
}
