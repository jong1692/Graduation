using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;

    public static TimeManager Instance
    {
        get { return instance; }
    }

    public double DeltaTime
    {
        get { return deltaTime; }
    }

    private double startTime;
    private double deltaTime;

    void Awake()
    {
        if (instance == null)
            instance = this;

        startTime = Time.realtimeSinceStartup;
    }

    public void pauseTime(bool pause)
    {
        if (pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    void Update()
    {
        deltaTime = Time.realtimeSinceStartup - startTime;
        startTime = Time.realtimeSinceStartup;
    }
}
