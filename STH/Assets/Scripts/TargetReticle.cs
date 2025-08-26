using UnityEngine;

public class TargetReticle : MonoBehaviour
{
    [SerializeField] public Vector3 gruntReticleSize;
    [SerializeField] public Vector3 runnerReticleSize;
    [SerializeField] public Vector3 tankReticleSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.Instance.levelEnded)
        transform.Rotate(0, 0, 1);
    }

    public void UpdateReticleSize(Vector3 newSize)
    {
        transform.localScale = newSize;
    }
}
