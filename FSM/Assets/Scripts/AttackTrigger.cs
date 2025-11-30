using UnityEngine;

public class AttackTrigger : MonoBehaviour {

    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private string targetTag;

    private float damage;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag(targetTag)) return;

        HealthSystem target = other.GetComponent<HealthSystem>();
        if (target == null) return;

        float defaultdamage = 10f;
        damage = weaponData != null ? weaponData.damage : defaultdamage;

        target.TakeDamage(damage);

        //Debug.Log($"{gameObject.name} has dealt {damage} damage to {other.name}");
    }


}
