# Unity External Program Integration

Unity에서 외부 프로그램을 실행하고 통신하기 위한 통합 패키지입니다.

## 특징

- 외부 프로그램 생명주기 관리
- 비동기 통신 지원
- 강력한 에러 처리
- 유연한 설정 시스템
- 포괄적인 테스트 커버리지

## 설치

1. Unity 패키지 매니저를 통한 설치:
   - Window > Package Manager
   - '+' 버튼 > Add package from git URL
   - URL 입력: `https://github.com/your-repo/Unity-ExternalProgram-Integration.git`

## 설치 요구사항

- Unity 2022.3 이상
- .NET Standard 2.0 호환

## 사용 방법

### 기본 설정
```csharp
var config = new ProgramConfig(
    processName: "MyProgram",
    executablePath: "path/to/program.exe",
    arguments: "-port 12345"
);
```

### 프로그램 실행
```csharp
using FAMOZ.ExternalProgram.Core;

var program = new ExternalProgram(config);
await program.StartAsync();
await program.ConnectAsync();
```

### 명령 전송
```csharp
await program.SendCommandAsync("command");
var response = await program.WaitForResponseAsync("expected");
```

### 에러 처리
```csharp
program.OnError += error =>
{
    switch (error.Type)
    {
        case ErrorType.ProcessStart:
            // 프로세스 시작 실패 처리
            break;
        case ErrorType.Communication:
            // 통신 에러 처리
            break;
    }
};
```

## 테스트

패키지는 포괄적인 테스트 스위트를 포함하고 있습니다:
- 단위 테스트
- 통합 테스트
- Mock 객체를 통한 시뮬레이션

테스트 실행:
1. Unity Test Runner 열기 (Window > General > Test Runner)
2. 'Run All' 클릭

## 라이선스


## 기여

버그 리포트, 기능 요청, 풀 리퀘스트를 환영합니다. 
자세한 내용은 [TODO](TODO.md) 파일을 참조하세요.

## 작성자

- Creator-HIAN (https://github.com/Creator-HIAN)