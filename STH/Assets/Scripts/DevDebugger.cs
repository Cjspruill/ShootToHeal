using UnityEngine;
using UnityEngine.UI;

public class DevDebugger : MonoBehaviour
{

    [SerializeField] PlayerController playerController;

    public Button addCash;
    public Button addXP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AddCash()
    {
        playerController.GetCash += 1000;
    }

    public void AddXP()
    {
        playerController.GetXp += 10;
    }
}
