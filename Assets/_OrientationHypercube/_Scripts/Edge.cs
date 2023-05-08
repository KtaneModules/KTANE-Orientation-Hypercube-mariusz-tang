using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class Edge : MonoBehaviour {

    private readonly Vector3 _defaultScale = new Vector3(0.00922564f, 0.5f, 0.00922564f);

    private Vertex[] _vertices;

    public void AssignVertices(params Vertex[] vertices) {
        if (vertices.Length != 2) {
            throw new RankException("An edge must have exactly two vertices.");
        }
        
        _vertices = vertices;
    }

    public void UpdatePosition() {
        transform.localPosition = 0.5f * (_vertices[0].transform.localPosition + _vertices[1].transform.localPosition);
        transform.localScale = new Vector3(_defaultScale.x, 0.5f * Vector3.Distance(_vertices[0].transform.localPosition, _vertices[1].transform.localPosition), _defaultScale.z);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, _vertices[0].transform.position - _vertices[1].transform.position);
    }
}
