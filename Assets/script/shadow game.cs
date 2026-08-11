using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGames
{
    public enum PunchDirection { None, Up, Down, Left, Right }
    public enum Actor { Player, Bot }

    /// <summary>
    /// Shadow-box style minigame. There is no timer: the round simply waits until
    /// the player presses a direction, and that input instantly triggers the bot's
    /// "simultaneous" choice and resolves the round. Whoever currently holds the
    /// Pointer role lands a strike against the other on a match; on a mismatch,
    /// roles flip. Strikes accumulate across the whole match -- the first actor to
    /// take 3 strikes is out and loses.
    ///
    /// This script contains no UI rendering by design -- see ShadowBoxUI for the
    /// 2D canvas front-end that hooks into these events.
    /// </summary>
    public class ShadowBoxGame : MonoBehaviour
    {
        [Header("Pacing")]
        [Tooltip("Pause after a round resolves before the next input is accepted. Purely cosmetic -- not a reaction timer.")]
        public float postRoundDelay = 0.5f;

        [Header("Rules")]
        [Tooltip("Number of strikes taken before an actor is out")]
        public int strikesToLose = 3;

        [Header("Bot difficulty")]
        [Range(0f, 1f)]
        [Tooltip("Chance the bot uses its prediction instead of a random guess")]
        public float botSkill = 0.55f;

        // --- Public state, read-only from outside ---
        public Actor CurrentPointer { get; private set; } = Actor.Player;
        public int PlayerStrikes { get; private set; }
        public int BotStrikes { get; private set; }
        public int RoundNumber { get; private set; }
        public bool GameOver { get; private set; }
        public bool WaitingForInput { get; private set; }

        // --- Events for UI scripts to hook into ---
        public event Action OnRoundStarted;           // now waiting for the player's input
        public event Action<PunchDirection, PunchDirection, bool> OnRoundResolved; // playerDir, botDir, wasHit
        public event Action<Actor> OnRoleFlipped;       // actor is the new Pointer
        public event Action<Actor> OnStrikeTaken;       // actor who was struck
        public event Action<Actor> OnGameWon;           // actor who won (opponent hit the strike limit)

        private readonly List<PunchDirection> _playerHistory = new List<PunchDirection>();
        private PunchDirection _capturedInput;
        private bool _acceptingInput;
        private Coroutine _loop;

        public void StartGame()
        {
            StopGame();
            CurrentPointer = Actor.Player;
            PlayerStrikes = 0;
            BotStrikes = 0;
            RoundNumber = 0;
            GameOver = false;
            _playerHistory.Clear();
            _loop = StartCoroutine(GameLoop());
        }

        public void StopGame()
        {
            if (_loop != null) StopCoroutine(_loop);
            _acceptingInput = false;
            WaitingForInput = false;
        }

        private void Update()
        {
            if (!_acceptingInput || Keyboard.current == null) return;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                CaptureInput(PunchDirection.Up);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                CaptureInput(PunchDirection.Down);
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                CaptureInput(PunchDirection.Left);
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                CaptureInput(PunchDirection.Right);
        }

        /// <summary>
        /// Call this from a UI button's OnClick (see ShadowBoxUI's direction
        /// buttons) if you want on-screen direction buttons in addition to
        /// keyboard input. This is what actually starts/resolves each round --
        /// there is no timer or window, the input itself is the trigger.
        /// </summary>
        public void CaptureInput(PunchDirection dir)
        {
            if (!_acceptingInput) return;
            _capturedInput = dir;
            _acceptingInput = false;
        }

        private IEnumerator GameLoop()
        {
            while (!GameOver)
            {
                yield return RunRound();
            }
        }

        private IEnumerator RunRound()
        {
            RoundNumber++;
            _capturedInput = PunchDirection.None;
            _acceptingInput = true;
            WaitingForInput = true;
            OnRoundStarted?.Invoke();

            // No timer -- just wait until CaptureInput() flips this to false.
            yield return new WaitUntil(() => !_acceptingInput);
            WaitingForInput = false;

            PunchDirection playerDir = _capturedInput;
            PunchDirection botDir = ChooseBotDirection();

            if (playerDir != PunchDirection.None) _playerHistory.Add(playerDir);

            bool isHit = playerDir != PunchDirection.None && playerDir == botDir;

            if (isHit)
            {
                // The current Pointer lands the strike against the current Looker.
                Actor struckActor = CurrentPointer == Actor.Player ? Actor.Bot : Actor.Player;
                if (struckActor == Actor.Player) PlayerStrikes++;
                else BotStrikes++;
                OnStrikeTaken?.Invoke(struckActor);
            }
            else
            {
                CurrentPointer = CurrentPointer == Actor.Player ? Actor.Bot : Actor.Player;
                OnRoleFlipped?.Invoke(CurrentPointer);
            }

            OnRoundResolved?.Invoke(playerDir, botDir, isHit);

            if (PlayerStrikes >= strikesToLose || BotStrikes >= strikesToLose)
            {
                GameOver = true;
                OnGameWon?.Invoke(PlayerStrikes >= strikesToLose ? Actor.Bot : Actor.Player);
                yield break;
            }

            yield return new WaitForSeconds(postRoundDelay);
        }

        private static readonly PunchDirection[] AllDirections =
        {
            PunchDirection.Up, PunchDirection.Down, PunchDirection.Left, PunchDirection.Right
        };

        private PunchDirection ChooseBotDirection()
        {
            PunchDirection prediction = PredictPlayerDirection();

            bool botIsPointer = CurrentPointer == Actor.Bot;
            bool useSkill = UnityEngine.Random.value < botSkill;

            if (botIsPointer)
            {
                return useSkill ? prediction : RandomDirection();
            }
            else
            {
                if (!useSkill) return RandomDirection();

                PunchDirection choice;
                do { choice = RandomDirection(); } while (choice == prediction);
                return choice;
            }
        }

        private PunchDirection PredictPlayerDirection()
        {
            if (_playerHistory.Count == 0) return RandomDirection();

            var counts = new Dictionary<PunchDirection, int>();
            int lookback = Mathf.Min(6, _playerHistory.Count);
            for (int i = _playerHistory.Count - lookback; i < _playerHistory.Count; i++)
            {
                var d = _playerHistory[i];
                counts[d] = counts.TryGetValue(d, out var c) ? c + 1 : 1;
            }

            PunchDirection best = AllDirections[0];
            int bestCount = -1;
            foreach (var dir in AllDirections)
            {
                int c = counts.TryGetValue(dir, out var v) ? v : 0;
                if (c > bestCount) { bestCount = c; best = dir; }
            }
            return best;
        }

        private static PunchDirection RandomDirection()
        {
            return AllDirections[UnityEngine.Random.Range(0, AllDirections.Length)];
        }
    }
}