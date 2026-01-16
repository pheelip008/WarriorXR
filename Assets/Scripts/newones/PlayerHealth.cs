using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText; // Optional: shows "75/100"

    [Header("Invulnerability")]
    public bool isInvulnerable = false;

    [Header("Audio (Optional)")]
    public AudioClip damageSound;
    private AudioSource audioSource;

    // Events for other systems
    public System.Action OnPlayerDeath;
    public System.Action<int> OnDamageTaken; // passes damage amount

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int damage)
    {
        // Check invulnerability
        if (isInvulnerable)
        {
            Debug.Log("Player is invulnerable! No damage taken.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Notify score manager
        OnDamageTaken?.Invoke(damage);

        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        UpdateHealthUI();

        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}";
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();

    }

    // Call this from PoseManager when holding a pose
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
}