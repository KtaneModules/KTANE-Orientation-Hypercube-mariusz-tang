using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Face : MonoBehaviour {

    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private Vertex[] _vertices;

    public string Direction { get; private set; }
    public Color Colour {
        get { return _renderer.material.color; }
        set { _renderer.material.color = value; }
    }

    private void Awake() {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        ConstructMesh();
    }

    private void ConstructMesh() {
        Vector3[] vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
        };

        _filter.mesh = new Mesh {
            vertices = vertices,
            triangles = new int[] {
                0, 1, 5,
                0, 5, 4,
                0, 6, 2,
                0, 4, 6,
                0, 3, 1,
                0, 2, 3,
                7, 5, 1,
                7, 4, 5,
                7, 2, 6,
                7, 6, 4,
                7, 1, 3,
                7, 3, 2,
            }
        };
        _filter.mesh.RecalculateNormals();
    }

    public void AssignVertices(Vertex[] vertices, string direction) {
        if (vertices.Length != 8) {
            throw new RankException("A hyperface needs eight vertices.");
        }
        if (direction.Length != 2 || !"+-".Contains(direction[0]) || !"XYZW".Contains(direction[1])) {
            throw new ArgumentException("The face direction must be of the form <+/-><X/Y/Z/W>.");
        }

        _vertices = vertices;
        Direction = direction;
    }

    public void UpdateVertices() {
        _filter.mesh.vertices = _vertices.Select(v => v.transform.localPosition).ToArray();
        _filter.mesh.RecalculateNormals();
    }
}
