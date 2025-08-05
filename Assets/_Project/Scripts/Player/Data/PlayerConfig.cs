using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Settings", fileName = "New Player Settings")]
public class PlayerConfig : ScriptableObject
{
    public int StartingGold = 100;
    
    [Header("Movement Settings")]
    [Tooltip("Максимальная скорость")] public float MaxSpeed = 20f;
    [Tooltip("Коэффициент ускорения")] public float Acceleration = 15f;
    [Tooltip("Естественное замедление, когда нет газа")] public float Drag = 2f;
    [Tooltip("Сглаживание yaw при повороте")] public float TurnSmoothTime = 0.15f;
    [Tooltip("Максимальный наклон (в градусах)")] public float LeanAngleMax = 18f;
    [Tooltip("Сглаживание самого lean")] public float LeanSmoothTime = 0.1f;
    [Tooltip("0 = нет дрифта, 1 = сильный дрифт при повороте")] public float DriftFactor = 0.35f;
    [Tooltip("Базовое затухание боковой скорости")] public float LateralFriction = 5f;
    [Tooltip("Коэффициент ускорения")] public float MinInputThreshold = 0.01f;
    
    [Header("Jump Settings")]
    public AudioClip JumpClip;
    public float JumpForce = 30f;
    public float JumpDistance = 1.2f;

    [Header("Push Settings")]
    public AudioClip PushClip;
    public float PushRadius = 2f;
    public float PushForce = 10f;
    public float PushCooldown = 1f;
    public float PushDuration = 1f;
    
    [Header("Available skins")]
    public List<SkinDefinition> AvailableSkins;
}


    // For reality physics movement
    // [Header("Bicycle Steering Settings")]
    // [Tooltip("Сглаживание наклона при повороте")] public float LeanSmoothTime = 0.1f;
    // [Tooltip("Максимальный угол наклона в градусах")] public float MaxLeanAngle = 25f;
    // [Tooltip("Дополнительный множитель на целевой наклон, чтобы сделать его заметнее")] public float LeanSensitivity = 1.2f;
    // [Tooltip("Минимальный угол наклона ниже которого не применяется (чтобы не дрожал)")] public float MinLeanAngleThreshold = 0.5f;
    // [Tooltip("Скорость кглаживания желаемого направления")] public float DesiredDirectionSmoothSpeed = 10f;
    //
    // [Space(4)]
    // [Tooltip("База колёс — влияет на радиус поворота (меньше = круче поворот)")] public float WheelBase = 1f;
    // [Tooltip("Максимальный угол рулевого (в градусах)")] public float MaxSteeringAngle = 35f;
    // [Tooltip("Жёсткость пружины рулевого управления (trail/self-centering)")] public float SteeringSpringStiffness = 16f;
    // [Tooltip("Демпфирование рулевого управления (должно быть ≥ 2*sqrt(stiffness) для устойчивости)")] public float SteeringDamping = 10f;
    // [Tooltip("Угол (в градусах), ниже которого отклик руления игнорируется, чтобы убрать микрокоррекции")] public float SteeringDeadzoneDegrees = 1.5f;
    // [Tooltip("Скорость, при которой руление достигает полной эффективности")] public float SteeringMinSpeedForFullResponse = 1.2f;
    // [Tooltip("Экспонента для отклика руления: меньше 1 делает руление резче на средних скоростях")] public float SteeringResponseExponent = 0.7f;
    // [Tooltip("Время сглаживания изменения heading (меньше = быстрее отклик, но может быть шумнее)")] public float HeadingSmoothTime = 0.02f;
    // [Tooltip("Импульс контрруления при резкой смене направления")] public float CountersteerImpulse = 25f;
    // [Tooltip("Подавление бокового скольжения")] public float LateralFriction = 5f;
    //
    // [Header("Quick Turn")]
    // [Tooltip("Угол (в градусах) для активации быстрого разворота")] public float QuickTurnAngleThreshold = 120f;
    // [Tooltip("Дополнительный yaw-рывок при quick turn (градусы в секунду)")] public float QuickTurnYawBoost = 500f;
    // [Tooltip("Длительность quick turn")] public float QuickTurnDuration = 0.15f;
    // [Tooltip("Множитель жёсткости рулевого во время quick turn")] public float QuickTurnSteeringStiffnessMultiplier = 2.5f;
    // [Tooltip("Множитель демпфирования рулевого во время quick turn")] public float QuickTurnDampingMultiplier = 1.5f;
    //
    // [Header("Air Control")]
    // [Tooltip("Во сколько раз ослаблено руление в воздухе (1 = как на земле)")] public float AirSteeringFactor = 0.5f;
    // [Tooltip("Во сколько раз ослаблено продольное управление в воздухе (1 = как на земле)")] public float AirThrottleFactor = 0.4f;
    // [Tooltip("Множитель наклона в воздухе (обычно меньше, чтобы визуально было мягче)")] public float InAirLeanMultiplier = 0.7f;
    //
    // [Header("Slope Settings")]
    // [Tooltip("Насколько сильно помогает подъёму компенсация гравитации (1 = полное гашение компоненты гравитации)")] public float ClimbAssist = 1f;
    // [Tooltip("Максимальный угол подъёма в градусах, выше которого подниматься трудно/невозможно")] public float MaxClimbableSlopeAngle = 50f;