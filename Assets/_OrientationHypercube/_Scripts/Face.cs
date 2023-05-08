using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour {

    [SerializeField] private Transform _hypercube;
    [SerializeField] private Material _material;

    private MeshFilter _filter;
    private MeshRenderer _renderer;

    void Start() {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();

        _filter.mesh = new Mesh {
            vertices = new Vector3[] {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
            },
            triangles = new int[] {
                0, 1, 3,
                0, 2, 3,
                0, 1, 5,
                0, 4, 5,
                0, 2, 6,
                0, 4, 6,
                7, 3, 1,
                7, 5, 1,
                7, 3, 2,
                7, 6, 2,
                7, 5, 4,
                7, 6, 4,
            },
        };
        _filter.mesh.Clear();

        _filter.mesh.SetVertices(new List<Vector3> {
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
        });
        _filter.mesh.SetTriangles(new int[] {
            0, 1, 3,
            0, 2, 3,
            0, 1, 5,
            0, 4, 5,
            0, 2, 6,
            0, 4, 6,
            7, 3, 1,
            7, 5, 1,
            7, 3, 2,
            7, 6, 2,
            7, 5, 4,
            7, 6, 4,
        }, 0);

        _renderer.material = _material;
    }

}
