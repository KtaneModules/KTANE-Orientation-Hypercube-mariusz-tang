using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Vertex : MonoBehaviour {

    private const float VERTEX_SCALE = 0.02147632f;

    private Vector4 _internalPosition4D;
    private Vector4 _position4D;
    private Vector3 _position3D;

    private float _elapsedTime = 0;
    private float[] _frequencies = new float[3];

    public Vector4 Position4D {
        get { return _position4D; }
        set {
            _position4D = value;
            Project();
        }
    }

    public Vector4 InternalPosition4D {
        get { return _internalPosition4D; }
        set {
            _internalPosition4D = value;
            Position4D = value;
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

    public void UpdateWobble(float wobbleFactor) {
        _elapsedTime += Time.deltaTime;
        float x = Mathf.Sin(_frequencies[0] * _elapsedTime);
        float y = Mathf.Sin(_frequencies[1] * _elapsedTime);
        float z = Mathf.Sin(_frequencies[2] * _elapsedTime);
        transform.localPosition = _position3D + wobbleFactor * new Vector3(x, y, z);
    }

    public void Rotate4D(Matrix4x4 matrix, bool setInternalPosition = false) {
        // Unity seems to do something different to normal matrix multiplication when using the * operator,
        // so this method implements it in the desired way.
        Vector4 newPosition = new Vector4();

        for (int i = 0; i < 4; i++) {
            float newValue = 0;
            for (int j = 0; j < 4; j++) {
                newValue += InternalPosition4D[j] * matrix[i, j];
            }
            newPosition[i] = newValue;
        }

        if (setInternalPosition) {
            InternalPosition4D = newPosition;
        }
        else {
            Position4D = newPosition;
        }
    }

}
