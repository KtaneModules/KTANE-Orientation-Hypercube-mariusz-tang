using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class OrientationHypercube : MonoBehaviour {

    private readonly Dictionary<string, string> _buttonToRotation = new Dictionary<string, string> {
        {"Left", "XY"},
        {"Right", "YX"},
        {"In", "WY"},
        {"Out", "YW"},
        {"Clock", "ZX"},
        {"Counter", "XZ"},
    };

    [SerializeField] private Hypercube _hypercube;
    [SerializeField] private KMSelectable[] _buttons;
    [SerializeField] private Observer _eye;

    private KMBombInfo _bomb;
    private KMAudio _audio;
    private KMBombModule _module;

    private string _axes = "XZYW";
    private int[] _signs = new int[] { 1, 1, -1, 1 };

    private void Awake() {
        _bomb = GetComponent<KMBombInfo>();
        _audio = GetComponent<KMAudio>();
        _module = GetComponent<KMBombModule>();

        foreach (KMSelectable button in _buttons) {
            button.OnInteract += delegate () { HandlePress(button.transform.name); return false; };
        }
    }

    private void HandlePress(string buttonName) {
        if (buttonName == "Set") {
            ShiftPerspectiveRight(true);
            return;
        }

        _hypercube.QueueRotation(GetRotationDigits(_buttonToRotation[buttonName]));
    }

    private string GetRotationDigits(string rotationLetters) {
        int fromDigit = _axes.IndexOf(rotationLetters[0]);
        int toDigit = _axes.IndexOf(rotationLetters[1]);

        if (_signs[fromDigit] != _signs[toDigit]) {
            return toDigit.ToString() + fromDigit.ToString();
        }
        return fromDigit.ToString() + toDigit.ToString();
    }

    private void ShiftPerspectiveRight(bool reverse = false) {
        int xPos = _axes.IndexOf('X');
        int yPos = _axes.IndexOf('Y');
        int tempSign = _signs[xPos];

        _axes = _axes.Replace('X', 'A').Replace('Y', 'X').Replace('A', 'Y');

        if (reverse != (_signs[yPos] == tempSign)) {
            _signs[xPos] = _signs[yPos];
            _signs[yPos] = -tempSign;
        }
        else {
            _signs[xPos] = -_signs[yPos];
            _signs[yPos] = tempSign;
        }

        _eye.MoveRight(reverse);
    }
}
