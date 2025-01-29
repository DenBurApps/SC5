using System;

[Serializable]
public class PlayerData
{
    public int Balance;
    public int FreeSpinsCount;
    public bool IsBambooFortunePurchased;
    public bool IsSpinFestivalPurchased;

    public PlayerData(int balance, int freeSpinsCount, bool isBambooFortunePurchased, bool isSpinFestivalPurchased)
    {
        Balance = balance;
        FreeSpinsCount = freeSpinsCount;
        IsBambooFortunePurchased = isBambooFortunePurchased;
        IsSpinFestivalPurchased = isSpinFestivalPurchased;
    }
}