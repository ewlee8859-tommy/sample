using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using AvalonDock;
using AvalonDock.Layout;
using DevTestWpfCalApp.UI.Core.Constants;
using DevTestWpfCalApp.UI.Core.Contracts;
using DevTestWpfCalApp.UI.Core.Events;
using DevTestWpfCalApp.UI.Core.Services;
using MotorMonitor.Application.Interfaces;
using MotorMonitor.Application.Services;
using MotorMonitor.ViewModels;
using MotorMonitor.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace MotorMonitor;

/// <summary>
/// 모니터링 플러그인 모듈
/// 모터 시스템 상태를 실시간으로 모니터링
/// </summary>
[Module(ModuleName = "Monitoring", OnDemand = true)]
public class MonitoringModule : IPluginModule
{
    private readonly IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly IContainerProvider _containerProvider;
    private MonitoringView? _view;
    private bool _disposed;

    public string ModuleId => "Monitoring";
    public string DisplayName => "Motor Monitor";
    public string Description => "모터 시스템 상태 실시간 모니터링";
    public string Version => "1.0.0";
    public bool IsActive { get; private set; }

    public MonitoringModule(
        IRegionManager regionManager,
        IEventAggregator eventAggregator,
        IContainerProvider containerProvider)
    {
        _regionManager = regionManager;
        _eventAggregator = eventAggregator;
        _containerProvider = containerProvider;
    }

    /// <summary>
    /// 모듈 자체 서비스 등록 (Self-Contained)
    /// </summary>
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 모듈 자체 서비스 등록
        containerRegistry.RegisterSingleton<IMonitoringService, MonitoringService>();

        // 뷰 네비게이션 등록
        containerRegistry.RegisterForNavigation<MonitoringView, MonitoringViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        // ModuleManagerService에 자신을 등록 (인스턴스 캐싱)
        var moduleManagerService = containerProvider.Resolve<IModuleManagerService>();
        moduleManagerService.RegisterPluginInstance(ModuleId, this);

        Activate();
    }

    /// <summary>
    /// 모듈 활성화 - Region에 View 추가
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        var region = _regionManager.Regions[RegionNames.DocumentRegion];

        // View 생성 및 추가
        _view = _containerProvider.Resolve<MonitoringView>();
        region.Add(_view);
        region.Activate(_view);

        // AvalonDock X 버튼 처리를 위한 이벤트 구독
        region.Views.CollectionChanged += OnRegionViewsChanged;

        IsActive = true;

        // 모듈 로드 완료 이벤트 발행
        _eventAggregator.GetEvent<ModuleLoadedEvent>().Publish(new ModuleLoadedEventArgs
        {
            ModuleId = ModuleId,
            DisplayName = DisplayName,
            Description = Description,
            Version = Version
        });
    }

    /// <summary>
    /// 모듈 비활성화 - Region에서 View 제거
    /// </summary>
    public void Deactivate()
    {
        if (_regionManager.Regions.ContainsRegionWithName(RegionNames.DocumentRegion))
        {
            var region = _regionManager.Regions[RegionNames.DocumentRegion];
            region.Views.CollectionChanged -= OnRegionViewsChanged;

            if (_view != null && region.Views.Contains(_view))
            {
                region.Remove(_view);
            }
        }

        // ViewModel 정리 (모니터링 타이머 중지)
        if (_view?.DataContext is MonitoringViewModel vm)
        {
            vm.Cleanup();
        }

        _view = null;
        IsActive = false;

        // 모듈 언로드 이벤트 발행
        _eventAggregator.GetEvent<ModuleUnloadedEvent>().Publish(new ModuleUnloadedEventArgs
        {
            ModuleId = ModuleId
        });
    }

    /// <summary>
    /// 모듈을 DocumentRegion 최상위로 활성화
    /// </summary>
    public void BringToFront()
    {
        if (!IsActive)
        {
            Activate();
            return;
        }

        if (_view == null) return;

        // AvalonDock DockingManager를 통해 LayoutDocument 검색
        var mainWindow = System.Windows.Application.Current.MainWindow;
        var dockingManager = FindVisualChild<DockingManager>(mainWindow);
        if (dockingManager?.Layout == null) return;

        var doc = FindLayoutDocumentByContent(dockingManager.Layout, _view);
        if (doc != null)
        {
            doc.IsSelected = true;
            doc.IsActive = true;
        }
    }

    /// <summary>
    /// AvalonDock X 버튼으로 탭이 닫힌 경우 상태 동기화
    /// ModuleUnloadedEvent는 발행하지 않음 (탭만 닫힌 것이므로 네비게이션 유지)
    /// </summary>
    private void OnRegionViewsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove &&
            e.OldItems?.Contains(_view) == true)
        {
            // 이벤트 구독 해제
            var region = _regionManager.Regions[RegionNames.DocumentRegion];
            region.Views.CollectionChanged -= OnRegionViewsChanged;

            // ViewModel 정리 (모니터링 타이머 중지)
            if (_view?.DataContext is MonitoringViewModel vm)
            {
                vm.Cleanup();
            }

            _view = null;
            IsActive = false;
        }
    }

    /// <summary>
    /// VisualTree에서 특정 타입의 자식 요소 검색
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;

            var found = FindVisualChild<T>(child);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// AvalonDock Layout에서 Content로 LayoutDocument 검색
    /// </summary>
    private static LayoutDocument? FindLayoutDocumentByContent(ILayoutContainer container, object content)
    {
        foreach (var child in container.Children)
        {
            if (child is LayoutDocument doc && doc.Content == content)
                return doc;

            if (child is ILayoutContainer childContainer)
            {
                var found = FindLayoutDocumentByContent(childContainer, content);
                if (found != null) return found;
            }
        }
        return null;
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
            Deactivate();
        }

        _disposed = true;
    }
}
