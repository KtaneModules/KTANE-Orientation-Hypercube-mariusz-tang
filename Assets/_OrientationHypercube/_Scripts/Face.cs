using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour {

    [SerializeField] private Transform _hypercube;

    private MeshFilter _filter;
    private MeshRenderer _renderer;

    void Start() {
        _filter = GetComponent<MeshFilter>();

        _filter.mesh = new Mesh();
        _filter.mesh.Clear();
        _filter.mesh.vertices = new Vector3[] {
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
        };
        _filter.mesh.triangles = new int[] {
            0, 1, 2,
        };
        _filter.mesh.RecalculateNormals();
    }
}
