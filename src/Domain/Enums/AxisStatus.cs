namespace MotorMonitor.Domain.Enums;

/// <summary>
/// 모터 축 상태
/// </summary>
public enum AxisStatus
{
    /// <summary>
    /// 비활성 (서보 OFF)
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// 대기 (서보 ON, 정지 상태)
    /// </summary>
    Ready = 1,

    /// <summary>
    /// 운전중 (모션 실행 중)
    /// </summary>
    Running = 2,

    /// <summary>
    /// 이상 (알람 발생)
    /// </summary>
    Error = 3
}

/// <summary>
/// AxisStatus 확장 메서드
/// </summary>
public static class AxisStatusExtensions
{
    /// <summary>
    /// 상태 표시 이름 (한글)
    /// </summary>
    public static string ToDisplayName(this AxisStatus status) => status switch
    {
        AxisStatus.Disabled => "비활성",
        AxisStatus.Ready => "대기",
        AxisStatus.Running => "운전중",
        AxisStatus.Error => "이상",
        _ => "알 수 없음"
    };

    /// <summary>
    /// 상태 표시 색상 (Hex)
    /// </summary>
    public static string ToColor(this AxisStatus status) => status switch
    {
        AxisStatus.Disabled => "#808080",  // Gray
        AxisStatus.Ready => "#00AA00",     // Green
        AxisStatus.Running => "#0066CC",   // Blue
        AxisStatus.Error => "#CC0000",     // Red
        _ => "#000000"
    };
}
