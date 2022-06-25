using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipStats : MonoBehaviour
{
    public MeshRenderer shieldRenderer;
    public Slider healthBar;
    public Slider shieldBar;
    public float health = 100;
    public float shields = 100;
    public float rechargeDelay = 5;
    public float rechargeRate = 20;
    public float damageResistance = 1000;
    public float timer = 0;
    [Range(0.1f, 0.3f)]
    public float lowShieldNotificationThreshold = 0.3f;
    [Range(0.1f, 0.3f)]
    public float hullCriticalNotificationThreshold = 0.3f;
    public GameObject fracture;
    public GameObject lowShieldNotification;
    public GameObject hullCriticalNotification;
    public GameObject deathNotification;
    public GameObject pauseNotification;
    public GameObject pauseMenu;
    public Text menuText;
    private float lowShieldNotificationThresholdAbsolute;
    private float hullCriticalNotificationThresholdAbsolute;

    // Start is called before the first frame update
    void Start()
    {
        lowShieldNotificationThresholdAbsolute = shields * lowShieldNotificationThreshold;
        hullCriticalNotificationThresholdAbsolute = health * hullCriticalNotificationThreshold;
        healthBar.maxValue = health;
        healthBar.value = health;
        shieldBar.maxValue = shields;
        shieldBar.value = shields;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                pauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                Time.timeScale = 0;
            }
            else
            {
                pauseMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
            }
        }

        if (shieldBar.value < shields)
        {
            timer += Time.deltaTime;
            if (timer > rechargeDelay)
            {
                shieldBar.value += rechargeRate * Time.deltaTime;
                shieldRenderer.material.SetFloat("Vector1_AE9DFBD", -1.0f + (shieldBar.value / shields) * 1.2f);
                if (shieldBar.value > lowShieldNotificationThresholdAbsolute)
                    lowShieldNotification.SetActive(false); // Reset the low shield notification
            }
        }

        shieldRenderer.material.SetColor("Color_63659391", new Color(1.0f - (shieldBar.value * 2 / 255), shieldBar.value / 255, shieldBar.value * 2 / 255));
    }

    private void OnCollisionEnter(Collision collision)
    {
        timer = 0;

        if (shieldBar.value > 0)
        {
            shieldBar.value -= collision.impulse.magnitude / damageResistance;

            if (shieldBar.value < lowShieldNotificationThresholdAbsolute)
                lowShieldNotification.SetActive(true); // Display low shield notification if shields go under the notification threshold

            if (shieldBar.value == 0)
                shieldRenderer.material.SetFloat("Vector1_AE9DFBD", -1.0f);
        }
        else
        {
            healthBar.value -= collision.impulse.magnitude / damageResistance;

            if (healthBar.value < hullCriticalNotificationThresholdAbsolute)
                hullCriticalNotification.SetActive(true); // Display hull critical notification if health goes under the notification threshold
        }

        if (healthBar.value == 0)
        {
            Instantiate(fracture, transform.position, transform.rotation);
            pauseMenu.SetActive(true);
            pauseNotification.SetActive(false);
            deathNotification.SetActive(true);
            menuText.text = "Pick An Option";
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Destroy(gameObject);
        }
    }
}
