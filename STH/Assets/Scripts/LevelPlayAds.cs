using Unity.Services.LevelPlay;
using UnityEngine;

public class LevelPlayAds : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelPlay.Init("2369c7645");
    }

    private void OnEnable()
    {
    }

    private void PauseApplication(bool isPaused)
    {
        if (isPaused)
        {

        }
        else
        {

        }
    }


    //Banner Callbacks

    //Fullsize Callbacks

    //Rewarded Ad Callbacks
}
