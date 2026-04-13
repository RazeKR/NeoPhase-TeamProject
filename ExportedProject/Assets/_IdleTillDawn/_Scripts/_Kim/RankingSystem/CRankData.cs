using System;
using System.Collections.Generic;

[Serializable]
public class CRankData
{
    public string uid;
    public string nickname;
    public int highestStageIdx;
    public EPlayerType characterType;
    public int totalKills;
    public int playerLevel = 1; // 서버에 필드가 없는 구버전 항목도 최소 1로 표시

    /// <summary>
    /// 세이브 데이터를 서버에 저장 시킬 랭킹 데이터로 변환하는 메서드
    /// </summary>
    /// <param name="localData">로컬 세이브 데이터</param>
    /// <returns></returns>
    public static CRankData FromSaveDataToRankData(CSaveData localData)
    {
        return new CRankData
        {
            uid             = localData.uid,
            nickname        = localData.nickname,
            highestStageIdx = localData.highestStageId,
            characterType   = localData.characterType,
            totalKills      = localData.totalKills,
            playerLevel     = localData.playerLevel
        };
    }
}

[Serializable]
public class RankDataList
{
    public List<CRankData> rankings;
}

[Serializable]
public class GASResponse
{
    public string status;
    public string message;
}
