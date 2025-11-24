using UnityEngine;

public class EquipmentSystem : MonoBehaviour {

    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject weaponSheath;
    [SerializeField] GameObject weapon;

    private GameObject currentWeaponInHand;
    private GameObject currentWeaponInSheath;

    private void Start() {
        currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
    }

    public void DrawWeapon() {
        currentWeaponInHand = Instantiate(weapon, weaponHolder.transform);
        Destroy(currentWeaponInSheath);
    }

    public void SheathWeapon() {
        currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
        Destroy(currentWeaponInHand);
    }

    public GameObject GetCurrentWeaponInHand() {
        return currentWeaponInHand;
    }
}
