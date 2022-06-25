using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShipStats : MonoBehaviour
{
    public MeshRenderer shieldRenderer;
    public GameObject shipStats;
    public Slider healthBar;
    public Slider shieldBar;
    public float health = 1000;
    public float shields = 1000;
    public float rechargeDelay = 5;
    public float rechargeRate = 20;
    public float damageResistance = 1000;
    public float timer = 0;
    public GameObject fracture;
    public GPUInstancer.GPUInstancerPrefabManager prefabManager;
    public GPUInstancer.GPUInstancerModificationCollider deinstancingSphere;
    public GPUInstancer.GPUInstancerModificationCollider deinstancingArea;
    private GPUInstancer.GPUInstancerPrefab allocatedGO;

    // Start is called before the first frame update
    void Start()
    {
        //shipStats.SetActive(false);
        healthBar.maxValue = health;
        healthBar.value = health;
        shieldBar.maxValue = shields;
        shieldBar.value = shields;
        prefabManager = GameObject.Find("GPUI Prefab Manager").GetComponent<GPUInstancer.GPUInstancerPrefabManager>();
        deinstancingSphere = GameObject.Find("DeInstancingSphere").GetComponent<GPUInstancer.GPUInstancerModificationCollider>();
        deinstancingArea = GameObject.Find("DeInstancingArea").GetComponent<GPUInstancer.GPUInstancerModificationCollider>();
        allocatedGO = GetComponent<GPUInstancer.GPUInstancerPrefab>();
        fracture.transform.localScale = transform.localScale;
        shipStats.SetActive(false);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (shieldBar.value < shields)
        {
            timer += Time.deltaTime;
            if (timer > rechargeDelay)
            {
                shieldBar.value += rechargeRate * Time.deltaTime;
                shieldRenderer.material.SetFloat("Vector1_AE9DFBD", -1.0f + (shieldBar.value / shields) * 1.2f);
                if (shieldBar.value == shields)
                {
                    shipStats.SetActive(false);
                    enabled = false;
                }
            }
        }

        shieldRenderer.material.SetColor("Color_63659391", new Color(1.0f - (shieldBar.value * 2 / 255), shieldBar.value / 255, shieldBar.value * 2 / 255));
    }

    private void OnCollisionEnter(Collision collision)
    {
        timer = 0;
        shipStats.SetActive(true);
        enabled = true;

        if (shieldBar.value > 0)
        {
            shieldBar.value -= collision.impulse.magnitude / damageResistance;

            if (shieldBar.value == 0)
                shieldRenderer.material.SetFloat("Vector1_AE9DFBD", -1.0f);
        }
        else
        {
            healthBar.value -= collision.impulse.magnitude / damageResistance;
        }

        if (healthBar.value == 0)
        {
            deinstancingArea._enteredInstances.Remove(allocatedGO);
            deinstancingSphere._enteredInstances.Remove(allocatedGO);
            GPUInstancer.GPUInstancerAPI.RemovePrefabInstance(prefabManager, allocatedGO);
            Instantiate(fracture, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}