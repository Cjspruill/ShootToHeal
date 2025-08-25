using UnityEngine;
using Unity.Cinemachine;
public class FaceCamera : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = FindFirstObjectByType<CinemachineCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cam.transform);
    }
}
