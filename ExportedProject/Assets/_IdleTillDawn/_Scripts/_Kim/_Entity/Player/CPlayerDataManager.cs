using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CPlayerDataManager : MonoBehaviour
{
    #region 내부 변수
    public static CPlayerDataManager Instance { get; private set; }

    private CPlayerSaveData _currentData;
    private string _savePath;
    private const string Key = "ITD_Encryption_Key";
    #endregion

    #region 프로퍼티
    public CPlayerSaveData CurrentData => _currentData;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        _savePath = Path.Combine(Application.persistentDataPath, "player_save.dat");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void SavePlayerData(CPlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(_savePath, EncryptDecrypt(json));
    }

    public void LoadPlayerData()
    {
        if (File.Exists(_savePath))
        {
            string encrypted = File.ReadAllText(_savePath);
            string json = EncryptDecrypt(encrypted);

            _currentData = JsonUtility.FromJson<CPlayerSaveData>(json);
        }
        else
        {
            CreateNewPlayer("이름 없는 플레이어", EPlayerType.Dasher);
        }
    }

    public void CreateNewPlayer(string nickName, EPlayerType type)
    {
        string newUid = GenerateNumericUID();
        _currentData = new CPlayerSaveData(newUid, nickName, type);
        SavePlayerData(_currentData);
    }

    /// <summary>
    /// 숫자 기반 UID 생성
    /// </summary>
    /// <returns>8 + 날짜 기반 6자리 + 랜덤 3자리</returns>
    private string GenerateNumericUID() => $"8{DateTime.Now:yyMMdd}{UnityEngine.Random.Range(100, 1000)}";

    private string EncryptDecrypt(string text)
    {
        StringBuilder res = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            res.Append((char)(text[i] ^ Key[i % Key.Length]));
        }

        return res.ToString();
    }
}
