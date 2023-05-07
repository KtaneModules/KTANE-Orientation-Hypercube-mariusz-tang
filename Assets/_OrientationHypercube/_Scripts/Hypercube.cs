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

    private float _wobbleFactor = 0.005f;

    private string _rotation = string.Empty;
    private float _rotationAngle = 0;
    private float _rotationRate = 1;

    private Queue<string> _rotationQueue = new Queue<string>();

    private void Start() {
        _vertices = new List<Vertex>();

        for (int i = 0; i < 16; i++) {
            _vertices.Add(Instantiate(_vertex, transform).GetComponent<Vertex>());
            int x = 2 * (i % 2) - 1;
            int y = 2 * ((i >> 1) % 2) - 1;
            int z = 2 * ((i >> 2) % 2) - 1;
            int w = 2 * ((i >> 3) % 2) - 1;
            _vertices[i].InternalPosition4D = new Vector4(x, y, z, w);
        }

        _edges = new List<Edge>();

        for (int i = 0; i < _vertexPairs.GetLength(0); i++) {
            _edges.Add(Instantiate(_edge, transform).GetComponent<Edge>());
            _edges[i].AssignVertices(_vertices[_vertexPairs[i, 0]], _vertices[_vertexPairs[i, 1]]);
        }
    }

    private void Update() {
        if (_rotation.Length != 0) {
            RotateHypercube();
        }
        else if (_rotationQueue.Count != 0) {
            _rotation = _rotationQueue.Dequeue();
        }

        _vertices.ForEach(v => v.UpdateWobble(_wobbleFactor));
        _edges.ForEach(e => e.UpdatePosition());
    }

    private void RotateHypercube() {
        Matrix4x4 matrix;

        _rotationAngle += _rotationRate * Time.deltaTime;
        if (_rotationAngle < Mathf.PI / 2) {
            matrix = GetRotationMatrix(rotationIsComplete: false);
            _vertices.ForEach(v => v.Rotate4D(matrix));
        }
        else {
            matrix = GetRotationMatrix(rotationIsComplete: true);
            _vertices.ForEach(v => v.Rotate4D(matrix, setInternalPosition: true));
            GetNextRotation();
        }
    }

    private Matrix4x4 GetRotationMatrix(bool rotationIsComplete) {
        Matrix4x4 matrix = Matrix4x4.identity;
        int fromAxis = _rotation[0] - '0';
        int toAxis = _rotation[1] - '0';

        if (rotationIsComplete) {
            matrix[fromAxis, fromAxis] = 0;
            matrix[toAxis, toAxis] = 0;
            matrix[fromAxis, toAxis] = -1;
            matrix[toAxis, fromAxis] = 1;
        }
        else {
            matrix[fromAxis, fromAxis] = Mathf.Cos(_rotationAngle);
            matrix[toAxis, toAxis] = Mathf.Cos(_rotationAngle);
            matrix[fromAxis, toAxis] = -Mathf.Sin(_rotationAngle);
            matrix[toAxis, fromAxis] = Mathf.Sin(_rotationAngle);
        }

        return matrix;
    }

    private void GetNextRotation() {
        if (_rotationQueue.Count != 0) {
            _rotationAngle -= Mathf.PI / 2;
            _rotation = _rotationQueue.Dequeue();
        }
        else {
            _rotation = string.Empty;
            _rotationAngle = 0;
        }
    }

    public void QueueRotation(string rotation) {
        int result;
        if (rotation.Length != 2 || !int.TryParse(rotation, out result)) {
            throw new ArgumentException("Rotation should be in the form of a string of two digits.");
        }
        if (result < 0 || result / 10 >= 4 || result % 10 >= 4) {
            throw new ArgumentException("Rotation should contain only the digits 0-3.");
        }

        _rotationQueue.Enqueue(rotation);
    }
}
