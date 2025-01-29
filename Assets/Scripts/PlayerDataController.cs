using System;
using System.IO;
using UnityEngine;

public enum GameType
{
    BambooFortune,
    SpinFestival,
    ChinaSlots,
    JungleMystique
}

public static class PlayerDataController
{
    private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "PlayerBalance");
    private const int InitialBalance = 1000;

    public static int FreeSpinsCount { get; private set; }
    public static int CurrentBalance { get; private set; }
    public static bool IsBambooFortunePurchased { get; private set; }
    public static bool IsSpinFestivalPurchased { get; private set; }

    public static event Action<int> OnBalanceChanged;
    public static event Action<int> OnFreeSpinsChanged;
    public static event Action<GameType, bool> OnGamePurchaseStatusChanged;

    static PlayerDataController()
    {
        FreeSpinsCount = 0;
        LoadBalanceData();
    }

    public static void IncreaseBalance(int amount)
    {
        if (FreeSpinsCount > 0)
        {
            FreeSpinsCount--;
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

    public static void PurchaseGame(GameType gameType, int price)
    {
        if (CurrentBalance < price)
        {
            Debug.Log($"Not enough balance to purchase {gameType}");
            return;
        }

        bool purchaseSuccess = false;
        switch (gameType)
        {
            case GameType.BambooFortune:
                if (!IsBambooFortunePurchased)
                {
                    IsBambooFortunePurchased = true;
                    purchaseSuccess = true;
                }
                break;
            case GameType.SpinFestival:
                if (!IsSpinFestivalPurchased)
                {
                    IsSpinFestivalPurchased = true;
                    purchaseSuccess = true;
                }
                break;
        }

        if (purchaseSuccess)
        {
            DecreaseBalance(price);
            OnGamePurchaseStatusChanged?.Invoke(gameType, true);
            SaveBalanceData();
        }
    }

    public static bool IsGamePurchased(GameType gameType)
    {
        return gameType switch
        {
            GameType.BambooFortune => IsBambooFortunePurchased,
            GameType.SpinFestival => IsSpinFestivalPurchased,
            _ => false
        };
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
            var data = JsonUtility.FromJson<PlayerData>(json);

            CurrentBalance = data.Balance;
            FreeSpinsCount = data.FreeSpinsCount;
            IsBambooFortunePurchased = data.IsBambooFortunePurchased;
            IsSpinFestivalPurchased = data.IsSpinFestivalPurchased;
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
            var data = new PlayerData(
                CurrentBalance,
                FreeSpinsCount,
                IsBambooFortunePurchased,
                IsSpinFestivalPurchased
            );
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
        IsBambooFortunePurchased = false;
        IsSpinFestivalPurchased = false;
        SaveBalanceData();
    }
}