using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ScriptableObject {

    [Header("Main stats")]
    public string weaponName;
    public GameObject weaponModelPrefab;
    public float damage = 10f;

}
