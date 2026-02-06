using System.Windows;
using System.Windows.Input;
using DevTestWpfCalApp.Sdk.Common.Base;
using MotorMonitor.Application.DTOs;
using MotorMonitor.Application.Interfaces;

namespace MotorMonitor.ViewModels;

/// <summary>
/// 모니터링 ViewModel
/// 모터 상태를 실시간으로 표시
/// </summary>
public class MonitoringViewModel : ViewModelBase
{
    private readonly IMonitoringService _monitoringService;

    // 표시용 속성
    private string _encoderCommand = "0 pulse";
    public string EncoderCommand
    {
        get => _encoderCommand;
        private set => SetProperty(ref _encoderCommand, value);
    }

    private string _encoderFeedback = "0 pulse";
    public string EncoderFeedback
    {
        get => _encoderFeedback;
        private set => SetProperty(ref _encoderFeedback, value);
    }

    private string _speed = "0.00 mm/s";
    public string Speed
    {
        get => _speed;
        private set => SetProperty(ref _speed, value);
    }

    private string _position = "0.00 mm";
    public string Position
    {
        get => _position;
        private set => SetProperty(ref _position, value);
    }

    private string _torque = "0.0 %";
    public string Torque
    {
        get => _torque;
        private set => SetProperty(ref _torque, value);
    }

    private string _positionError = "0 pulse";
    public string PositionError
    {
        get => _positionError;
        private set => SetProperty(ref _positionError, value);
    }

    private string _axisStatus = "대기";
    public string AxisStatus
    {
        get => _axisStatus;
        private set => SetProperty(ref _axisStatus, value);
    }

    private string _statusColor = "#00AA00";
    public string StatusColor
    {
        get => _statusColor;
        private set => SetProperty(ref _statusColor, value);
    }

    private bool _isMonitoring;
    public bool IsMonitoring
    {
        get => _isMonitoring;
        private set
        {
            if (SetProperty(ref _isMonitoring, value))
            {
                RaisePropertyChanged(nameof(CanStart));
                RaisePropertyChanged(nameof(CanStop));
            }
        }
    }

    public bool CanStart => !IsMonitoring;
    public bool CanStop => IsMonitoring;

    // 커맨드
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ScanCommand { get; }

    public MonitoringViewModel(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;

        // 이벤트 구독
        _monitoringService.StatusUpdated += OnStatusUpdated;

        // 커맨드 초기화
        StartCommand = new RelayCommand(StartMonitoring, () => CanStart);
        StopCommand = new RelayCommand(StopMonitoring, () => CanStop);
        ResetCommand = new RelayCommand(ResetValues);
        ScanCommand = new RelayCommand(OnScan);

        // 초기 상태 표시
        UpdateDisplay(_monitoringService.GetCurrentStatus());
    }

    private void StartMonitoring()
    {
        _monitoringService.StartMonitoring(100);  // 100ms 주기
        IsMonitoring = true;
    }

    private void StopMonitoring()
    {
        _monitoringService.StopMonitoring();
        IsMonitoring = false;
    }

    private void ResetValues()
    {
        _monitoringService.ResetValues();
    }

    private void OnScan()
    {
        // 빈 구현 (추후 장치 스캔 기능)
    }

    /// <summary>
    /// 상태 업데이트 이벤트 핸들러
    /// </summary>
    private void OnStatusUpdated(MotorStatusDto status)
    {
        // UI 스레드에서 업데이트
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            UpdateDisplay(status);
        });
    }

    /// <summary>
    /// UI 표시 업데이트
    /// </summary>
    private void UpdateDisplay(MotorStatusDto status)
    {
        EncoderCommand = status.EncoderCommandDisplay;
        EncoderFeedback = status.EncoderFeedbackDisplay;
        Speed = status.SpeedDisplay;
        Position = status.PositionDisplay;
        Torque = status.TorqueDisplay;
        PositionError = status.PositionErrorDisplay;
        AxisStatus = status.StatusDisplayName;
        StatusColor = status.StatusColor;
    }

    /// <summary>
    /// 리소스 정리 (모듈 비활성화 시 호출)
    /// </summary>
    public void Cleanup()
    {
        _monitoringService.StatusUpdated -= OnStatusUpdated;

        if (IsMonitoring)
        {
            _monitoringService.StopMonitoring();
            IsMonitoring = false;
        }
    }
}
