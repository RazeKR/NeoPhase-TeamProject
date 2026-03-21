# ============================================================
# Unity 충돌 해결 스크립트 (PowerShell)
# 사용법: .\scripts\resolve-conflict.ps1 [옵션]
# ============================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("ours", "theirs", "status", "meta-only", "packages-only", "settings-only")]
    [string]$Mode = "status"
)

$ProjectRoot = "ExportedProject"

function Show-ConflictStatus {
    Write-Host "`n=== 현재 충돌 상태 ===" -ForegroundColor Cyan
    $conflicts = git diff --name-only --diff-filter=U
    if (-not $conflicts) {
        Write-Host "충돌 없음 ✓" -ForegroundColor Green
        return
    }
    $conflicts | ForEach-Object {
        $type = switch -Wildcard ($_) {
            "*.meta"              { "[META]     " }
            "*/manifest.json"     { "[PACKAGES] " }
            "*/packages-lock.json"{ "[PKG-LOCK] " }
            "*/ProjectSettings/*" { "[SETTINGS] " }
            "*.unity"             { "[SCENE]    " }
            "*.prefab"            { "[PREFAB]   " }
            default               { "[OTHERS]   " }
        }
        Write-Host "  $type $_" -ForegroundColor Yellow
    }
    Write-Host "`n총 충돌 파일: $($conflicts.Count)개" -ForegroundColor Red
}

function Resolve-MetaFiles {
    param([string]$Strategy)
    Write-Host "`n-- .meta 파일 처리 중 ($Strategy) --" -ForegroundColor Magenta
    $metaConflicts = git diff --name-only --diff-filter=U | Where-Object { $_ -like "*.meta" }
    if (-not $metaConflicts) { Write-Host "  .meta 충돌 없음" -ForegroundColor Green; return }
    $metaConflicts | ForEach-Object {
        git checkout --$Strategy $_
        git add $_
        Write-Host "  ✓ $_" -ForegroundColor Green
    }
}

function Resolve-PackageFiles {
    param([string]$Strategy)
    Write-Host "`n-- Packages 파일 처리 중 ($Strategy) --" -ForegroundColor Magenta

    $manifestConflict = git diff --name-only --diff-filter=U | Where-Object { $_ -like "*/manifest.json" }
    if ($manifestConflict) {
        git checkout --$Strategy $manifestConflict
        git add $manifestConflict
        Write-Host "  ✓ manifest.json" -ForegroundColor Green
    }

    $lockConflict = git diff --name-only --diff-filter=U | Where-Object { $_ -like "*/packages-lock.json" }
    if ($lockConflict) {
        # packages-lock.json은 삭제 후 Unity가 재생성하도록 함
        Remove-Item $lockConflict -ErrorAction SilentlyContinue
        git add $lockConflict
        Write-Host "  ✓ packages-lock.json (삭제 → Unity 재생성 예정)" -ForegroundColor Green
    }
}

function Resolve-ProjectSettings {
    param([string]$Strategy)
    Write-Host "`n-- ProjectSettings 처리 중 ($Strategy) --" -ForegroundColor Magenta
    $settingsConflicts = git diff --name-only --diff-filter=U | Where-Object { $_ -like "*/ProjectSettings/*" }
    if (-not $settingsConflicts) { Write-Host "  ProjectSettings 충돌 없음" -ForegroundColor Green; return }
    $settingsConflicts | ForEach-Object {
        git checkout --$Strategy $_
        git add $_
        Write-Host "  ✓ $_" -ForegroundColor Green
    }
}

# ── 실행 ────────────────────────────────────────────────────

switch ($Mode) {
    "status" {
        Show-ConflictStatus
        Write-Host "`n사용법:" -ForegroundColor Cyan
        Write-Host "  .\scripts\resolve-conflict.ps1 -Mode theirs      # 모든 충돌을 theirs로 해결"
        Write-Host "  .\scripts\resolve-conflict.ps1 -Mode ours        # 모든 충돌을 ours로 해결"
        Write-Host "  .\scripts\resolve-conflict.ps1 -Mode meta-only   # .meta만 theirs로 해결"
        Write-Host "  .\scripts\resolve-conflict.ps1 -Mode packages-only"
        Write-Host "  .\scripts\resolve-conflict.ps1 -Mode settings-only"
    }
    "theirs" {
        Show-ConflictStatus
        Resolve-MetaFiles -Strategy "theirs"
        Resolve-PackageFiles -Strategy "theirs"
        Resolve-ProjectSettings -Strategy "theirs"
        $remaining = git diff --name-only --diff-filter=U
        if ($remaining) {
            Write-Host "`n-- 나머지 파일 일괄 처리 (theirs) --" -ForegroundColor Magenta
            $remaining | ForEach-Object { git checkout --theirs $_; git add $_ }
        }
        Write-Host "`n✓ 모든 충돌 해결 완료. 커밋하세요:" -ForegroundColor Green
        Write-Host '  git commit -m "fix: resolve merge conflicts"' -ForegroundColor White
    }
    "ours" {
        Show-ConflictStatus
        Resolve-MetaFiles -Strategy "ours"
        Resolve-PackageFiles -Strategy "ours"
        Resolve-ProjectSettings -Strategy "ours"
        $remaining = git diff --name-only --diff-filter=U
        if ($remaining) {
            $remaining | ForEach-Object { git checkout --ours $_; git add $_ }
        }
        Write-Host "`n✓ 모든 충돌 해결 완료. 커밋하세요:" -ForegroundColor Green
        Write-Host '  git commit -m "fix: resolve merge conflicts"' -ForegroundColor White
    }
    "meta-only" {
        Resolve-MetaFiles -Strategy "theirs"
        Write-Host "`n✓ .meta 충돌만 해결 완료" -ForegroundColor Green
    }
    "packages-only" {
        Resolve-PackageFiles -Strategy "theirs"
        Write-Host "`n✓ Packages 충돌만 해결 완료" -ForegroundColor Green
    }
    "settings-only" {
        Resolve-ProjectSettings -Strategy "theirs"
        Write-Host "`n✓ ProjectSettings 충돌만 해결 완료" -ForegroundColor Green
    }
}
