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
    private MonitoringToolbarView? _toolbarView;
    private MonitoringViewModel? _viewModel;
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

        // ViewModel 등록 (수동 DataContext 설정을 위해 직접 등록)
        containerRegistry.Register<MonitoringViewModel>();

        // 뷰 등록
        containerRegistry.Register<MonitoringView>();
        containerRegistry.Register<MonitoringToolbarView>();
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

        var documentRegion = _regionManager.Regions[RegionNames.DocumentRegion];

        // 공유 ViewModel 생성
        _viewModel = _containerProvider.Resolve<MonitoringViewModel>();

        // 메인 View 생성 및 추가 (DataContext 수동 설정)
        _view = _containerProvider.Resolve<MonitoringView>();
        _view.DataContext = _viewModel;
        documentRegion.Add(_view);
        documentRegion.Activate(_view);

        // AvalonDock X 버튼 처리를 위한 이벤트 구독
        documentRegion.Views.CollectionChanged += OnRegionViewsChanged;

        // 툴바 View 생성 및 추가 (동일 ViewModel 공유)
        if (_regionManager.Regions.ContainsRegionWithName(RegionNames.ModuleToolbarRegion))
        {
            var toolbarRegion = _regionManager.Regions[RegionNames.ModuleToolbarRegion];
            _toolbarView = _containerProvider.Resolve<MonitoringToolbarView>();
            _toolbarView.DataContext = _viewModel;
            toolbarRegion.Add(_toolbarView);
            toolbarRegion.Activate(_toolbarView);
        }

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
        // Document Region에서 메인 뷰 제거
        if (_regionManager.Regions.ContainsRegionWithName(RegionNames.DocumentRegion))
        {
            var documentRegion = _regionManager.Regions[RegionNames.DocumentRegion];
            documentRegion.Views.CollectionChanged -= OnRegionViewsChanged;

            if (_view != null && documentRegion.Views.Contains(_view))
            {
                documentRegion.Remove(_view);
            }
        }

        // Module Toolbar Region에서 툴바 뷰 제거
        if (_regionManager.Regions.ContainsRegionWithName(RegionNames.ModuleToolbarRegion))
        {
            var toolbarRegion = _regionManager.Regions[RegionNames.ModuleToolbarRegion];
            if (_toolbarView != null && toolbarRegion.Views.Contains(_toolbarView))
            {
                toolbarRegion.Remove(_toolbarView);
            }
        }

        // ViewModel 정리 (모니터링 타이머 중지)
        _viewModel?.Cleanup();

        _view = null;
        _toolbarView = null;
        _viewModel = null;
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
            var documentRegion = _regionManager.Regions[RegionNames.DocumentRegion];
            documentRegion.Views.CollectionChanged -= OnRegionViewsChanged;

            // 툴바도 함께 제거
            if (_regionManager.Regions.ContainsRegionWithName(RegionNames.ModuleToolbarRegion))
            {
                var toolbarRegion = _regionManager.Regions[RegionNames.ModuleToolbarRegion];
                if (_toolbarView != null && toolbarRegion.Views.Contains(_toolbarView))
                {
                    toolbarRegion.Remove(_toolbarView);
                }
            }

            // ViewModel 정리 (모니터링 타이머 중지)
            _viewModel?.Cleanup();

            _view = null;
            _toolbarView = null;
            _viewModel = null;
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
