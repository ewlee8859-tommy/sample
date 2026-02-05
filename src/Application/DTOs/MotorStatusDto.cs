using MotorMonitor.Domain.Entities;
using MotorMonitor.Domain.Enums;

namespace MotorMonitor.Application.DTOs;

/// <summary>
/// 모터 상태 DTO
/// Presentation 레이어로 전달되는 표시용 데이터
/// </summary>
public class MotorStatusDto
{
    /// <summary>
    /// 엔코더 커맨드 값 (pulse)
    /// </summary>
    public long EncoderCommand { get; init; }

    /// <summary>
    /// 엔코더 피드백 값 (pulse)
    /// </summary>
    public long EncoderFeedback { get; init; }

    /// <summary>
    /// 속도 (units/sec)
    /// </summary>
    public double Speed { get; init; }

    /// <summary>
    /// 위치 (units)
    /// </summary>
    public double Position { get; init; }

    /// <summary>
    /// 토크 (%)
    /// </summary>
    public double Torque { get; init; }

    /// <summary>
    /// 축 상태
    /// </summary>
    public AxisStatus Status { get; init; }

    /// <summary>
    /// 위치 편차 (pulse)
    /// </summary>
    public long PositionError { get; init; }

    /// <summary>
    /// 측정 시점
    /// </summary>
    public DateTime Timestamp { get; init; }

    // UI 표시용 포맷 문자열

    /// <summary>
    /// 엔코더 커맨드 표시 문자열 (예: "1,234,567 pulse")
    /// </summary>
    public string EncoderCommandDisplay => $"{EncoderCommand:N0} pulse";

    /// <summary>
    /// 엔코더 피드백 표시 문자열 (예: "1,234,550 pulse")
    /// </summary>
    public string EncoderFeedbackDisplay => $"{EncoderFeedback:N0} pulse";

    /// <summary>
    /// 속도 표시 문자열 (예: "150.25 mm/s")
    /// </summary>
    public string SpeedDisplay => $"{Speed:F2} mm/s";

    /// <summary>
    /// 위치 표시 문자열 (예: "450.75 mm")
    /// </summary>
    public string PositionDisplay => $"{Position:F2} mm";

    /// <summary>
    /// 토크 표시 문자열 (예: "35.2 %")
    /// </summary>
    public string TorqueDisplay => $"{Torque:F1} %";

    /// <summary>
    /// 위치 편차 표시 문자열 (예: "+17 pulse")
    /// </summary>
    public string PositionErrorDisplay => $"{PositionError:+#,##0;-#,##0;0} pulse";

    /// <summary>
    /// 상태 표시 이름 (한글)
    /// </summary>
    public string StatusDisplayName => Status.ToDisplayName();

    /// <summary>
    /// 상태 색상 (Hex)
    /// </summary>
    public string StatusColor => Status.ToColor();

    /// <summary>
    /// MotorStatus 엔티티로부터 DTO 생성
    /// </summary>
    public static MotorStatusDto FromEntity(MotorStatus entity)
    {
        return new MotorStatusDto
        {
            EncoderCommand = entity.EncoderCommand,
            EncoderFeedback = entity.EncoderFeedback,
            Speed = entity.Speed,
            Position = entity.Position,
            Torque = entity.Torque,
            Status = entity.Status,
            PositionError = entity.PositionError,
            Timestamp = entity.Timestamp
        };
    }
}
