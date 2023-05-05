using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Vertex : MonoBehaviour {

    private const float VERTEX_SCALE = 0.02147632f;
    private float WOBBLE_FACTOR = 0.005f;

    private Vector4 _position4D;
    private Vector3 _position3D;

    private float[] _frequencies = new float[3];

    public Vector4 Position4D {
        get { return _position4D; }
        set {
            _position4D = value;
            Project();
        }
    }

    private void Awake() {
        transform.localScale = Vector3.one * VERTEX_SCALE;
        for (int i = 0; i < 3; i++) {
            _frequencies[i] = Rnd.Range(0f, 2f);
        }
    }

    private void Project() {
        _position3D = new Vector3(Position4D.x, Position4D.y, Position4D.z) * 0.5f * Mathf.Pow(1.4f, Position4D.w - 1);
    }

    private float _elapsedTime = 0;

    private void Update() {
        _elapsedTime += Time.deltaTime;
        float x = Mathf.Sin(_frequencies[0] * _elapsedTime);
        float y = Mathf.Sin(_frequencies[1] * _elapsedTime);
        float z = Mathf.Sin(_frequencies[2] * _elapsedTime);
        transform.localPosition = _position3D + WOBBLE_FACTOR * new Vector3(x, y, z);
    }

}
