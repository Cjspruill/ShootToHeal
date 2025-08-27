using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{

    [SerializeField] Transform transformToFollow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(transformToFollow.position.x, transform.position.y, transformToFollow.position.z);
        transform.position = newPos;
    }
}
