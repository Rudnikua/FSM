using System;
using UnityEngine;

public class HealthBarChangedEventArgs : EventArgs {

    public float CurrentHealth { get; }
    public float MaxHealth { get; }

    public HealthBarChangedEventArgs(float currentHealth, float maxHealth) {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }

}