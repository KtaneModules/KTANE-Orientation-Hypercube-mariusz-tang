using System.Collections;
using UnityEngine;

using Rnd = UnityEngine.Random;

public class HyperStatusLight : MonoBehaviour {

    private const float X = 0.075167f;
    private const float Y_MIDPOINT = 0.0392f;
    private const float Y_AMIPLITUDE = 0.005f;
    private const float Z = 0.076057f;

    private float[] _rotationSpeeds = new float[3];
    private float[] _mobileOffsets = new float[3];
    private float[] _currentRotations = new float[3];
    private float _moveFrequency;
    private float _elapsedTime = 0;

    private Material[] _materials;
    private Color _defaultColour;
    private Coroutine _strikeFlash;

    private void Awake() {
        _materials = GetComponent<MeshRenderer>().materials;
        _defaultColour = _materials[0].color;

        _moveFrequency = Rnd.Range(0.5f, 1.5f);

        for (int i = 0; i < 3; i++) {
            _rotationSpeeds[i] = Rnd.Range(0.5f, 1.5f);
            _mobileOffsets[i] = Rnd.Range(0.1f, 0.5f);
        }
    }

    // Update is called once per frame
    private void Update() {
        _elapsedTime += Time.deltaTime;
        for (int i = 0; i < 3; i++) {
            _currentRotations[i] = Mathf.Sin((_rotationSpeeds[i] + _mobileOffsets[i]) * _elapsedTime);
        }
        transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(_currentRotations[0], _currentRotations[1], _currentRotations[2]));
        transform.localPosition = new Vector3(X, Y_MIDPOINT + Y_AMIPLITUDE * Mathf.Sin(_moveFrequency * _elapsedTime), Z);
    }

    public void StrikeFlash() {
        if (_strikeFlash != null) {
            StopCoroutine(_strikeFlash);
        }

        _strikeFlash = StartCoroutine(StrikeFlashAnimation());
    }

    private IEnumerator StrikeFlashAnimation() {
        for (int i = 0; i < _materials.Length; i++) {
            yield return new WaitForSeconds(0.05f);
            _materials[i].color = new Color(1, Rnd.Range(0, 0.2f), Rnd.Range(0, 0.2f));
        }
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < _materials.Length; i++) {
            yield return new WaitForSeconds(0.05f);
            _materials[i].color = _defaultColour;
        }
    }

    public void SolvedState() {
        if (_strikeFlash != null) {
            StopCoroutine(_strikeFlash);
        }

        StartCoroutine(SolveStateAnimation());
    }

    private IEnumerator SolveStateAnimation() {
        while (true) {
            for (int i = 0; i < _materials.Length; i++) {
                yield return new WaitForSeconds(0.05f);
                _materials[i].color = new Color(Rnd.Range(0, 0.4f), Rnd.Range(0.7f, 1), Rnd.Range(0, 0.4f));
            }
        }
    }
}
