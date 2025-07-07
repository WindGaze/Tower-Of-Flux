using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class BossHP : MonoBehaviour
{
    public GameObject targetObject;    // Assign boss object here
    public Image healthBarFill;
    public float fillSpeed = 5f;
    public float maxHealthValue = 5000f; // set this in Inspector to match your starting health

    private float currentFill = 1f;
    private FieldInfo healthField;
    private PropertyInfo healthProp;
    private Component cachedComponent;

    void Awake()
    {
        UpdateReflection();
    }

    void UpdateReflection()
    {
        if (targetObject != null)
        {
            foreach (var comp in targetObject.GetComponents<Component>())
            {
                var compType = comp.GetType();

                var hf = compType.GetField("health", BindingFlags.Instance | BindingFlags.Public);
                var hp = compType.GetProperty("health", BindingFlags.Instance | BindingFlags.Public);

                if (hf != null || hp != null)
                {
                    cachedComponent = comp;
                    healthField = hf;
                    healthProp = hp;
                    return;
                }
            }

            cachedComponent = null;
            Debug.LogWarning("No 'health' field or property found on targetObject!");
        }
    }

    void Update()
    {
        if (targetObject == null || healthBarFill == null || cachedComponent == null)
        {
            UpdateReflection();
            return;
        }

        float health = maxHealthValue;
        if (healthField != null)
            health = (float)System.Convert.ChangeType(healthField.GetValue(cachedComponent), typeof(float));
        else if (healthProp != null)
            health = (float)System.Convert.ChangeType(healthProp.GetValue(cachedComponent, null), typeof(float));

        float targetFill = maxHealthValue > 0 ? health / maxHealthValue : 0;
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);
        healthBarFill.fillAmount = Mathf.Clamp01(currentFill);
    }
}