using MotorMonitor.Application.DTOs;

namespace MotorMonitor.Application.Interfaces;

/// <summary>
/// 모니터링 서비스 인터페이스
/// 모터 상태를 주기적으로 모니터링하고 업데이트 이벤트를 발생시킴
/// </summary>
public interface IMonitoringService : IDisposable
{
    /// <summary>
    /// 현재 모터 상태 조회
    /// </summary>
    /// <returns>현재 모터 상태 DTO</returns>
    MotorStatusDto GetCurrentStatus();

    /// <summary>
    /// 주기적 모니터링 시작
    /// </summary>
    /// <param name="intervalMs">업데이트 주기 (밀리초, 기본 100ms)</param>
    void StartMonitoring(int intervalMs = 100);

    /// <summary>
    /// 모니터링 중지
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// 모니터링 실행 중 여부
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 상태 업데이트 이벤트
    /// 모니터링 주기마다 새로운 상태 DTO와 함께 발생
    /// </summary>
    event Action<MotorStatusDto>? StatusUpdated;
}
