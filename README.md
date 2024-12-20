# Unity External Program Integration

Unity에서 외부 프로그램을 실행하고 통신하기 위한 통합 패키지입니다.

## 요구사항

- Unity 2021.3 이상
- .NET Standard 2.1

## 특징

- 외부 프로그램 생명주기 관리
- 비동기 통신 지원
- 강력한 에러 처리
- 유연한 설정 시스템
- 포괄적인 테스트 커버리지

## 설치 방법

### UPM을 통한 설치 (Git URL 사용)

#### 선행 조건

- Git 클라이언트(최소 버전 2.14.0)가 설치되어 있어야 합니다.
- Windows 사용자의 경우 `PATH` 시스템 환경 변수에 Git 실행 파일 경로가 추가되어 있어야 합니다.

#### 설치 방법 1: Package Manager UI 사용

1. Unity 에디터에서 Window > Package Manager를 엽니다.
2. 좌측 상단의 + 버튼을 클릭하고 "Add package from git URL"을 선택합니다.

   ![Package Manager Add Git URL](https://i.imgur.com/1tCNo66.png)
3. 다음 URL을 입력합니다:

```text
https://github.com/creator-hian/Unity-ExternalProgram-Integration.git
```
<!-- markdownlint-disable MD029 -->
4. 'Add' 버튼을 클릭합니다.

   ![Package Manager Add Button](https://i.imgur.com/yIiD4tT.png)
<!-- markdownlint-enable MD029 -->

#### 설치 방법 2: manifest.json 직접 수정

1. Unity 프로젝트의 `Packages/manifest.json` 파일을 열어 다음과 같이 dependencies 블록에 패키지를 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.external-program-integration": "https://github.com/creator-hian/Unity-ExternalProgram-Integration.git",
    ...
  }
}
```

#### 특정 버전 설치

특정 버전을 설치하려면 URL 끝에 #{version} 을 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.external-program-integration": "https://github.com/creator-hian/Unity-ExternalProgram-Integration.git#0.1.0",
    ...
  }
}
```

#### 참조 문서

- [Unity 공식 매뉴얼 - Git URL을 통한 패키지 설치](https://docs.unity3d.com/kr/2023.2/Manual/upm-ui-giturl.html)

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

- Creator-HIAN (<https://github.com/Creator-HIAN>)
