# NeoPhase - 팀 협업 Git 설정 가이드

> 처음 프로젝트를 받은 팀원은 이 문서를 **순서대로** 따라하세요.

---

## 1단계 | Unity Editor 강제 텍스트 직렬화 확인 (최우선)

Unity Smart Merge는 **YAML 텍스트 형식**으로 저장된 파일에서만 동작합니다.

1. Unity Editor 열기
2. `Edit` → `Project Settings` → `Editor` 클릭
3. **Version Control** 섹션
   - `Mode` → `Visible Meta Files` 선택
4. **Asset Serialization** 섹션
   - `Mode` → **`Force Text`** 선택
5. 변경 후 Unity 재시작

> ⚠️ 이 설정이 없으면 Smart Merge가 동작하지 않습니다.

---

## 2단계 | UnityYAMLMerge git config 등록

UnityYAMLMerge.exe 경로를 git에 등록합니다.

### Windows PowerShell에서 실행:

```powershell
# Unity 버전에 맞게 경로를 수정하세요
# 예시: Unity 6000.0.x 기준

git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.0.32f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'
git config --global merge.unityyamlmerge.recursive binary
```

### Unity 경로 확인 방법

```powershell
# 설치된 Unity 버전 경로 확인
Get-ChildItem "C:\Program Files\Unity\Hub\Editor\" | Select-Object Name
```

출력 예시:
```
6000.0.32f1
2022.3.45f1
```

확인된 버전으로 경로를 교체하세요:
```
C:/Program Files/Unity/Hub/Editor/{여기에_버전}/Editor/Data/Tools/UnityYAMLMerge.exe
```

### 설정 확인 명령어

```powershell
git config --global --list | Select-String "unityyamlmerge"
```

---

## 3단계 | fallback merge 설정 (충돌 발생 시 fallback)

UnityYAMLMerge가 처리 못하는 경우 사용할 merge 툴을 설정합니다.

```powershell
# VS Code를 fallback merge 툴로 설정
git config --global merge.tool vscode
git config --global mergetool.vscode.cmd 'code --wait $MERGED'

# 또는 기본 diff3 사용 (외부 툴 없이)
git config --global merge.conflictstyle diff3
```

---

## 4단계 | 설정 완료 확인

```powershell
git config --global --list
```

아래 항목이 보이면 정상입니다:
```
merge.unityyamlmerge.name=Unity SmartMerge
merge.unityyamlmerge.driver=...UnityYAMLMerge.exe...
merge.unityyamlmerge.recursive=binary
```

---

## 5단계 | 프로젝트 클론 후 최초 세팅

```powershell
# 1. 저장소 클론
git clone https://github.com/RazeKR/NeoPhase-TeamProject.git

# 2. develop 브랜치로 이동
cd NeoPhase-TeamProject
git checkout develop

# 3. 내 작업 브랜치 생성 (예: feature/플레이어-이동)
git checkout -b feature/플레이어-이동

# 4. 작업 후 커밋 & 푸시
git add .
git commit -m "feat: 플레이어 이동 구현"
git push -u origin feature/플레이어-이동
```

---

## 브랜치 전략

```
main        ← 배포/릴리즈 브랜치 (직접 push 금지)
  └── develop     ← 통합 개발 브랜치 (PR merge 대상)
        ├── feature/기능명    ← 기능 개발용
        ├── fix/버그명        ← 버그 수정용
        └── art/리소스명      ← 아트 에셋 작업용
```

### 규칙
- `main`, `develop`에 **직접 push 금지**
- 작업 완료 후 **Pull Request** 생성 → 리뷰어 지정
- develop과 **최소 하루 1회 sync** (git pull --rebase)

---

## 씬(Scene) 충돌 방지 전략

### 씬 분리 원칙
```
Assets/
  Scenes/
    Main.unity          ← 최종 통합 씬 (팀장만 수정)
    Dev_플레이어.unity   ← 플레이어 작업용 씬
    Dev_맵.unity         ← 맵 담당자 씬
    Dev_UI.unity         ← UI 담당자 씬
    Dev_몬스터.unity     ← 몬스터 담당자 씬
```

- **각자 개인 Dev 씬에서만 작업**
- 최종 통합은 팀장이 Main.unity에서 진행
- Additive Scene Loading 활용 권장

### Prefab 작업 원칙
- 씬에 직접 오브젝트 추가 ❌ → Prefab으로 만들고 씬에 배치 ✅
- 같은 Prefab을 동시에 2명이 수정하지 않기
- Prefab Variant 활용으로 기반 Prefab 공유

---

## merge conflict 발생 시 처리 절차

### 1. Smart Merge 자동 실행 (설정된 경우 자동)
```powershell
git merge develop
# UnityYAMLMerge가 자동으로 실행됨
```

### 2. 수동 merge tool 실행
```powershell
git mergetool
```

### 3. 긴급 상황 - 한쪽 버전으로 덮어쓰기
```powershell
# 내 버전(ours) 유지
git checkout --ours Assets/Scenes/Main.unity
git add Assets/Scenes/Main.unity

# 상대방 버전(theirs) 채택
git checkout --theirs Assets/Scenes/Main.unity
git add Assets/Scenes/Main.unity
```

---

## 체크리스트 (팀원 필수 확인)

- [ ] Unity Editor: Asset Serialization → **Force Text** 설정
- [ ] Unity Editor: Version Control → **Visible Meta Files** 설정
- [ ] git config: `merge.unityyamlmerge.driver` 등록 완료
- [ ] .gitattributes 파일이 프로젝트에 존재하는지 확인
- [ ] 작업 전 항상 `git pull --rebase origin develop` 실행
- [ ] 개인 Dev 씬에서만 작업

---

## 자주 쓰는 명령어

```powershell
# develop 최신화 후 내 브랜치에 rebase
git fetch origin
git rebase origin/develop

# 충돌 없이 pull하기
git pull --rebase origin develop

# 스테이징 영역 확인
git status

# 변경 내용 확인
git diff HEAD

# 마지막 커밋 취소 (로컬만, push 전)
git reset --soft HEAD~1
```
