using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] Light mainLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainLight = GetComponent<Light>();

        Vector3 lightRotation = new Vector3(Random.Range(0, 180), Random.Range(0, 360), Random.Range(0, 360));
        mainLight.transform.rotation = Quaternion.Euler(lightRotation);
        float newTemperature = Random.Range(1500f, 20000f);
        mainLight.colorTemperature = newTemperature;
        float newIntensity = Random.Range(1f, 5f);
        mainLight.intensity = newIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
