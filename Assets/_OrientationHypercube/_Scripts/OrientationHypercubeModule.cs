using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

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
    [SerializeField] private KMSelectable[] _buttons;
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

    private bool _isPreviewMode = false;
    private bool _isBusy = false;

    private void Awake() {
        _moduleId = _moduleCount++;

        _audio = GetComponent<KMAudio>();
        _module = GetComponent<KMBombModule>();
        _readGenerator = new ReadGenerator(this);

        foreach (KMSelectable button in _buttons) {
            button.OnInteract += delegate () { HandlePress(button.transform.name); return false; };
        }
        foreach (KMSelectable panelButton in _panel) {
            panelButton.OnHighlight += delegate () { HandleHover(panelButton); };
            panelButton.OnHighlightEnded += delegate () { HandleUnhover(panelButton); };
        }
        _centrePanelButton.OnInteract += delegate () { HandleCentrePress(); return false; };
    }

    private void Start() {
        _readGenerator.Generate();
        _hypercube.SetColours(_readGenerator.GetFaceColours());
    }

    private void HandlePress(string buttonName) {
        if (buttonName == "Set") {
            ShiftPerspectiveRight(true);
            return;
        }

        _hypercube.QueueRotation(GetRotationDigits(_buttonToRotation[buttonName]));
    }

    private void HandleHover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white;

        if (_isBusy || _isPreviewMode) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            _hypercube.HighlightFace(_panelButtonDirections[panelButton.transform.name]);
        }
    }

    private void HandleUnhover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white * (49f / 255f);

        if (_isBusy || _isPreviewMode) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            _hypercube.EndHighlight();
        }
    }

    private void HandleCentrePress() {
        if (_isBusy) {
            return;
        }
        StartCoroutine(ModeChangeAnimation(!_isPreviewMode));
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

    public void Log(string message) {
        Debug.Log($"[Orientation Hypercube #{_moduleId}] {message}");
    }

    public void Strike(string strikeMessage) {
        _module.HandleStrike();
        Log($"✕ {strikeMessage}");
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
        }
        else {
            _hypercube.EndHighlight();
        }
        _hypercube.transform.localScale = Vector3.zero;

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
    }
}
