using UnityEngine;

public class DisableEnableCollider : MonoBehaviour {

    [SerializeField] private GameObject currentWeaponInHand;

    public void EnableEnemySwordCollider() {
        currentWeaponInHand.GetComponent<BoxCollider>().enabled = true;
    }

    public void DisableEnemySwordCollider() {
        currentWeaponInHand.GetComponent<BoxCollider>().enabled = false;
    }
}
