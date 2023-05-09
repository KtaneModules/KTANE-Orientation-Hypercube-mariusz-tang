using System.Collections;
using System.Collections.Generic;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ReadGenerator {

    private const string AXES = "XYZW";
    private readonly Dictionary<char, string> _axisBits = new Dictionary<char, string> {
        { 'X', "00" },
        { 'Y', "01" },
        { 'Z', "10" },
        { 'W', "11" },
    };

    private OrientationHypercubeModule _module;
    private KMBombInfo _bomb;
    // Generation happens in reverse.
    private List<string> _reversedLogging = new List<string>();

    private string[] _positiveBinaries = new string[3];
    private string[] _negativeBinaries = new string[3];

    public string[] FromFaces { get; private set; }
    public string[] ToFaces { get; private set; }

    public ReadGenerator(OrientationHypercubeModule module) {
        _module = module;
        _bomb = _module.GetComponent<KMBombInfo>();
    }

    public void Generate() {
        GetFaceMappings();
        GetFinalBinaries();
        DoSerialNumberInversions();
        DoShifts();

        _reversedLogging.Reverse();
        foreach (string message in _reversedLogging) {
            _module.Log(message);
        }
    }

    private void GetFaceMappings() {
        string fromAxes = "XYZW";
        string toAxes = "XYZW";
        string signs = "-+";

        FromFaces = new string[3];
        ToFaces = new string[3];

        for (int i = 0; i < 3; i++) {
            FromFaces[i] = signs[Rnd.Range(0, 2)].ToString();
            ToFaces[i] = signs[Rnd.Range(0, 2)].ToString();

            int axisNumber = Rnd.Range(0, fromAxes.Length);
            FromFaces[i] += fromAxes[axisNumber];
            fromAxes = fromAxes.Remove(axisNumber, 1);

            axisNumber = Rnd.Range(0, toAxes.Length);
            // Prevent mapping a face to itself.
            if (toAxes[axisNumber] == FromFaces[i][1] && FromFaces[i][0] == ToFaces[i][0]) {
                axisNumber += Rnd.Range(1, toAxes.Length);
                axisNumber %= toAxes.Length;
            }
            ToFaces[i] += toAxes[axisNumber];
            toAxes = toAxes.Remove(axisNumber, 1);
        }

        _reversedLogging.Add($"Blue: {FromFaces[2]} to {ToFaces[2]}.");
        _reversedLogging.Add($"Green: {FromFaces[1]} to {ToFaces[1]}.");
        _reversedLogging.Add($"Red: {FromFaces[0]} to {ToFaces[0]}.");
        _reversedLogging.Add("Final face mappings:");
    }

    private void GetFinalBinaries() {
        for (int i = 0; i < 3; i++) {
            int thirdBit = Rnd.Range(0, 2);
            _negativeBinaries[i] = _axisBits[FromFaces[i][1]];
            _negativeBinaries[i] += thirdBit;

            if (FromFaces[i][0] == '-') {
                _negativeBinaries[i] += 1 - thirdBit;
            }
            else {
                _negativeBinaries[i] += thirdBit;
            }

            thirdBit = Rnd.Range(0, 2);
            _positiveBinaries[i] = _axisBits[ToFaces[i][1]];
            _positiveBinaries[i] += thirdBit;

            if (ToFaces[i][0] == '-') {
                _positiveBinaries[i] += 1 - thirdBit;
            }
            else {
                _positiveBinaries[i] += thirdBit;
            }
        }

        LogCurrentBinaryState("Final binaries:");
    }

    private void DoSerialNumberInversions() {
        string serial = _bomb.GetSerialNumber();
        string presentAxes = string.Empty;

        for (int i = 0; i < 4; i++) {
            if (serial.Contains(AXES[i].ToString())) {
                // Invert bits in position i.
                for (int j = 0; j < 3; j++) {
                    _positiveBinaries[j] = _positiveBinaries[j].Insert(i, (1 - (_positiveBinaries[j][i] - '0')).ToString()).Remove(i + 1, 1);
                    _negativeBinaries[j] = _negativeBinaries[j].Insert(i, (1 - (_negativeBinaries[j][i] - '0')).ToString()).Remove(i + 1, 1);
                }
                presentAxes += AXES[i];
            }
        }

        if (presentAxes.Length == 0) {
            _reversedLogging.Add("The serial number does not share any letters with \"XYZW\".");
        }
        else {
            _reversedLogging.Add($"The serial number contains the axis letters {presentAxes}.");
        }

        LogCurrentBinaryState("After shifting:");
    }

    private void DoShifts() {
        // These are both left shifts.
        int nShift = (4 - (_bomb.GetBatteryCount() - _bomb.GetBatteryHolderCount()) % 4) % 4;
        int pShift = _bomb.GetPortCount() % 4;

        for (int i = 0; i < 3; i++) {
            _negativeBinaries[i] = _negativeBinaries[i].Substring(nShift) + _negativeBinaries[i].Substring(0, nShift);
            _positiveBinaries[i] = _positiveBinaries[i].Substring(pShift) + _positiveBinaries[i].Substring(0, pShift);
        }

        LogCurrentBinaryState("The faces read:");
    }

    private void LogCurrentBinaryState(string message) {
        _reversedLogging.Add($"R-:{_negativeBinaries[0]} | G-:{_negativeBinaries[1]} | B-:{_negativeBinaries[2]}");
        _reversedLogging.Add($"R+:{_positiveBinaries[0]} | G+:{_positiveBinaries[1]} | B+:{_positiveBinaries[2]}");
        _reversedLogging.Add(message);
    }

    public Dictionary<string, Color> GetFaceColours() {
        var faceColours = new Dictionary<string, Color>();

        for (int i = 0; i < 4; i++) {
            int rValue = _positiveBinaries[0][i] - '0';
            int gValue = _positiveBinaries[1][i] - '0';
            int bValue = _positiveBinaries[2][i] - '0';
            Color newColour = GetColourFromRGB(rValue, gValue, bValue);
            faceColours.Add($"+{"XYZW"[i]}", newColour);

            rValue = _negativeBinaries[0][i] - '0';
            gValue = _negativeBinaries[1][i] - '0';
            bValue = _negativeBinaries[2][i] - '0';
            newColour = GetColourFromRGB(rValue, gValue, bValue);
            faceColours.Add($"-{"XYZW"[i]}", newColour);
        }

        return faceColours;
    }

    private Color GetColourFromRGB(int rValue, int gValue, int bValue) {
        if (rValue + gValue + bValue != 0) {
            return new Color(rValue, gValue, bValue, 0.25f);
        }
        else {
            return new Color(0, 0, 0, 0);
        }
    }
}
