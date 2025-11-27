using UnityEngine;
using UnityEngine.InputSystem;

public class AirplaneInput : MonoBehaviour
{
    [Header("Input Actions")]
    public AirplaneControls inputActions;

    [Header("Input Values")]
    public float throttleInput;
    public float pitchInput;
    public float rollInput;
    public float yawInput;
    public bool brakeInput;

    void Awake()
    {
        // Создаем экземпляр input actions
        inputActions = new AirplaneControls();
    }

    void OnEnable()
    {
        // Включаем все действия
        inputActions.Enable();
    }

    void OnDisable()
    {
        // Отключаем все действия
        inputActions.Disable();
    }

    void Update()
    {
        // Чтение значений из Input System
        throttleInput = inputActions.Flight.Throttle.ReadValue<float>();
        pitchInput = inputActions.Flight.Pitch.ReadValue<float>();
        rollInput = inputActions.Flight.Roll.ReadValue<float>();
        yawInput = inputActions.Flight.Yaw.ReadValue<float>();
        brakeInput = inputActions.Flight.Brake.ReadValue<float>() > 0.5f;
    }
}