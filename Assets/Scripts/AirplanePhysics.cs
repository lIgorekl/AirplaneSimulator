using UnityEngine;

public class AirplanePhysics : MonoBehaviour
{
    [Header("Основные параметры")]
    public float baseMass = 10000f;
    public float fuelAmount = 2000f;
    public float fuelDensity = 0.8f;
    public float maxThrust = 120000f;
    public float wingArea = 50f;
    
    [Header("Аэродинамические кривые")]
    public AnimationCurve liftCurve = AnimationCurve.Linear(0, 0, 15, 1);
    public float stallAngle = 15f;
    
    [Header("Управление")]
    public float rollSensitivity = 50000f;
    public float pitchSensitivity = 40000f;
    public float yawSensitivity = 20000f;
    
    [Header("Текущее состояние")]
    public float angleOfAttack; // Сделал публичной
    public float currentThrottle;
    
    // Приватные переменные
    private Rigidbody rb;
    private AirplaneInput input;
    
    // Публичные свойства для HUD
    public float AirSpeed { get; private set; }
    public float Altitude { get; private set; }
    public float VerticalSpeed { get; private set; }
    public bool IsStalled { get; private set; }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<AirplaneInput>();
        
        SetupPhysics();
    }
    
    void SetupPhysics()
    {
        rb.mass = CalculateTotalMass();
        rb.linearDamping = 0.05f;
        rb.angularDamping = 1f;
    }
    
    float CalculateTotalMass()
    {
        return baseMass + (fuelAmount * fuelDensity);
    }
    
    void FixedUpdate()
    {
        ApplyThrust();
        ApplyAerodynamicForces();
        ApplyControlSurfaces();
        ApplyStabilization();
        UpdateTelemetry();
    }
    
    void ApplyThrust()
    {
        currentThrottle = input.throttleInput;
        float thrust = currentThrottle * maxThrust;
        rb.AddForce(transform.forward * thrust);
    }
    
    void ApplyAerodynamicForces()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        
        // Расчет угла атаки (по теории)
        angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
        
        // Проверка сваливания
        IsStalled = Mathf.Abs(angleOfAttack) > stallAngle;
        
        // Подъемная сила (формула из теории)
        float liftCoefficient = liftCurve.Evaluate(Mathf.Abs(angleOfAttack));
        if (IsStalled) liftCoefficient *= 0.3f;
        
        float airDensity = 1.225f * Mathf.Exp(-Altitude / 8000f);
        float dynamicPressure = 0.5f * airDensity * Mathf.Pow(localVelocity.z, 2);
        float liftForce = liftCoefficient * dynamicPressure * wingArea;
        
        rb.AddForce(transform.up * liftForce);
        
        // Лобовое сопротивление
        float dragCoefficient = 0.02f + (Mathf.Pow(angleOfAttack / 30f, 2) * 0.3f);
        float dragForce = dragCoefficient * dynamicPressure * wingArea;
        rb.AddForce(-rb.linearVelocity.normalized * dragForce);
    }
    
    void ApplyControlSurfaces()
    {
        // Управление с учетом скорости (чем медленнее, тем менее эффективно)
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / 40f);
        
        // Плавное управление с интерполяцией
        float smoothRoll = Mathf.Lerp(0, input.rollInput, 2f * Time.fixedDeltaTime);
        float smoothPitch = Mathf.Lerp(0, input.pitchInput, 2f * Time.fixedDeltaTime);
        float smoothYaw = Mathf.Lerp(0, input.yawInput, 2f * Time.fixedDeltaTime);
        
        // Крен (A/D)
        float rollTorque = -smoothRoll * rollSensitivity * speedFactor;
        
        // Тангаж (стрелки вверх/вниз)
        float pitchTorque = smoothPitch * pitchSensitivity * speedFactor;
        
        // Рыскание (стрелки влево/вправо)
        float yawTorque = smoothYaw * yawSensitivity * speedFactor;
        
        rb.AddRelativeTorque(pitchTorque, yawTorque, rollTorque);
    }
    
    void ApplyStabilization()
    {
        // Нормализуем углы Эйлера в диапазон [-180, 180]
        Vector3 euler = transform.eulerAngles;
        float normalizedRoll = euler.z > 180 ? euler.z - 360 : euler.z;
        float normalizedPitch = euler.x > 180 ? euler.x - 360 : euler.x;
        
        // ПЛАВНАЯ СТАБИЛИЗАЦИЯ ПО КРЕНУ
        if (Mathf.Abs(input.rollInput) < 0.1f && Mathf.Abs(normalizedRoll) > 5f)
        {
            // Мягкая стабилизация к нулевому крену
            float rollStabilization = -normalizedRoll * 800f; // Уменьшил силу стабилизации
            rollStabilization = Mathf.Clamp(rollStabilization, -4000f, 4000f);
            rb.AddRelativeTorque(0, 0, rollStabilization);
        }
        
        // ПЛАВНАЯ СТАБИЛИЗАЦИЯ ПО ТАНГАЖУ
        if (Mathf.Abs(input.pitchInput) < 0.1f && Mathf.Abs(normalizedPitch) > 3f)
        {
            // Стабилизируем на небольшом положительном угле (2 градуса) для поддержания высоты
            float targetPitch = 2f;
            float pitchError = targetPitch - normalizedPitch;
            float pitchStabilization = pitchError * 600f; // Уменьшил силу стабилизации
            pitchStabilization = Mathf.Clamp(pitchStabilization, -3000f, 3000f);
            rb.AddRelativeTorque(pitchStabilization, 0, 0);
        }
        
        // УМЕРЕННОЕ ДЕМПФИРОВАНИЕ УГЛОВОЙ СКОРОСТИ
        Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
        
        // Демпфирование вращения вокруг всех осей (уменьшил силу)
        float angularDamping = 800f;
        Vector3 angularDampingTorque = -localAngularVelocity * angularDamping;
        rb.AddRelativeTorque(angularDampingTorque);
    }
    
    void UpdateTelemetry()
    {
        AirSpeed = rb.linearVelocity.magnitude * 3.6f;
        Altitude = transform.position.y;
        VerticalSpeed = rb.linearVelocity.y;
    }
    
    public float GetThrottle() => currentThrottle;
}