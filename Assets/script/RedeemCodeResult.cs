namespace CrimeGame
{
    /// <summary>
    /// Plain static class holding whether the player redeemed a valid code, and
    /// which one. Static fields survive scene loads, so any later scene can
    /// check RedeemCodeResult.HasRedeemed without needing a persistent GameObject.
    /// </summary>
    public static class RedeemCodeResult
    {
        public static bool HasRedeemed;
        public static string RedeemedCode;

        public static void Set(string code)
        {
            RedeemedCode = code;
            HasRedeemed = true;
        }

        public static void Clear()
        {
            HasRedeemed = false;
            RedeemedCode = null;
        }
    }
}
