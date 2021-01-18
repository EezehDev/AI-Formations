# AI-Formations (WIP, defining the scope of project)

AI Steering behaviors, while keeping a certain formation. Also includes some variations and group functionality.

The project is implemented using Unity (version 2020.1.13f1).

## Goal

Formations are often used in RTS games, to move a group of units in a pattern fit for the given situation. The main focus of this project is to attempt to recreate this behavior, in my own way and figuring out what the best approach to this would be. Since there are over a hundred different ways to go over formations, I will start with keeping the current formation when selecting a group of units and later look into dealing with obstacles while keeping the formation intact, pre-defined patterns, different unit speeds within the same formation and more. 

To embrace the RTS style, I will also try to simulate some of the input and controls available in these games. A few of these controls being: rectangular (multi-) selection, movement commands, grouping/ungrouping units and more. This is not part of the actual research, but felt necessary to test out the project functionality.

## Project info

**Controls**
- "WASD", freelook camera
- "LMB", selecting unit (hold for rectangle select)
- "RMB", move unit(s)
- "G", group units
- "Shift-G", ungroup units

You start with a few units grouped up, you can choose to either ungroup the units or create new groups.
There is also functionality to add and remove units from the group, which will update the current formation.

Using the UI you can choose to move the units in formation or stack.
Grouped units will always move together with their corresponding group (moving a unit will move all units in the same group), while trying to navigate in group around the environment including the obstacles in place.

## Implementation

**Choosing the engine / framework**

Initially, I wanted to use Unreal Engine due to the already existing top-down template and engine architecture. Due to limited time and no previous knowledge of C++ in the engine, I ended up switching to Unity. 

Unity has decent physics simulation and a component called nav mesh agent, which should suffice for the goal of this project. Since there was no decent template available, I chose to start with a simple 3D empty project. To have some graphical representation, I added Universal Render Pipeline together with some assets from the Unity Asset Store **(see sources)**.

**RTS Selection**

Usually Top-Down RTS games give you the option to click and drag to select multiple units at once, so this is what I tried to recreate. The easiest way I could think of, was to make a plane with a collision attached that would extend upwards. This way I could scale this plane when holding down and dragging the mouse around, and upon releasing get all the overlapping objects.

I also added some small additional features: holding down "CTRL" to add more units to your current select, and pressing "ESC" to clear your selection. To visualise the selected units, I changed the material color upon selection.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/rts-selection.gif" width="500" />

**Navigation**

When it comes to navigating in unity, it's fairly straight forward. All I had to do is: attach a Nav Mesh Agent component to my units, configure some variables and build the Navigation Mesh. Once that was done, I can tell the component to move my unit around.

This doesn't look too bad for now, but there is definitely more work to do to properly navigate the units in formation.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/navigation.gif" width="500" />

## Functionality

**Grouping**

**Formations**

**Steering**

**Pathfinding**

**Extra**

## Result

*Video/Gif*

## Sources

**Game Engine**

[Unity](https://unity.com/)

**Assets**

[Basic Motions FREE Pack - Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/basic-motions-free-pack-154271)

**Research**

[Coordinated Movement - Jeremiah Warm](http://www.jeremiahwarm.com/coordinated-movement.php)
