using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable {

    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool isPlayer = false;

    private float currentHealth;

    public event EventHandler<HealthBarChangedEventArgs> OnHealthChanged;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake() {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage) {
        if (damage < 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        //Debug.Log($"<b>[{gameObject.name}]<b> - {damage}HP -> {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(this, new HealthBarChangedEventArgs(currentHealth, maxHealth));

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        if (isPlayer) {
            Debug.Log("Player died! (GAME OVER)");
        } else {
            Debug.Log($"{gameObject.name} died!");
            Destroy(gameObject);
        }
    }
}

