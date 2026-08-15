namespace CrimeGame
{
    public enum EndingType
    {
        BothCrimes,   // friend snitches, pins everything on player, friend walks free
        OneCrime,     // both player and friend arrested
        NoCrimes      // only friend arrested, player free -> QR code + quiz
    }

    /// <summary>
    /// Static class tracking whether the player ended up committing each crime
    /// (whether by choosing Yes, or by losing the resistance minigame after
    /// choosing No). Survives scene loads since it's plain static data.
    /// </summary>
    public static class CrimeTracker
    {
        public static bool DidSteal { get; private set; }
        public static bool DidVandalize { get; private set; }

        public static void MarkStole() => DidSteal = true;
        public static void MarkVandalized() => DidVandalize = true;

        public static EndingType GetEnding()
        {
            int count = (DidSteal ? 1 : 0) + (DidVandalize ? 1 : 0);
            if (count >= 2) return EndingType.BothCrimes;
            if (count == 1) return EndingType.OneCrime;
            return EndingType.NoCrimes;
        }

        public static void Reset()
        {
            DidSteal = false;
            DidVandalize = false;
        }
    }
}
