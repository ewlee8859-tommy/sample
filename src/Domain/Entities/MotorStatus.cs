using MotorMonitor.Domain.Enums;

namespace MotorMonitor.Domain.Entities;

/// <summary>
/// 모터 상태 엔티티
/// 모터의 현재 상태 정보를 나타냄
/// </summary>
public class MotorStatus
{
    /// <summary>
    /// 엔코더 커맨드 값 (pulse)
    /// </summary>
    public long EncoderCommand { get; set; }

    /// <summary>
    /// 엔코더 피드백 값 (pulse)
    /// </summary>
    public long EncoderFeedback { get; set; }

    /// <summary>
    /// 속도 (units/sec, 기어비 기반 변환값)
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// 위치 (units, 기어비 기반 변환값)
    /// </summary>
    public double Position { get; set; }

    /// <summary>
    /// 토크 (%, 정격 대비 비율)
    /// </summary>
    public double Torque { get; set; }

    /// <summary>
    /// 축 상태
    /// </summary>
    public AxisStatus Status { get; set; }

    /// <summary>
    /// 측정 시점
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 위치 편차 (Command - Feedback, pulse 단위)
    /// </summary>
    public long PositionError => EncoderCommand - EncoderFeedback;

    /// <summary>
    /// 위치 편차가 허용 오차 내에 있는지 확인
    /// </summary>
    /// <param name="tolerancePulse">허용 오차 (pulse)</param>
    /// <returns>오차 내이면 true</returns>
    public bool IsWithinTolerance(long tolerancePulse)
    {
        return Math.Abs(PositionError) <= tolerancePulse;
    }

    /// <summary>
    /// 상태 복사본 생성 (스냅샷용)
    /// </summary>
    public MotorStatus Clone()
    {
        return new MotorStatus
        {
            EncoderCommand = EncoderCommand,
            EncoderFeedback = EncoderFeedback,
            Speed = Speed,
            Position = Position,
            Torque = Torque,
            Status = Status,
            Timestamp = Timestamp
        };
    }
}
