using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour {
    public enum DisplayMode {
        AlwaysVisible,
        ShowOnDamage
    }

    [Header("References")]
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private Slider damageBarSlider;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Canvas healthCanvas;

    [Header("Visual")]
    [SerializeField] private float slideSpeed = 100f;

    [Header("Behavior")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.AlwaysVisible;

    private float visualHealth;
    private float targetHealth;
    private void Awake() {
        if (displayMode == DisplayMode.ShowOnDamage && healthCanvas != null) {
            healthCanvas.enabled = false;
        }
    }

    private void Start() {

        if (healthSystem == null) {
            Debug.LogError($"[{name}] HealthBarUI: healthSystem not assigned!", this);
            enabled = false;
            return;
        }

        float maxHealth = healthSystem.MaxHealth;
        float currentHealth = healthSystem.CurrentHealth;

        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = currentHealth;

        if (damageBarSlider != null) {
            damageBarSlider.maxValue = maxHealth;
            damageBarSlider.value = currentHealth;
        }

        visualHealth = currentHealth;
        targetHealth = currentHealth;

        healthSystem.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(object sender, HealthBarChangedEventArgs e) {
        targetHealth = e.CurrentHealth;
        healthBarSlider.maxValue = e.MaxHealth;
        if (damageBarSlider != null)
            damageBarSlider.maxValue = e.MaxHealth;

        if (displayMode == DisplayMode.ShowOnDamage && healthCanvas != null && !healthCanvas.enabled) {
            healthCanvas.enabled = true;
        }
    }

    private void Update() {
        visualHealth = Mathf.MoveTowards(visualHealth, targetHealth, slideSpeed * Time.deltaTime);
        healthBarSlider.value = visualHealth;

        if (damageBarSlider != null) {
            float damageBarTarget = targetHealth;
            damageBarSlider.value = Mathf.MoveTowards(damageBarSlider.value, damageBarTarget, slideSpeed * 0.3f * Time.deltaTime);
        }
    }

    private void OnDestroy() {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= OnHealthChanged;
    }
}