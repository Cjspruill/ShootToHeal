using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationX; 
    public float rotationY; 
    public float rotationZ; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationX * Time.deltaTime, rotationY * Time.deltaTime, rotationZ * Time.deltaTime); 
    }
}
