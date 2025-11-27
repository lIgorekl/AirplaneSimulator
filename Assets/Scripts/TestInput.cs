using UnityEngine;

public class TestInput : MonoBehaviour
{
    public AirplaneInput airplaneInput;

    void Start()
    {
        // Автоматически находим компонент на этом же объекте
        airplaneInput = GetComponent<AirplaneInput>();
    }

    void Update()
    {
        if (airplaneInput != null)
        {
            Debug.Log($"Throttle: {airplaneInput.throttleInput}, " +
                     $"Pitch: {airplaneInput.pitchInput}, " +
                     $"Roll: {airplaneInput.rollInput}, " +
                     $"Yaw: {airplaneInput.yawInput}, " +
                     $"Brake: {airplaneInput.brakeInput}");
        }
    }
}