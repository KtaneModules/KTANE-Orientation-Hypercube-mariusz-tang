﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class OrientationHypercubeModule : MonoBehaviour {

    private readonly Dictionary<string, string> _buttonToRotation = new Dictionary<string, string> {
        {"Left", "XY"},
        {"Right", "YX"},
        {"In", "WY"},
        {"Out", "YW"},
        {"Clock", "ZX"},
        {"Counter", "XZ"},
    };
    private readonly Dictionary<string, string> _panelButtonDirections = new Dictionary<string, string> {
        { "Right Inner", "+X"},
        { "Left Inner", "-X"},
        { "Top Outer", "+Y"},
        { "Bottom Outer", "-Y"},
        { "Top Inner", "+Z"},
        { "Bottom Inner", "-Z"},
        { "Right Outer", "+W"},
        { "Left Outer", "-W"},
    };

    [SerializeField] private Hypercube _hypercube;
    [SerializeField] private KMSelectable[] _rotationButtons;
    [SerializeField] private KMSelectable _setButton;
    [SerializeField] private KMSelectable[] _panel;
    [SerializeField] private KMSelectable _centrePanelButton;
    [SerializeField] private Animator _panelAnimator;
    [SerializeField] private Observer _eye;

    private static int _moduleCount = 0;
    private int _moduleId;

    private KMAudio _audio;
    private KMBombModule _module;
    private ReadGenerator _readGenerator;

    private string _axes = "XZYW";
    private int[] _signs = new int[] { 1, 1, -1, 1 };
    private List<string> _inputtedRotations = new List<string>();

    private string _highlightedFace = string.Empty;

    private bool _isPreviewMode = false;
    private bool _isBusy = false;

    private void Awake() {
        _moduleId = _moduleCount++;

        _audio = GetComponent<KMAudio>();
        _module = GetComponent<KMBombModule>();
        _readGenerator = new ReadGenerator(this);

        AssignInteractionHandlers();
    }

    private void AssignInteractionHandlers() {
        foreach (KMSelectable button in _rotationButtons) {
            button.OnInteract += delegate () { HandleRotationPress(button.transform.name); return false; };
        }
        _setButton.OnInteract += delegate () { StartCoroutine(HandleSetPress()); return false; };

        foreach (KMSelectable panelButton in _panel) {
            panelButton.OnHighlight += delegate () { HandleHover(panelButton); };
            panelButton.OnHighlightEnded += delegate () { HandleUnhover(panelButton); };
        }
        _centrePanelButton.OnInteract += delegate () { StartCoroutine(HandleCentrePress()); return false; };
    }

    private void Start() {
        _readGenerator.Generate();
        _hypercube.SetColours(_readGenerator.GetFaceColours());
    }

    private void HandleRotationPress(string buttonName) {
        if (_isBusy) {
            return;
        }

        if (_isPreviewMode) {
            _hypercube.QueueRotation(GetRotationDigits(_buttonToRotation[buttonName]));
        }
        else {
            _inputtedRotations.Add(GetRotationDigits(_buttonToRotation[buttonName]));
            if (buttonName != "Left" && buttonName != "Right") {
                // The observer can stay, move left, or move right with equal probability.
                int rng = Rnd.Range(0, 3);
                if (rng != 0) {
                    ShiftPerspectiveRight(reverse: rng == 1);
                }
            }
        }
    }

    private IEnumerator HandleSetPress() {
        if (!_isPreviewMode) {
            _isBusy = true;
            _inputtedRotations.ForEach(r => _hypercube.QueueRotation(r));
            _inputtedRotations.Clear();

            yield return new WaitUntil(() => !_hypercube.IsBusy);
            if (CorrectOrientation()) {
                _module.HandlePass();
            }
            else {
                Strike("Strike lol.");
                _isBusy = false;
            }
        }
    }

    private void HandleHover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white;
        if (panelButton.transform.name != "Centre") {
            _highlightedFace = _panelButtonDirections[panelButton.transform.name];
        }

        if (_isBusy || _isPreviewMode || _inputtedRotations.Count() > 0) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            _hypercube.HighlightFace(_highlightedFace);
        }
    }

    private void HandleUnhover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white * (49f / 255f);
        _highlightedFace = string.Empty;

        if (_isBusy || _isPreviewMode || _inputtedRotations.Count() > 0) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            _hypercube.EndHighlight();
        }
    }

    private IEnumerator HandleCentrePress() {
        if (_isBusy || _inputtedRotations.Count() > 0) {
            yield break;
        }

        _isBusy = true;
        _hypercube.UpdateColours();
        _hypercube.StopRotations();
        yield return new WaitUntil(() => !_hypercube.IsBusy);
        StartCoroutine(ModeChangeAnimation(!_isPreviewMode));
    }

    private void RehighlightFace() {
        if (_highlightedFace.Length != 0) {
            _hypercube.HighlightFace(_highlightedFace);
        }
    }

    private string GetRotationDigits(string rotationLetters) {
        string axesToUse = _isPreviewMode ? "XYZW" : _axes;
        int[] signsToUse = _isPreviewMode ? new int[] { 1, 1, 1, 1 } : _signs;

        int fromDigit = axesToUse.IndexOf(rotationLetters[0]);
        int toDigit = axesToUse.IndexOf(rotationLetters[1]);

        if (signsToUse[fromDigit] != signsToUse[toDigit]) {
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

    public void Log(string message) {
        Debug.Log($"[Orientation Hypercube #{_moduleId}] {message}");
    }

    public void Strike(string strikeMessage) {
        _module.HandleStrike();
        Log($"✕ {strikeMessage}");
        _hypercube.ResetInitialFaceDirections();
        RehighlightFace();
    }

    private IEnumerator ModeChangeAnimation(bool setToPreviewMode) {
        _isBusy = true;
        _panelAnimator.SetTrigger("ModeChange");

        float elapsedTime = 0;
        float animationTime = 1;

        yield return null;
        while (elapsedTime < animationTime * 0.5f) {
            elapsedTime += Time.deltaTime;
            float offset = -Mathf.Sin(elapsedTime * Mathf.PI / animationTime);
            _hypercube.transform.localScale = Vector3.one * (1 + offset);
            _hypercube.transform.localPosition = new Vector3(0, 0.5f * offset, 0);
            yield return null;
        }

        if (setToPreviewMode) {
            _hypercube.HighlightFace("None");
            _eye.ToggleDefuserPerspective(true);
        }
        else {
            _hypercube.EndHighlight();
            _eye.ToggleDefuserPerspective(false);
        }
        _hypercube.transform.localScale = Vector3.zero;

        if (!setToPreviewMode) {
            RehighlightFace();
        }

        while (elapsedTime < animationTime) {
            elapsedTime += Time.deltaTime;
            float offset = -Mathf.Sin(elapsedTime * Mathf.PI / animationTime);
            _hypercube.transform.localScale = Vector3.one * (1 + offset);
            _hypercube.transform.localPosition = new Vector3(0, 0.5f * offset, 0);
            yield return null;
        }

        _hypercube.transform.localScale = Vector3.one;
        _hypercube.transform.localPosition = Vector3.zero;
        _isBusy = false;
        _isPreviewMode = setToPreviewMode;
        _hypercube.ResetInitialFaceDirections();
    }

    private bool CorrectOrientation() {
        Face[] faces = _hypercube.GetFaces();
        string[] fromFaces = _readGenerator.FromFaces;
        string[] toFaces = _readGenerator.ToFaces;

        foreach (Face face in faces) {
            if (fromFaces.Contains(face.InitialDirection) && toFaces[Array.IndexOf(fromFaces, face.InitialDirection)] != face.CurrentDirection) {
                return false;
            }
        }
        return true;
    }
}
