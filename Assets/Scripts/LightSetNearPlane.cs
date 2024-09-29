using UnityEngine;

[ExecuteInEditMode]
public class LightSetNearPlane : MonoBehaviour
{
    public float nearPlane;

    void Start()
    {
        OnValidate();
    }
    void OnValidate()
    {
        GetComponent<Light>().shadowNearPlane = nearPlane; 
    }
}
