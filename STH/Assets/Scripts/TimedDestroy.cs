using UnityEngine;

public class TimedDestroy : MonoBehaviour
{

    [SerializeField] float destroyTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, destroyTime);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
