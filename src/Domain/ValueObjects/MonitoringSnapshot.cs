using MotorMonitor.Domain.Entities;

namespace MotorMonitor.Domain.ValueObjects;

/// <summary>
/// 모니터링 스냅샷 Value Object
/// 특정 시점의 모터 상태를 불변 객체로 저장
/// </summary>
public sealed class MonitoringSnapshot : IEquatable<MonitoringSnapshot>
{
    /// <summary>
    /// 스냅샷 생성 시점
    /// </summary>
    public DateTime SnapshotAt { get; }

    /// <summary>
    /// 엔코더 커맨드 값 (pulse)
    /// </summary>
    public long EncoderCommand { get; }

    /// <summary>
    /// 엔코더 피드백 값 (pulse)
    /// </summary>
    public long EncoderFeedback { get; }

    /// <summary>
    /// 속도 (units/sec)
    /// </summary>
    public double Speed { get; }

    /// <summary>
    /// 위치 (units)
    /// </summary>
    public double Position { get; }

    /// <summary>
    /// 토크 (%)
    /// </summary>
    public double Torque { get; }

    /// <summary>
    /// 위치 편차 (pulse)
    /// </summary>
    public long PositionError { get; }

    /// <summary>
    /// MotorStatus로부터 스냅샷 생성
    /// </summary>
    public MonitoringSnapshot(MotorStatus motorStatus)
    {
        ArgumentNullException.ThrowIfNull(motorStatus);

        SnapshotAt = DateTime.Now;
        EncoderCommand = motorStatus.EncoderCommand;
        EncoderFeedback = motorStatus.EncoderFeedback;
        Speed = motorStatus.Speed;
        Position = motorStatus.Position;
        Torque = motorStatus.Torque;
        PositionError = motorStatus.PositionError;
    }

    /// <summary>
    /// 직접 값 지정으로 스냅샷 생성
    /// </summary>
    public MonitoringSnapshot(
        DateTime snapshotAt,
        long encoderCommand,
        long encoderFeedback,
        double speed,
        double position,
        double torque)
    {
        SnapshotAt = snapshotAt;
        EncoderCommand = encoderCommand;
        EncoderFeedback = encoderFeedback;
        Speed = speed;
        Position = position;
        Torque = torque;
        PositionError = encoderCommand - encoderFeedback;
    }

    public bool Equals(MonitoringSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return SnapshotAt == other.SnapshotAt &&
               EncoderCommand == other.EncoderCommand &&
               EncoderFeedback == other.EncoderFeedback &&
               Math.Abs(Speed - other.Speed) < 0.0001 &&
               Math.Abs(Position - other.Position) < 0.0001 &&
               Math.Abs(Torque - other.Torque) < 0.0001;
    }

    public override bool Equals(object? obj) => Equals(obj as MonitoringSnapshot);

    public override int GetHashCode()
    {
        return HashCode.Combine(SnapshotAt, EncoderCommand, EncoderFeedback, Speed, Position, Torque);
    }

    public static bool operator ==(MonitoringSnapshot? left, MonitoringSnapshot? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(MonitoringSnapshot? left, MonitoringSnapshot? right)
    {
        return !(left == right);
    }
}
