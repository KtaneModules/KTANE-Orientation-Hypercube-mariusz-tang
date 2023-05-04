using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hypercube : MonoBehaviour {

    private const float VERTEX_SCALE = 0.02147632f;

    [SerializeField] private GameObject _vertex;
    [SerializeField] private GameObject _edge;

    private GameObject _vertex1;
    private GameObject _vertex2;
    private GameObject _edge12;

    private void Start() {
        _vertex1 = Instantiate(_vertex, transform);
        _vertex1.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
        _vertex1.transform.localScale = Vector3.one * VERTEX_SCALE;
        _vertex1.transform.rotation = Quaternion.identity;
        _vertex2 = Instantiate(_vertex, transform);
        _vertex2.transform.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
        _vertex2.transform.localScale = Vector3.one * VERTEX_SCALE;
        _vertex2.transform.rotation = Quaternion.identity;
        _edge12 = Instantiate(_edge, transform);
        _edge12.transform.localPosition = 0.5f * (_vertex1.transform.localPosition + _vertex2.transform.localPosition);
        _edge12.transform.localScale *= Vector3.Distance(_vertex1.transform.localPosition, _vertex2.transform.localPosition);
        _edge12.transform.rotation = Quaternion.FromToRotation( Vector3.up, _vertex1.transform.position - _vertex2.transform.position);
    }
}
