using System;
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
    private readonly string[] _eyeDirections = new string[] {
        "front",
        "right",
        "back",
        "left",
    };

    [SerializeField] private Hypercube _hypercube;
    [SerializeField] private HyperStatusLight _statusLight;
    [SerializeField] private KMSelectable _statusLightButton;

    [SerializeField] private KMSelectable[] _rotationButtons;
    [SerializeField] private KMSelectable _setButton;

    [SerializeField] private KMSelectable[] _panel;
    [SerializeField] private KMSelectable _centrePanelButton;
    [SerializeField] private Animator _panelAnimator;
    [SerializeField] private TextMesh _cbText;

    [SerializeField] private Observer _eye;

    private static int _moduleCount = 0;
    private int _moduleId;

    private KMAudio _audio;
    private KMBombModule _module;
    private ReadGenerator _readGenerator;

    private string _axes = "XZYW";
    private int _eyeRotation;
    private int[] _signs = new int[] { 1, 1, -1, 1 };
    private List<string> _inputtedRotations = new List<string>();

    private string _highlightedFace = string.Empty;

    private bool _isPreviewMode = false;
    private bool _isRecovery = false;
    private bool _isBusy = false;
    private bool _isMuted = false;
    private bool _cbModeOn = false;

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
            button.OnInteractEnded += delegate () { PlaySound("Button Release"); };
        }
        _setButton.OnInteract += delegate () { StartCoroutine(HandleSetPress()); return false; };
        _setButton.OnInteractEnded += delegate () { PlaySound("Button Release"); };

        foreach (KMSelectable panelButton in _panel) {
            panelButton.OnHighlight += delegate () { HandleHover(panelButton); };
            panelButton.OnHighlightEnded += delegate () { HandleUnhover(panelButton); };
        }
        _centrePanelButton.OnInteract += delegate () { StartCoroutine(HandleCentrePress()); return false; };
        _statusLightButton.OnInteract += delegate () { PlaySound("Rotation"); _cbModeOn = !_cbModeOn; return false; };
    }

    private void Start() {
        _readGenerator.Generate();
        _hypercube.SetColours(_readGenerator.GetFaceColours());

        _eyeRotation = 0;
        for (int i = 0, j = Rnd.Range(0, 4); i < j; i++) {
            ShiftPerspectiveRight();
        }
        Log("-=-=-=- Start -=-=-=-");
        Log($"The observer starts off facing the {_eyeDirections[_eyeRotation]} face.");
    }

    private void HandleRotationPress(string buttonName) {
        PlaySound("Button Press");
        if (_isBusy || _isRecovery) {
            return;
        }

        if (_isPreviewMode) {
            _hypercube.QueueRotation(GetRotationDigits(_buttonToRotation[buttonName]));
        }
        else {
            Log($"Pressed {buttonName.ToLower()}.");
            _inputtedRotations.Add(GetRotationDigits(_buttonToRotation[buttonName]));
            if (buttonName != "Left" && buttonName != "Right") {
                // The observer can stay, move left, or move right with equal probability.
                int rng = Rnd.Range(0, 3);
                if (rng != 0) {
                    ShiftPerspectiveRight(reverse: rng == 1);
                    Log($"The observer moved to face the {_eyeDirections[_eyeRotation]}.");
                }
            }
        }
    }

    private IEnumerator HandleSetPress() {
        PlaySound("Button Press");

        if (_isBusy) {
            yield break;
        }
        if (_isRecovery) {
            _hypercube.ResetInitialFaceDirections();
            _hypercube.UpdateColours();
            Unhighlight();
            RehighlightFace();
            _isRecovery = false;
        }
        else if (!_isPreviewMode) {
            _isBusy = true;
            Log("-=-=-=- Submit -=-=-=-");
            PlaySound("Start Submission");
            yield return StartCoroutine(_hypercube.DisplayFromFaces(_readGenerator.FromFaces));
            _inputtedRotations.ForEach(r => _hypercube.QueueRotation(r));
            _inputtedRotations.Clear();

            while (_hypercube.IsBusy) {
                _hypercube.RotationRate += Time.deltaTime * 0.3f;
                yield return null;
            }

            KMAudio.KMAudioRef thinkingSound = _audio.PlaySoundAtTransformWithRef("Thinking", transform);
            yield return new WaitForSeconds(3);
            thinkingSound.StopSound();

            if (CorrectOrientation()) {
                StartCoroutine(SolveAnimation());
            }
            else {
                StartCoroutine(Strike("The faces did not get mapped to the correct places! Strike!"));
                _isBusy = false;
            }
        }
    }

    private void HandleHover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white;
        PlaySound("Hover");
        if (panelButton.transform.name != "Centre") {
            _highlightedFace = _panelButtonDirections[panelButton.transform.name];
        }

        if (_isBusy || _isPreviewMode) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            Highlight();
        }
    }

    private void HandleUnhover(KMSelectable panelButton) {
        panelButton.GetComponent<MeshRenderer>().material.color = Color.white * (49f / 255f);
        _highlightedFace = string.Empty;

        if (_isBusy || _isPreviewMode) {
            return;
        }

        if (panelButton.transform.name != "Centre") {
            Unhighlight();
        }
    }

    private void Highlight() {
        string direction;
        _hypercube.HighlightFace(_highlightedFace, out direction);
        if (_cbModeOn) {
            if (!_isRecovery) {
                _cbText.text = _readGenerator.GetCbText(_highlightedFace);
            }
            else {
                _cbText.text = GetRecoveryCbText(direction);
            }
        }
    }

    private string GetRecoveryCbText(string face) {
        if (_readGenerator.FromFaces.Contains(face)) {
            return "RGB"[Array.IndexOf(_readGenerator.FromFaces, face)].ToString();
        }
        return "K";
    }

    private void Unhighlight() {
        _cbText.text = string.Empty;
        _hypercube.EndHighlight();
    }

    private IEnumerator HandleCentrePress() {
        if (_isBusy || _isRecovery) {
            yield break;
        }
        if (_inputtedRotations.Count() > 0) {
            PlaySound("Cannot Change Mode");
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
            Highlight();
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

        _eyeRotation += 4 + (reverse ? -1 : 1);
        _eyeRotation %= 4;
        _eye.MoveRight(reverse);
    }

    public void Log(string message) {
        Debug.Log($"[Orientation Hypercube #{_moduleId}] {message}");
    }

    public IEnumerator Strike(string strikeMessage) {
        float elapsedTime = 0;
        float animationTime = 0.5f;

        _statusLight.StrikeFlash();
        PlaySound("Strike");
        _module.HandleStrike();

        yield return null;
        while (elapsedTime < animationTime) {
            elapsedTime += Time.deltaTime;
            _hypercube.WobbleFactor = Hypercube.BASE_WOBBLE_FACTOR * 510 * Mathf.Sin(elapsedTime / animationTime * Mathf.PI);
            yield return null;
        }
        _hypercube.WobbleFactor = Hypercube.BASE_WOBBLE_FACTOR;

        Log($"✕ {strikeMessage}");
        Log("-=-=-=- Reset -=-=-=-");
        _hypercube.RotationRate = 1;
        _isRecovery = true;
        RehighlightFace();
    }

    public void PlaySound(string soundName) {
        if (!_isMuted) {
            _audio.PlaySoundAtTransform(soundName, transform);
        }
    }

    private IEnumerator ModeChangeAnimation(bool setToPreviewMode) {
        _isBusy = true;
        _panelAnimator.SetTrigger("ModeChange");
        PlaySound("Mode Change");

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

        Unhighlight();
        if (setToPreviewMode) {
            _hypercube.HighlightFace("None");
            _eye.ToggleDefuserPerspective(true);
        }
        else {
            _eye.ToggleDefuserPerspective(false);
            RehighlightFace();
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
        _hypercube.ResetInitialFaceDirections();
    }

    private bool CorrectOrientation() {
        Face[] faces = _hypercube.GetFaces();
        string[] fromFaces = _readGenerator.FromFaces;
        string[] expectedToFaces = _readGenerator.ToFaces;
        string[] actualToFaces = new string[3];

        foreach (Face face in faces) {
            if (fromFaces.Contains(face.InitialDirection)) {
                actualToFaces[Array.IndexOf(fromFaces, face.InitialDirection)] = face.CurrentDirection;
            }
        }

        Log("The submitted rotations resulted in the following map:");
        Log($"Red: {fromFaces[0]} to {actualToFaces[0]}.");
        Log($"Green: {fromFaces[1]} to {actualToFaces[1]}.");
        Log($"Blue: {fromFaces[2]} to {actualToFaces[2]}.");

        return actualToFaces.Where((dir, ind) => dir != expectedToFaces[ind]).Count() == 0;
    }

    private IEnumerator SolveAnimation() {
        _isBusy = true;

        _hypercube.UpdateColours();
        Unhighlight();

        _module.HandlePass();
        _statusLight.SolvedState();
        PlaySound("Solve");
        _isMuted = true;
        Log("Submitted the correct orientation!");
        Log("-=-=-=- Solved -=-=-=-");

        float elapsedTime = 0;
        float[] rotationSpeeds = new float[3];
        float[] mobileOffsets = new float[3];
        float[] currentRotations = new float[3];

        for (int i = 0; i < 3; i++) {
            rotationSpeeds[i] = Rnd.Range(0.5f, 1.5f);
            mobileOffsets[i] = Rnd.Range(0.1f, 0.5f);
        }

        while (true) {
            elapsedTime += Time.deltaTime;

            // Done like this so that it always begins pointing upwards, ie. no rotation.
            currentRotations[0] = Mathf.Sin((rotationSpeeds[0] + mobileOffsets[0]) * elapsedTime);
            currentRotations[1] = Mathf.Cos((rotationSpeeds[1] + mobileOffsets[1]) * elapsedTime);
            currentRotations[2] = Mathf.Sin((rotationSpeeds[2] + mobileOffsets[2]) * elapsedTime);

            if (_hypercube.transform.localPosition.y < 0.2f) {
                _hypercube.transform.localPosition += Vector3.up * Time.deltaTime * 0.1f; ;
            }
            _hypercube.transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(currentRotations[0], currentRotations[1], currentRotations[2]));

            if (!_hypercube.IsBusy) {
                _hypercube.QueueRotation("03");
                _hypercube.QueueRotation("13");
                _hypercube.QueueRotation("23");
                _hypercube.QueueRotation("30");
                _hypercube.QueueRotation("31");
                _hypercube.QueueRotation("32");
            }
            yield return null;
        }
    }
}
