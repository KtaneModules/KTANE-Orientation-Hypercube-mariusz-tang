using UnityEngine;

[RequireComponent(typeof(Animator), typeof(KMSelectable))]
public class Button : MonoBehaviour {

    private void Awake() {
        Animator _animator = GetComponent<Animator>();
        KMSelectable selectable = GetComponent<KMSelectable>();

        selectable.OnInteract += delegate () {
            _animator.SetBool("IsPressed", true);
            selectable.AddInteractionPunch();
            return false;
        };
        selectable.OnInteractEnded += delegate () {
            _animator.SetBool("IsPressed", false);
        };
    }
}
