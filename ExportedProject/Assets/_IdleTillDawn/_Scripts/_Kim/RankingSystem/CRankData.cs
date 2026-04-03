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

    /// <summary>
    /// 세이브 데이터를 서버에 저장 시킬 랭킹 데이터로 변환하는 메서드
    /// </summary>
    /// <param name="localData">로컬 세이브 데이터</param>
    /// <returns></returns>
    public static CRankData FromSaveDataToRankData(CSaveData localData)
    {
        return new CRankData
        {
            uid = localData.uid,
            nickname = localData.nickname,
            highestStageIdx = localData.highestStageId,
            characterType = localData.characterType,
            totalKills = localData.totalKills
        };
    }
}

[Serializable]
public class RankDataList
{
    public List<CRankData> rankings;
}
