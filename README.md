# G7_DeepPressure_IP

# Deep Pressure

Deep Pressure is a third-person crime-prevention story game made in Unity 6. The player is influenced by their friend Jaiden and must decide whether to participate in theft and vandalism or resist the pressure through a minigame.

The two decisions lead to a Good, Neutral, or Bad ending.

## Controls

| Input | Action |
| --- | --- |
| W, A, S, D | Move |
| Mouse | Control the camera |
| Left Shift | Sprint |
| Space | Jump |
| E | Start an interaction or perform the current task |
| Mouse click | Continue dialogue and select Yes or No |
| WASD or Arrow keys | Select a direction in the ShadowBox minigame |
| Escape | Open or close the pause menu |

## Gameplay

1. Talk to Jaiden in the classroom.
2. Follow him to the Foodclub and decide whether to steal an unattended bag.
3. Go to the Void Deck and decide whether to kick over a dustbin.
4. Talk to the police and receive an ending based on both decisions.

Choosing No starts the ShadowBox resistance minigame. If the player loses, Jaiden responds and the player can retry. Winning allows the player to resist the crime.

## ShadowBox Rules

- The roles are Pointer and Looker.
- Choose Up, Down, Left, or Right.
- If both directions match, the Pointer strikes the Looker.
- If the directions do not match, the roles swap.
- The first participant to receive three strikes loses.

There is no fixed puzzle answer because the bot chooses directions dynamically.

## Story Flow

```text
Main Menu
  -> Classroom
  -> Foodclub decision
       -> Yes: Steal bag
       -> No: ShadowBox minigame
  -> Void Deck decision
       -> Yes: Kick dustbin
       -> No: ShadowBox minigame
  -> Talk to Police
       -> 0 crimes: Good Ending
       -> 1 crime: Neutral Ending
       -> 2 crimes: Bad Ending
```

## Known Limitations

- Windows keyboard and mouse controls only.
- No save or checkpoint system.
- The interface is designed mainly for a 16:9 display.
- Some sounds are temporary placeholder effects.
- Minigame difficulty and duration vary because the bot includes randomness.

## Credits

- Game design, story integration, scene assembly, scripting, and UI: G7 project team
- Unity 6 and Universal Render Pipeline: Unity Technologies
- Unity Starter Assets Third Person Controller: Unity Technologies
- Unity Input System, Cinemachine, UGUI, and TextMesh Pro: Unity Technologies
- LowPolyAssetPack_Free: CC0 1.0 Universal
- Characters PSX, classroom environment, and Chair with Bag: external project assets
- Liberation Sans: Google Corporation and Red Hat, Inc., SIL Open Font License 1.1

External assets remain subject to their original licences. Placeholder audio should be replaced or verified before public distributi
