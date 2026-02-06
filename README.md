# MotorMonitor

모터 상태 실시간 모니터링 플러그인

## 개요

DevTestWpfCalApp 플랫폼용 플러그인 모듈입니다.
모터 시스템의 상태를 실시간으로 모니터링하고 표시합니다.

## 기능

- 실시간 모터 상태 모니터링 (100ms 주기)
- Encoder Command/Feedback 표시
- Position, Speed, Torque 값 표시
- Position Error 계산 및 표시
- 상태 표시등 (Ready/Running/Error)
- 모니터링 값 초기화 (Reset)

## 모니터링 항목

| 항목 | 설명 | 단위 |
|------|------|------|
| Encoder Command | 명령 펄스 | pulse |
| Encoder Feedback | 피드백 펄스 | pulse |
| Position Error | 위치 오차 | pulse |
| Speed | 속도 | mm/s |
| Position | 위치 | mm |
| Torque | 토크 | % |

## 프로젝트 구조

```
src/
├── Application/
│   ├── DTOs/              # 데이터 전송 객체
│   ├── Interfaces/        # 서비스 인터페이스
│   └── Services/          # 모니터링 서비스
├── Domain/
│   └── Entities/          # 도메인 엔티티
├── ViewModels/            # MVVM ViewModel
├── Views/                 # WPF View
└── MonitoringModule.cs    # 플러그인 진입점
```

## 빌드

```bash
dotnet build
```

빌드된 DLL을 DevTestWpfCalApp의 `Plugins/` 폴더에 복사하면 자동으로 로드됩니다.

## 의존성

- DevTestWpfCalApp.UI.Core (SDK)
- Prism.DryIoc

## 플랫폼

- [DevTestWpfCalApp](https://github.com/ewlee8859-tommy/sandbox) - 호스트 플랫폼
