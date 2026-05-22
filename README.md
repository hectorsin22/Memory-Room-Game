# MEMORY ROOM

## About the Project

MEMORY ROOM is a memory and reconstruction game built around physical movement inside a shared room. Players are shown a set of objects arranged in the space, have to **memorize what appeared and where**, and then **rebuild that arrangement** by walking to the objects and picking them up — without controllers, just by moving and crouching in real space.

The whole experience is designed for multiple players at once and is meant to be played by physically walking around, not with a headset or a gamepad. (Technically it runs as a projected, body-tracked installation, but the focus of the project is the game itself and how it plays.)

## The Game

### Game flow

The game runs as a continuous loop of rounds, managed by a state machine with four phases:

**Menu → Memorize → Reconstruction → Round Finished → (next round)**

1. **Menu** — Players start in a dimly lit room. There is no UI to click: navigation is physical. Walking into a marked floor zone starts the game; another zone quits it.
2. **Memorize** — A set of objects appears at fixed positions around the room. Players have a limited time to memorize *which* objects appeared and *where* each one was.
3. **Reconstruction** — The objects vanish, the chest opens, and the matching objects become available to grab. Players must walk to the correct objects and place/carry them to rebuild the original arrangement from memory.
4. **Round Finished** — Short pause, the difficulty steps up, and the next round begins.

### Difficulty progression

Each round is harder than the last. The game scales several things automatically as rounds advance:

- **More objects to remember** — starts small and grows round by round, up to a cap.
- **Tighter timing** — memorization and reconstruction windows change with the round, balancing pressure against fairness.
- **Impostor objects** — from round 6 onward, extra "wrong" objects are mixed into the available pool, so players can no longer just grab everything; they have to remember which objects actually belonged to the arrangement. The number of impostors grows with the round, up to a cap.

This progression is what turns a simple "look and remember" exercise into an escalating memory challenge.

### How players interact

Interaction is entirely physical and based on the player's body:

- **Pick up** — crouch down while standing over an object. The object is then carried with the player.
- **Drop** — stand back up and crouch again to release it.
- Only **one object can be carried at a time**, and there's a short cooldown after dropping so objects aren't grabbed again by accident.

This crouch-to-grab / crouch-to-drop design is the core of the gameplay feel — it makes picking things up a deliberate, physical act rather than a button press.

### Objects, spawns and the chest

- Every object has a unique identity, so the game can tell apart the version shown during memorization, the version players grab during reconstruction, and the spawn point where it originally appeared.
- Memory objects spawn at fixed points and are aligned to the ground so they always sit naturally in the room.
- The **chest** acts as a visual cue for the game state: closed during memorization, opening when the reconstruction phase begins (with supporting sound).

## Current State of Development

The game is a working, playable prototype. The full gameplay structure is already in place:

- ✅ Complete round loop (menu, memorize, reconstruct, round transitions)
- ✅ Physical, crouch-based pickup and drop
- ✅ Multi-player support — several people can play in the same room at once
- ✅ Automatic difficulty scaling (object count, timing)
- ✅ Impostor objects from round 6 to increase challenge
- ✅ Physical menu navigation by walking into zones
- ✅ Chest open/close animation and sound cues
- ✅ Object identity / spawn-tracking system

### What's being worked on

- Tuning the **difficulty curve** (how fast objects, impostors and timers ramp up) so the challenge feels fair.
- Polishing the **interaction feel** of picking up and dropping.
- Adding more **content** (objects and arrangements) and refining the round-to-round experience.

> A temporary keyboard control (**E** to drop) exists only for testing in the editor and will be removed; the real game is played entirely through movement.

## Final Goal

The final objective of MEMORY ROOM is a polished, multi-player memory game driven entirely by natural movement: progressive difficulty, richer and trickier memory challenges, and fully physical, controller-free interaction from start to finish.


