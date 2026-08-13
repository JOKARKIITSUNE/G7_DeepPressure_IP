namespace MiniGames
{
    /// <summary>
    /// Plain static class (not a MonoBehaviour) that holds the outcome of the
    /// last shadow box match. Static fields survive a scene load, so this is
    /// the easiest way to carry the result from the minigame scene into
    /// whatever comes next -- read ShadowBoxResult.Winner from any script in
    /// the next scene's Start() to react to it (unlock a door, change dialogue,
    /// grant a reward, etc).
    /// </summary>
    public static class ShadowBoxResult
    {
        public static bool HasResult;
        public static Actor Winner;

        public static void Set(Actor winner)
        {
            Winner = winner;
            HasResult = true;
        }

        public static void Clear()
        {
            HasResult = false;
        }
    }
}
