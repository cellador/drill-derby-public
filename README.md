# Drill Derby 

A 2-4 player couch-multiplayer action game. Crash into your opponents and be the last one to survive. Play with multiple people on a single keyboard, using Xbox 360 gamepads or any combination thereof.

![in-game screenshots of menu and gameplay scenes of drill derby (moritzkirschner.com)](/montage.jpg?raw=true "Drill Derby Screenshots (moritzkirschner.com)")

# Overview

This is a game prototype developed by Moritz Kirschner, written in C# using the Unity Engine. The build on display is considered feature complete with local multiplayer, a menu for mapping input and a rudimentary gameplay loop. Currently, I'm implementing networked multiplayer functionality before expanding the game in any other way.

### I don't want to read code for an hour, tell me about the cool stuff.

**About 2.5k lines of code**

**Character Input**

Input is abstracted using the command pattern (See [CharacterInput.cs](/Assets/Scripts/Input/CharacterInput.cs)). This lays the foundation for local (and later networked) multiplayer.
  
**Input Mapping**

[InputMappingCursor.cs](/Assets/Scripts/UI/InputMappingCursor.cs) handles the mapping of player actions in the main menu lobby. It assigns player actions to any input from the keyboard and any Xbox 360 gamepads.

**Cold Start**

The game can be cold-started from a game scene as long as the scene contains a [WorldManager](/Assets/Scripts/Gameplay/WorldManager.cs). We call the singletons [InputManager](/Assets/Scripts/Input/InputManager.cs) and [GameManager](/Assets/Scripts/Gameplay/GameManager.cs) which come with a default configuration to quickly test gameplay without going through the menu.

**Character Controller**

The heart of this game is the [DrillCharacterController](/Assets/Scripts/Gameplay/DrillCharacterController.cs) which inherits from [PhysicsObject](/Assets/Scripts/Physics/PhysicsObject.cs). PhysicsObject implements realistic forces such as gravity and drag on objects that move in the game world (the result of which I refer to as extrinsic velocity). However, for an arcade-like game like this, controlling your player through realistic forces only often results in floaty gameplay. Therefore, PhysicsObject also implements a virtual function CustomMovement which is called once per physics update. This can be used to set what I call "intrinsic velocity" and result in instantaneous/erratic movement. DrillCharacterController makes ample use of this and the result is snappy movement which still feels grounded in reality.

Other details such as freeze-frames and particle movements on hit are also implemented there to add to satisfying game feel.

# Build

1. Install Unity 2019.4.21f1 LTS
2. Create new empty project & copy repository content into project root
3. Open via either scene in `Assets/Scenes` and build through Unity
