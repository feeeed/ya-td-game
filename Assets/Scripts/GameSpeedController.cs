using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    public float maxGameSpeed = 4;

    float gameSpeed;


    void Start()
    {
        SetGameSpeed(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            SetGameSpeed(gameSpeed + 1);
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SetGameSpeed(gameSpeed - 1);
        }

        if (Time.timeScale > 0 && Time.timeScale != gameSpeed)
        {
            SetGameSpeed(gameSpeed);
        }
    }

    public void SetGameSpeed(float v)
    {
        gameSpeed = Mathf.Clamp(v, 1, maxGameSpeed);
        Time.timeScale = gameSpeed;
    }
}
