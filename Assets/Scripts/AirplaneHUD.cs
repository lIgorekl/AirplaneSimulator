using UnityEngine;
using TMPro;

public class AirplaneHUD : MonoBehaviour
{
    public AirplanePhysics physics;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI verticalSpeedText;
    public TextMeshProUGUI throttleText;
    public TextMeshProUGUI stallWarningText;
    public TextMeshProUGUI aoaText;
    
    void Update()
    {
        if (physics == null) return;
        
        UpdateBasicTelemetry();
        UpdateWarnings();
    }
    
    void UpdateBasicTelemetry()
    {
        speedText.text = $"SPD: {physics.AirSpeed:F0} km/h";
        altitudeText.text = $"ALT: {physics.Altitude:F0} m";
        
        string vsIndicator = physics.VerticalSpeed > 0 ? "↑" : "↓";
        verticalSpeedText.text = $"V/S: {vsIndicator}{Mathf.Abs(physics.VerticalSpeed):F1} m/s";
        
        throttleText.text = $"THR: {physics.GetThrottle() * 100:F0}%";
        aoaText.text = $"AOA: {physics.angleOfAttack:F1}°";
    }
    
    void UpdateWarnings()
    {
        if (stallWarningText != null)
        {
            stallWarningText.gameObject.SetActive(physics.IsStalled);
        }
    }
}