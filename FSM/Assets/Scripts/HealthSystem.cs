using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable {

    public static HealthSystem Instance { get; private set; }

    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool isPlayer = false;

    private float currentHealth;

    public event EventHandler<HealthBarChangedEventArgs> OnHealthChanged;

    public event EventHandler<EventArgs> OnPlayerDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake() {
        if (isPlayer) {
            if (Instance != null && Instance != this) {
                Destroy(this.gameObject);
            } else {
                Instance = this;
            }
        }
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
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        } else {
            Debug.Log($"{gameObject.name} died!");
            Destroy(gameObject);
            KillManager.Instance?.AddKill();
        }
    }
}

