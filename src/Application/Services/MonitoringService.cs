using MotorMonitor.Application.DTOs;
using MotorMonitor.Application.Interfaces;
using MotorMonitor.Domain.Entities;
using MotorMonitor.Domain.Enums;

namespace MotorMonitor.Application.Services;

/// <summary>
/// 모니터링 서비스 구현
/// 가상 데이터를 생성하여 모터 상태를 시뮬레이션
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly MotorStatus _currentStatus;
    private readonly Random _random;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _monitoringTask;
    private bool _disposed;

    // 시뮬레이션 파라미터
    private double _simulationTime;
    private const double Frequency = 0.5;           // 사인파 주파수 (Hz)
    private const double Amplitude = 100.0;         // 위치 진폭 (mm)
    private const int VirtualResolution = 10000;    // 가상 분해능 (pulse/rev)
    private const double LeadPerRev = 10.0;         // 가상 리드 (mm/rev)

    public bool IsRunning { get; private set; }

    public event Action<MotorStatusDto>? StatusUpdated;

    public MonitoringService()
    {
        _currentStatus = new MotorStatus
        {
            Status = AxisStatus.Ready,
            Timestamp = DateTime.Now
        };
        _random = new Random();
        _simulationTime = 0;
    }

    public MotorStatusDto GetCurrentStatus()
    {
        return MotorStatusDto.FromEntity(_currentStatus);
    }

    public void StartMonitoring(int intervalMs = 100)
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
        IsRunning = true;

        _currentStatus.Status = AxisStatus.Running;

        _monitoringTask = RunMonitoringLoopAsync(_cts.Token);
    }

    public void StopMonitoring()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _timer?.Dispose();
        _timer = null;

        IsRunning = false;
        _currentStatus.Status = AxisStatus.Ready;

        // 정지 상태 통지
        StatusUpdated?.Invoke(MotorStatusDto.FromEntity(_currentStatus));
    }

    public void ResetValues()
    {
        // 모든 값을 0으로 초기화
        _currentStatus.EncoderCommand = 0;
        _currentStatus.EncoderFeedback = 0;
        _currentStatus.Position = 0;
        _currentStatus.Speed = 0;
        _currentStatus.Torque = 0;
        _currentStatus.Timestamp = DateTime.Now;

        // 시뮬레이션 시간도 리셋
        _simulationTime = 0;

        // UI 업데이트 알림
        StatusUpdated?.Invoke(MotorStatusDto.FromEntity(_currentStatus));
    }

    private async Task RunMonitoringLoopAsync(CancellationToken ct)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(ct))
            {
                UpdateSimulatedData();
                StatusUpdated?.Invoke(MotorStatusDto.FromEntity(_currentStatus));
            }
        }
        catch (OperationCanceledException)
        {
            // 정상 취소
        }
    }

    /// <summary>
    /// 가상 데이터 생성 (사인파 기반 시뮬레이션)
    /// </summary>
    private void UpdateSimulatedData()
    {
        // 시간 증가 (100ms 기준)
        _simulationTime += 0.1;

        // 사인파 기반 위치 생성 (mm)
        double position = Amplitude * Math.Sin(2 * Math.PI * Frequency * _simulationTime);

        // 속도 = 위치의 미분 (mm/s)
        double speed = Amplitude * 2 * Math.PI * Frequency * Math.Cos(2 * Math.PI * Frequency * _simulationTime);

        // 위치 → 엔코더 펄스 변환
        // pulse = position(mm) / lead(mm/rev) * resolution(pulse/rev)
        double pulsePerMm = VirtualResolution / LeadPerRev;
        long encoderCommand = (long)(position * pulsePerMm);

        // 피드백 = 커맨드 + 약간의 지연 + 노이즈
        int noise = _random.Next(-5, 6);  // ±5 pulse 노이즈
        long encoderFeedback = encoderCommand + noise;

        // 토크 = 가속도 비례 (사인파의 2차 미분) + 노이즈
        double acceleration = -Amplitude * Math.Pow(2 * Math.PI * Frequency, 2) * Math.Sin(2 * Math.PI * Frequency * _simulationTime);
        double torque = Math.Abs(acceleration) / 100.0 * 30.0 + _random.NextDouble() * 5.0;  // 0~35% 범위
        torque = Math.Min(torque, 100.0);  // 최대 100%

        // 상태 업데이트
        _currentStatus.EncoderCommand = encoderCommand;
        _currentStatus.EncoderFeedback = encoderFeedback;
        _currentStatus.Position = position;
        _currentStatus.Speed = speed;
        _currentStatus.Torque = torque;
        _currentStatus.Timestamp = DateTime.Now;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            StopMonitoring();
            _cts?.Dispose();
            _timer?.Dispose();
        }

        _disposed = true;
    }
}
