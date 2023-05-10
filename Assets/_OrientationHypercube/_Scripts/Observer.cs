using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour {

    private const float ANIMATION_TIME = 0.5f;

    private Coroutine _moveAnimation;
    private int _internalPosition = 0;

    public void MoveRight(bool reverse) {
        _internalPosition += reverse ? 90 : -90;

        if (_moveAnimation != null) {
            StopCoroutine(_moveAnimation);
        }
        _moveAnimation = StartCoroutine(RotateTo(_internalPosition));
    }

    private IEnumerator RotateTo(float targetRotation) {
        float elapsedTime = 0;
        float initialRotation = transform.localRotation.eulerAngles.y;

        while (elapsedTime <= ANIMATION_TIME) {
            elapsedTime += Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0, Mathf.LerpAngle(initialRotation, targetRotation, elapsedTime / ANIMATION_TIME), 90);
            yield return null;
        }
    }

    // ! Needs implementing.
    private IEnumerator TogglePosition(bool setToDefusePerspective) {
        yield return null;
    }
}
