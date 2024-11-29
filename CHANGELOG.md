# Changelog
All notable changes to this project will be documented in this file.

## 버전 관리 정책

이 프로젝트는 Semantic Versioning을 따릅니다:
- **Major.Minor.Patch** 형식
  - **Major**: 호환성이 깨지는 변경
  - **Minor**: 하위 호환성 있는 기능 추가
  - **Patch**: 하위 호환성 있는 버그 수정


## [0.1.0] - 2024-11-30
### Added
- 기본 외부 프로그램 통합 기능 구현
  - 프로세스 시작/종료 관리
  - 비동기 통신 지원
  - 에러 처리 및 로깅
- Mock 객체를 통한 테스트 프레임워크 구축
  - MockExternalProgram
  - MockCommunicationProtocol
  - MockLogger
- 단위 테스트 구현
  - 프로세스 생명주기 테스트
  - 상태 전이 테스트
  - 에러 처리 테스트
  - 통신 프로토콜 테스트
- TODO 리스트 작성
  - 향후 개선사항 및 테스트 시나리오 정리
  - 코드 개선 포인트 식별

### Changed
- 비동기 작업 처리 방식 개선
- 에러 처리 로직 강화
- 테스트 코드 구조화

### Fixed
- 프로세스 종료 시 리소스 정리 문제
- 비동기 작업 취소 처리 개선
- 테스트의 타임아웃 처리 수정



## [0.0.1] - 2024-11-30
### 추가됨
- 기본 패키지 구조 설정
- 외부 프로그램 통신을 위한 핵심 기능 구현
  - `IExternalProgram` 인터페이스 정의
  - `ExternalProgramBase` 추상 클래스 구현
  - 프로그램 상태 관리 시스템
  - 에러 처리 및 이벤트 시스템
- 상수 정의
  - 네트워크 통신 관련 상수
  - 타임아웃 설정
  - 재시도 정책

### 기술적 세부사항
- Unity 2022.3 LTS 버전 지원
- TCP/IP 기반 통신 구조
- 비동기 작업 지원 (async/await)
- 이벤트 기반 상태 관리

### 알려진 문제
- 현재 버전은 초기 구현 단계로, 실제 사용 시 안정성 테스트가 필요합니다.

## 예정된 변경사항
- 다양한 통신 프로토콜 지원 추가
- 보안 기능 강화
- 성능 최적화
- 유닛 테스트 추가