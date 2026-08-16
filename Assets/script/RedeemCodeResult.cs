using UnityEngine;

namespace CrimeGame
{
    /// <summary>
    /// Tracks whether the player has ever redeemed a valid code. Backed by
    /// PlayerPrefs so it survives not just scene loads but full game restarts --
    /// call Load() once (MainMenuUI does this on Start) to pull the saved state
    /// into these fields before reading them.
    /// </summary>
    public static class RedeemCodeResult
    {
        private const string HasRedeemedKey = "HasRedeemedCode";
        private const string RedeemedCodeKey = "RedeemedCode";

        public static bool HasRedeemed { get; private set; }
        public static string RedeemedCode { get; private set; }

        /// <summary>Pulls the saved redeem state from PlayerPrefs into memory.</summary>
        public static void Load()
        {
            HasRedeemed = PlayerPrefs.GetInt(HasRedeemedKey, 0) == 1;
            RedeemedCode = PlayerPrefs.GetString(RedeemedCodeKey, null);
        }

        public static void Set(string code)
        {
            RedeemedCode = code;
            HasRedeemed = true;

            PlayerPrefs.SetInt(HasRedeemedKey, 1);
            PlayerPrefs.SetString(RedeemedCodeKey, code);
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            HasRedeemed = false;
            RedeemedCode = null;

            PlayerPrefs.DeleteKey(HasRedeemedKey);
            PlayerPrefs.DeleteKey(RedeemedCodeKey);
        }
    }
}