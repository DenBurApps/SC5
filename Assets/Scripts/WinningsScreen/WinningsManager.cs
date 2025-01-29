using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WinningsManager
{
    private const string SaveKey = "HighWinnings";
    private const int MaxWinsToStore = 40;

    public static event Action<GameType, int> OnNewWin;
    private static List<WinData> _winDatas;

    static WinningsManager()
    {
        LoadWinnings();
    }

    public static void AddWin(GameType gameType, int amount)
    {
 
        if (_winDatas.Any(w => w.GameType == gameType && w.Win == amount))
        {
            return;
        }

        _winDatas.Add(new WinData(gameType, amount));
        _winDatas = _winDatas
            .OrderByDescending(w => w.Win)
            .Take(MaxWinsToStore)
            .ToList();
            
        SaveWinnings();
        OnNewWin?.Invoke(gameType, amount);
    }

    public static List<WinData> GetTopWinnings(int count = MaxWinsToStore)
    {
        return _winDatas
            .OrderByDescending(w => w.Win)
            .Take(count)
            .ToList();
    }

    private static void SaveWinnings()
    {
        string json = JsonUtility.ToJson(new WinningsData { Winnings = _winDatas });
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private static void LoadWinnings()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            var data = JsonUtility.FromJson<WinningsData>(json);
            _winDatas = data.Winnings;
        }
        else
        {
            _winDatas = new List<WinData>();
        }
    }
}

[Serializable]
public class WinData
{
    public GameType GameType;
    public int Win;

    public WinData(GameType gameType, int win)
    {
        GameType = gameType;
        Win = win;
    }
}

[Serializable]
public class WinningsData
{
    public List<WinData> Winnings = new();
}