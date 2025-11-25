using UnityEngine;

public class LookAtCamera : MonoBehaviour {

    [SerializeField] private Transform mainCamera;

    private void LateUpdate() {
        transform.LookAt(transform.position + mainCamera.forward); 
    }

}
