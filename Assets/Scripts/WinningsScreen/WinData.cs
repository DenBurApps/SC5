namespace WinningsScreen
{
    [System.Serializable]
    public class WinData
    {
        public GameType GameType;
        public int WinAmount;

        public WinData(GameType gameType, int winAmount)
        {
            GameType = gameType;
            WinAmount = winAmount;
        }
    }
}