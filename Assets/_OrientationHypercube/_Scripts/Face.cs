using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Face : MonoBehaviour {

    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private Vertex[] _vertices;

    public string InitialDirection { get; private set; }
    public string CurrentDirection { get; private set; }
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
        InitialDirection = direction;
        CurrentDirection = direction;
    }

    public void ResetDirection() {
        InitialDirection = CurrentDirection;
    }

    public void UpdateVertices() {
        _filter.mesh.vertices = _vertices.Select(v => v.transform.localPosition).ToArray();
        _filter.mesh.RecalculateNormals();
    }

    public void UpdateCurrentDirection(string rotation) {
        if (rotation.Length != 2 || !"0123".Contains(rotation[0]) || !"0123".Contains(rotation[1])) {
            throw new ArgumentException($"\"{rotation}\" is not a valid rotation.");
        }
        rotation = $"{"XYZW"[rotation[0] - '0']}{"XYZW"[rotation[1] - '0']}";

        if (CurrentDirection[1] == rotation[0]) {
            CurrentDirection = $"{CurrentDirection[0]}{rotation[1]}";
        }
        else if (CurrentDirection[1] == rotation[1]) {
            CurrentDirection = $"{(CurrentDirection[0] == '-' ? "+" : "-")}{rotation[0]}";
        }
    }
}
