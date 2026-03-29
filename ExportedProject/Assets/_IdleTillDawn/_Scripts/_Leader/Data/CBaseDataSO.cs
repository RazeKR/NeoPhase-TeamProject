using UnityEngine;

/// <summary>
/// 모든 게임 데이터 ScriptableObject의 추상 기반 클래스입니다.
/// CDataManager의 Dictionary 캐싱 키로 사용되는 정수 ID를 제공하며,
/// 이 클래스를 상속하지 않은 SO는 DataManager에 등록될 수 없습니다.
/// </summary>
public abstract class CBaseDataSO : ScriptableObject
{
    #region InspectorVariables

    [Header("─── Base ID ─────────────────────")]
    [SerializeField] private int _id; // 고유 정수 ID - CDataManager Dictionary의 키로 반드시 고유해야 합니다

    #endregion

    #region Properties

    /// <summary>
    /// 데이터의 고유 정수 ID입니다.
    /// CDataManager 초기화 시 Dictionary 키로 사용되며, 같은 타입 내에서 중복되면 안 됩니다.
    /// </summary>
    public int Id => _id;

    #endregion
}
