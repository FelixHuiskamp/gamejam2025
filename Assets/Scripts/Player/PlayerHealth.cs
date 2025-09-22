using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;


    [Header("UI")]
    public Slider healthBar; 

    [Header("Death Screen")]
    public GameObject deathScreen; 

    public bool isDead = false;
    public bool invincible = false;
    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (deathScreen != null)
            deathScreen.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (invincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        if (deathScreen != null)
            deathScreen.SetActive(true);

        
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<CharacterController>().enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


}

