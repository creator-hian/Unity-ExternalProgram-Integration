# Unity-ExternalProgram-Integration

Unity 외부 프로그램 통합을 위한 패키지입니다.

## 개요

이 패키지는 Unity에서 외부 프로그램과의 통신 및 제어를 위한 기능을 제공합니다.

### 주요 기능

- 외부 프로그램 실행 및 종료 관리
- 프로세스 상태 모니터링
- 확장 가능한 통신 프로토콜 시스템
- 명령어 전송 및 응답 처리
- 에러 처리 및 로깅

## 설치 요구사항

- Unity 2022.3 이상
- .NET Standard 2.0 호환

## 사용 방법

### 기본 설정

```csharp
  // 프로그램 설정 생성
  var config = new ProgramConfig
  {
    processName = "MyProgram",
    executablePath = "path/to/program.exe",
    protocolType = "TCP",
    portNumber = 8080
  };
  // 프로토콜 팩토리 설정
  var protocolFactory = new CommunicationProtocolFactory();
  protocolFactory.RegisterProvider(new TcpProtocolProvider());
  // 외부 프로그램 매니저 생성
  var programManager = new ExternalProgramManager(config, protocolFactory);
```

### 새로운 통신 프로토콜 추가하기

1. `ICommunicationProtocolProvider` 인터페이스 구현:

```csharp
public class CustomProtocolProvider : ICommunicationProtocolProvider
{
    public string ProtocolType => "CUSTOM";
    public ICommunicationProtocol CreateProtocol(ProgramConfig config)
    {
        return new CustomProtocol(config);
    }
}
```

2. 프로토콜 구현:

```csharp
public class CustomProtocol : ICommunicationProtocol
{
  public CustomProtocol(ProgramConfig config)
  {
  // 프로토콜 초기화
  }
  // ICommunicationProtocol 인터페이스 구현
...
}
```

3. 프로토콜 등록:

```csharp
var factory = new CommunicationProtocolFactory();
factory.RegisterProvider(new ustomProtocolProvider());
```

### 프로세스 관리

```csharp
// 프로그램 시작
await programManager.StartAsync();

// 명령어 전송
await programManager.SendCommandAsync("command");

// 프로그램 종료
await programManager.StopAsync();
```

### 이벤트 처리

```csharp
programManager.OnStateChanged += (state) =>
{
  Debug.Log($"Program state changed to: {state}");  
};

programManager.OnOutputReceived += (output) =>
{
  Debug.Log($"Received output: {output}");
};

programManager.OnError += (error) =>
{
  Debug.LogError($"Error occurred: {error.Message}");
};
```

## 구조
- **Constants**: 상수 정의
- **Core**: 
  - Base: 기본 구현 클래스
  - Communication: 통신 프로토콜 관련 클래스
  - Enum: 상태 및 에러 타입 정의
  - Interface: 핵심 인터페이스
  - Model: 설정 및 데이터 모델
  - Manager: 프로그램 관리 클래스

## 라이선스
[라이선스 정보 참조](LICENSE.md)

## 기여
프로젝트에 기여하고 싶으시다면:
1. 이슈를 생성하여 변경사항을 논의합니다.
2. Fork 후 변경사항을 구현합니다.
3. Pull Request를 생성합니다.

## 원작성자
- [Hian](https://github.com/creator-hian)