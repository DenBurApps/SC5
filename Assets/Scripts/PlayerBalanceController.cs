using System;
using System.IO;
using UnityEngine;

public static class PlayerBalanceController
{
    private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "PlayerBalance");
    private const int InitialBalance = 1000;

    public static int FreeSpinsCount { get; private set; }
    public static int CurrentBalance { get; private set; }

    public static event Action<int> OnBalanceChanged;
    public static event Action<int> OnFreeSpinsChanged;

    static PlayerBalanceController()
    {
        FreeSpinsCount = 0;
        LoadBalanceData();
    }

    public static void IncreaseBalance(int amount)
    {
        if (FreeSpinsCount > 0)
        {
            FreeSpinsCount--;
            amount = 50;
            OnFreeSpinsChanged?.Invoke(FreeSpinsCount);
        }

        CurrentBalance += amount;
        SaveBalanceData();
        OnBalanceChanged?.Invoke(CurrentBalance);
    }

    public static void DecreaseBalance(int amount)
    {
        if (FreeSpinsCount > 0)
        {
            FreeSpinsCount--;
            OnFreeSpinsChanged?.Invoke(FreeSpinsCount);
        }
        else if (CurrentBalance >= amount)
        {
            CurrentBalance -= amount;
            OnBalanceChanged?.Invoke(CurrentBalance);
        }
        else
        {
            Debug.Log("Not enough balance");
        }

        SaveBalanceData();
    }

    public static void AddFreeSpins(int spins)
    {
        FreeSpinsCount += spins;
        OnFreeSpinsChanged?.Invoke(FreeSpinsCount);
        SaveBalanceData();
    }

    private static void LoadBalanceData()
    {
        if (!File.Exists(SaveFilePath))
        {
            ResetToDefault();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<PlayerBalanceData>(json);

            CurrentBalance = data.Balance;
            FreeSpinsCount = data.FreeSpinsCount;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load balance: " + e.Message);
            ResetToDefault();
        }
    }

    private static void SaveBalanceData()
    {
        try
        {
            var data = new PlayerBalanceData(CurrentBalance, FreeSpinsCount);
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save balance: " + e.Message);
        }
    }

    private static void ResetToDefault()
    {
        CurrentBalance = InitialBalance;
        FreeSpinsCount = 0;
        SaveBalanceData();
    }
}

[Serializable]
public class PlayerBalanceData
{
    public int Balance;
    public int FreeSpinsCount;

    public PlayerBalanceData(int balance, int freeSpinsCount)
    {
        Balance = balance;
        FreeSpinsCount = freeSpinsCount;
    }
}