using System.Collections.Specialized;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using DevTestWpfCalApp.Sdk.Abstractions.Regions;
using DevTestWpfCalApp.Sdk.Common.Module;
using MotorMonitor.Application.Interfaces;
using MotorMonitor.Application.Services;
using MotorMonitor.ViewModels;
using MotorMonitor.Views;

namespace MotorMonitor;

/// <summary>
/// 모니터링 플러그인 모듈
/// PluginModuleBase 상속 — 이벤트 자동 발행, Dispose 자동 처리
/// </summary>
[Module(ModuleName = "Monitoring", OnDemand = true)]
public class MonitoringModule : PluginModuleBase
{
    private MonitoringView? _view;
    private MonitoringToolbarView? _toolbarView;
    private MonitoringViewModel? _viewModel;

    public override string ModuleId => "Monitoring";
    public override string DisplayName => "Motor Monitor";
    public override string Description => "모터 시스템 상태 실시간 모니터링";
    public override string Version => "1.0.0";

    public MonitoringModule(IRegionManager regionManager, IEventAggregator eventAggregator)
        : base(regionManager, eventAggregator) { }

    public override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IMonitoringService, MonitoringService>();
        containerRegistry.Register<MonitoringViewModel>();
        containerRegistry.Register<MonitoringView>();
        containerRegistry.Register<MonitoringToolbarView>();
    }

    protected override void OnActivate()
    {
        var documentRegion = RegionManager.Regions[RegionNames.DocumentRegion];

        var container = ContainerProvider!;
        _viewModel = container.Resolve<MonitoringViewModel>();
        _view = container.Resolve<MonitoringView>();
        _view.DataContext = _viewModel;
        documentRegion.Add(_view);
        documentRegion.Activate(_view);

        // X 버튼 처리
        documentRegion.Views.CollectionChanged += OnRegionViewsChanged;

        // 툴바
        if (RegionManager.Regions.ContainsRegionWithName(RegionNames.ModuleToolbarRegion))
        {
            var toolbarRegion = RegionManager.Regions[RegionNames.ModuleToolbarRegion];
            _toolbarView = container.Resolve<MonitoringToolbarView>();
            _toolbarView.DataContext = _viewModel;
            toolbarRegion.Add(_toolbarView);
            toolbarRegion.Activate(_toolbarView);
        }
    }

    protected override void OnDeactivate()
    {
        if (RegionManager.Regions.ContainsRegionWithName(RegionNames.DocumentRegion))
        {
            var documentRegion = RegionManager.Regions[RegionNames.DocumentRegion];
            documentRegion.Views.CollectionChanged -= OnRegionViewsChanged;
            if (_view != null && documentRegion.Views.Contains(_view))
                documentRegion.Remove(_view);
        }

        if (RegionManager.Regions.ContainsRegionWithName(RegionNames.ModuleToolbarRegion))
        {
            var toolbarRegion = RegionManager.Regions[RegionNames.ModuleToolbarRegion];
            if (_toolbarView != null && toolbarRegion.Views.Contains(_toolbarView))
                toolbarRegion.Remove(_toolbarView);
        }

        _viewModel?.Cleanup();
        _view = null;
        _toolbarView = null;
        _viewModel = null;
    }

    public override void BringToFront()
    {
        if (_view == null)
        {
            // 탭 X로 닫힌 후 재오픈 — View 재생성
            OnActivate();
            IsActive = true;
            return;
        }

        if (!IsActive) { Activate(); return; }

        var documentRegion = RegionManager.Regions[RegionNames.DocumentRegion];
        documentRegion.Activate(_view);
    }

    /// <summary>
    /// 탭 X 버튼 클릭 — UI만 닫기, lifecycle 상태 유지
    /// Deactivate()는 이벤트를 발행하지 않으므로 안전하게 호출 가능
    /// lifecycle 상태는 Activated 유지 → 네비게이터 노드 보존
    /// </summary>
    private void OnRegionViewsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove &&
            e.OldItems?.Contains(_view) == true)
        {
            Deactivate();
        }
    }
}
