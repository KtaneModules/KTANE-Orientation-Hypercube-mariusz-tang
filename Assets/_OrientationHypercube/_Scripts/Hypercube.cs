using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class Hypercube : MonoBehaviour {

    private readonly int[,] _vertexPairs = new int[,] {
        { 0, 1 },
        { 1, 3 },
        { 3, 2 },
        { 2, 0 },
        { 4, 5 },
        { 5, 7 },
        { 7, 6 },
        { 6, 4 },
        { 8, 9 },
        { 9, 11 },
        { 11, 10 },
        { 10, 8 },
        { 12, 13 },
        { 13, 15 },
        { 15, 14 },
        { 14, 12 },
        { 0, 4 },
        { 0, 8 },
        { 12, 4 },
        { 12, 8 },
        { 1, 5 },
        { 1, 9 },
        { 13, 5 },
        { 13, 9 },
        { 2, 6 },
        { 2, 10 },
        { 14, 6 },
        { 14, 10 },
        { 3, 7 },
        { 3, 11 },
        { 15, 7 },
        { 15, 11 },
    };

    [SerializeField] private GameObject _vertex;
    [SerializeField] private GameObject _edge;

    private List<Vertex> _vertices;
    private List<Edge> _edges;

    private void Start() {
        _vertices = new List<Vertex>();

        for (int i = 0; i < 16; i++) {
            _vertices.Add(Instantiate(_vertex, transform).GetComponent<Vertex>());
            int x = 2 * (i % 2) - 1;
            int y = 2 * ((i >> 1) % 2) - 1;
            int z = 2 * ((i >> 2) % 2) - 1;
            int w = 2 * ((i >> 3) % 2) - 1;
            _vertices[i].Position4D = new Vector4(x, y, z, w);
        }

        _edges = new List<Edge>();

        for (int i = 0; i < _vertexPairs.GetLength(0); i++) {
            _edges.Add(Instantiate(_edge, transform).GetComponent<Edge>());
            _edges[i].AssignVertices(_vertices[_vertexPairs[i, 0]], _vertices[_vertexPairs[i, 1]]);
        }
    }
}
