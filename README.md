# AI-Formations (WIP, defining the scope of project)

AI Steering behaviors, while keeping a certain formation. Also includes some variations and group functionality.

The project is implemented using Unity (version 2020.1.13f1).

## Goal

Formations are often used in RTS games, to move a group of units without making them vulnerable to certain attack patterns. The main focus of this project is to attempt to recreate a user friendly implementation of RTS style formations, where you can: navigate through a world with units in group or solo, group and ungroup them all while maintaining their group relations and formations based on input.

**Extra** - Unit movement using pre-defined formations

## Project info

**Controls**
- **WASD**, freelook camera
- **LMB**, selecting unit (hold for rectangle select)
- **RMB**, move unit(s)
- **G**, group units
- **Shift-G**, ungroup units

You start with a few units grouped up, you can choose to either ungroup the units or create new groups.
There is also functionality to add and remove units from the group, which will update the current formation.

Using the UI you can choose to move the units in formation or stack.
Grouped units will always move together with their corresponding group (moving 1 unit will move all units in the same group), while trying to navigate in group around the environment including the obstacles in place.

## Implementation

**Choosing the engine / framework**
Initially, I wanted to use Unreal Engine due to the already existing top-down template and engine architecture. Due to limited time and no previous knowledge of C++ in the engine, I ended up switching to Unity. Unity has decent physics simulation and should suffice for the goal of this project. Since there was no decent template available, I chose to start with a simple 3D empty project. To have some graphical representation, I added Universal Render Pipeline together with some assets from the Unity Asset Store (see sources).

## Functionality

**Grouping**

**Formations**

**Steering**

**Pathfinding**

**Extra**

## Result

*Video/Gif*

## Sources

Name - *URL*
