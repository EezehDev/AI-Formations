# Coordinated Group Movement (AI-Formations)

Research on coordinated movement of AI in groups, and certain formations while navigating through a world. Based on RTS games and various other researches, includes RTS style selections and input. 

The project is made using Unity (version 2020.1.13f1) and C# (version 7.3).

## Goal

Coordinated Group Movement are often used in RTS games, to move a group of units in a pattern fit for the given situation. The main focus of this project is to attempt to recreate this behavior, in my own way and figuring out what the best approach to this would be. Since there are over a hundred different ways to go over formations, I will start with keeping the current formation when selecting a group of units and later look into dealing with obstacles while keeping the formation intact, pre-defined patterns, handling different unit sizes/speeds within the same formation and more. 

To embrace the RTS style, I will also try to simulate some of the input and controls available in these games. A few of these controls being: rectangular (multi-) selection, movement commands, grouping/ungrouping units. This is not part of the actual research, but feels like a perfect fit.

## Project info

**Installation Guide**
1. Download [Unity](https://unity.com/)
2. Install the correct Unity version (2020.1)
3. Download code in [ZIP](https://github.com/MrEezeh/AI-Formations/archive/main.zip)
4. Unpack "AI-Formations-main.zip"
5. Add the "Project" folder in Unity Hub
6. All done, open the project

**Game Controls**
- "WASD", freelook camera
- "LMB", selecting unit (hold for rectangle select)
- "CTRL", add to your current selection
- "RMB", move selection
- "G", group units (limit of four groups)
- "Shift-G", ungroup units

## Implementation

**Game Engine**

Unreal engine would be the best choice, since it has great physics and a well structured engine architecture. However coding C++ in Unreal Engine can be quite tricky and isn't easy to pick up. For this reason Unity is a great alternative with a decent physic simulation and much more beginner friendly, and what I personally used.

**RTS selection**

RTS style selection is implemented using a 3D plane that scales when dragging the mouse cursor around, upon releasing the left mouse button all units within the box collision attached will be selected and can be controlled by the player.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/rts-selection.gif" alt="rts-selection example" width="500" />

Grouping is done by selecting multiple units, and assigning them in a different list or by using a group ID.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/grouping.gif" alt="grouping example" width="500" />

**Navigation**

We will be using A* pathfinding to navigate all units around obstacles in the environment, in combination with a navigation mesh. A* is very flexible algorithm and can be used in many other cases. Using a good navigation algorithm is essential to our coordinated movement, and will play a big part when it comes to grouping up our units, moving the formation around and the way they handling obstacles.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/navigation.gif" alt="navigation example" width="500" />

**Defining a Leader**

The leader is in charge of keeping the group together and will be responsible for coordinating the movement. Where the leader goes, the group will follow it essentially works as the "pivot point / center" of the group. But that is not his only task, in a game environment he must also keep track of some data. Amount of units in the group, current formation and position of each unit, speed of the group, etc.

When it comes to selecting a leader, you have a few options:

**1. Random unit**

Choosing a random unit is a simple solution but comes with some disadvantages. Upon creating a group, one unit will take on the role as leader and hold all the necessary data. All units will follow his movement, this is the main issue. A random unit will never be the same when creating a group, and since all group members follow the leader the movement will never be the same between two similar groups. You can change the position of the leader within the group, but that doesn't quite solve the issue since your leader can not break the formation, often making groups where the leader can not be in the center behave different from the rest.

**2. Virtual unit**

To fix the above issues, we can create a new unit that doesn't have a visual representation. This unit can be placed anywhere in the formation without the player noticing and gives us more control over the group. Most of the time you will want this unit to be in the center, making formations behave as you would expect. In some cases games will make the leader stronger, so to fix this you would still have to assign a random unit the leader role while keeping all the data stored in this virtual unit.

**3. Most important unit**

This type is often the case in video games (at least visually), where one unit will be chosen as leader based on different stats (strongest becomes leader). This comes with the advantage of being able to adapt the formation to protect the leader, making a square around it or forming a line behind them. The same principles apply as with a random unit.

As you can see in the image below, the position of the leader has great influence on how the group moves.

![leader differences](https://github.com/MrEezeh/AI-Formations/blob/main/Images/leaders.jpg)

Using a virtual leader has the most benefits, since it can also assign a random or most important unit to take on the visual role as leader. It is important that we give this leader the functionality to reposition within the group and keep a list of all the units so we can control them through this class.

To decide the spawn position we can either set the leader in the middle of all selected units making sure that all units arrive at roughly the same time, or we can use an average position which makes already close units barely have to move while far away units have to travel towards them.

We will use middle position to spawn the leader, it can be implemented as follows.

```
location minimumPos = (infinity, infinity, infinity);
location maximumPos = (-infinity, -infinity, -infinity);
        
For each unit in selectedUnits
    If unit.position < minimumPos
        Set minimumPos = unit.position;
    Else if unit.position > maximumPos
        Set maximumPos = unit.position;

    middle = (minimum + maximum) / 2;
    
    Set leader.position = middle;
```

Once we have our position of the leader, we need to assign all the units a leader and add them to the list of units.

```
For each unit in selectedUnits
    Add unit to leader.units;
    Set unit.leader = leader;
```

**Formations**

Every group has a formation, usually formations are strategic positions to gain an advantage in a fight due to a stronger offensive or defensive position. This means that the core of a formation needs to remain intact whenever possible. Formations also require some data, such as: rows, columns, units per row/column, location of each unit, etc. To start off easy we can add this formation data to our leader, and set all the locations relative to the leader's position.

As a simple formation, see below code for the implementation of a line formation that scales with amount of units in the group.

```
location currentPosition = (0, 0, 0);
Set currentPosition.x = -unitWidth * (amountUnits / 2) + (unitWidth / 2);

index = 0;
For index < amountUnits
    Spawn point;
    Set point.relativePosition = currentPosition;
    Add point to transformPoints;
    Set currentPosition.x = currentPosition.x + unitWidth;
```

After assigning the positions, we move all the units to their corresponding transformation point.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/line-formation.gif" alt="line-formation example" width="500" />

**Formation movement**

To move a formation, we can again use A* pathfinding to move our virtual leader. It is important to move all transformation points relative to the leader, and changing the orientation to match the movement direction in order to keep it looking natural. We will update our pathfinding every frame to ensure a correct path towards the target while also moving all units back to their point.

```
Every frame
    deltaTime = time since last frame;
    
    If distance(position, targetLocation) > stopDistance
        Calculate path using NavMesh
        Set direction = (path.point - position).normalized;
        Set velocity = direction * speed;
    Else
        Set velocity = (0, 0, 0);

    If velocity != (0, 0, 0)
        Set position = position + (velocity * deltaTime);
        Set atan = Call atan(velocity.y, velocity.x)
        Set rotation = 90 - (atan * degrees);

    Call MoveUnits();
```

This gives us roughly what we are looking to achieve, we now have units following the assigned formation given by their leader and we can move this leader around making the units follow shortly after. There are still a few issues to fix and some additional features we need to implement.

Here is an example of how the current progression looks like:

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/formation-movement.gif" alt="formation-movement example" width="500" />

As you can see, units from the same formation have trouble keeping up with the leader and can bump into eachother since they aren't aware of their adjacent units. Then there is the problem where the formation becomes impossible to complete due to the transformation points being inside obstacles. Currently our group can also split up at random positions if the units can't seem to create a path in that direction, making our formation fall apart.

For now we are using only one semi-hardcoded formation, and it would be a better idea to make the group be able to switch and change formation at runtime.

**Group Cohesion**

**1. Unit speed**

It's a good idea to get the lowest unit speed and use this as a reference for our group speed. Our leader should move at the speed of the slowest unit so our formation never moves faster than the slowest unit alone, if we then set the speed of all units in our group to at least match this speed with a small multiplication our units will be able to catch up to the group. Careful not to set this speed multiplier too high, as it will start feeling unnatural to the player. 

**2. Leader rotation**

We should limit the angular speed or speed at which the leader can rotate based on our group size in order to prevent sharp turns that can break our formation. Using an angular velocity makes navigating a bit more complex since you are no longer allowed to simply move straight to your destinations, but instead need to rotate while walking in that direction.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/group-cohesion.gif" alt="group-cohesion example" width="500" />

## How to improve (not implemented)

**Similar path**

When only using A* and moving the units individually towards a target, they will often split up around obstacles to avoid bumping into eachother. This causes the group to shatter in all directions to reach their goal as soon as possible. When dealing with a group, it often looks better to make them all follow the same path around obstacles. To achieve this units will all need to move towards the same choke point, without blocking eachother. This means units will have to wait for eachother before going to the goal themselves.

![similar path](https://github.com/MrEezeh/AI-Formations/blob/main/Images/choke-point.jpg)

You can take this step even further, by also making units wait and slow down in order to make the full group arrive at roughly the same time.

**Allowing units to pass**

One of the most important and difficult tasks for a group, is to make room for group units to take their position. When for example the most important unit has to move to the middle of the group, you would want the units in it's path to move to the side as he gets closer and shortly after his arrival close back up. For this you need to keep the unit speeds and sizes into account, while all units need to be able to communicate and tell eachother when they need to move to avoid collisions.

**Advanced pathing**

As a last improvement, you can choose to upgrade how your formation deals with pathing in general. This can be things like splitting up the formation at certain points without losing strength, and regrouping them correctly after. By doing this, you allow the group to arrive faster than moving them all towards the same choke point.

Being flexible with the pathfinding algorithm is a good idea when it comes to performance, usually you want to split up your pathfinding into two different methods. One used for long distances to decrease computing time, and one for handling pathing around smaller objects and spaces with more accurate results. Often this is referred as divide-and-conquer, as we split up the issue into smaller parts. [read more on pathfinding](https://github.com/MrEezeh/AI-Formations/#sources)

## Result

## Sources

**Assets**

[Basic Motions FREE Pack - Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/basic-motions-free-pack-154271)

**Similar work**

[Implementing Coordinated Movement - Dave Pottinger](https://www.gamasutra.com/view/feature/3314/coordinated_unit_movement.php?print=1)

[Coordinated Movement - Jeremiah Warm](http://www.jeremiahwarm.com/coordinated-movement.php)

[Group Movement in a RTS game - Marc Latorre](https://marclafr.github.io/Research-Group-Movement-RTS-/)

[RTS Group Movement - Sandra Alvarez](https://sandruski.github.io/RTS-Group-Movement/)

**Useful links**

[Introduction to A* - Amit Patel](http://theory.stanford.edu/~amitp/GameProgramming/AStarComparison.html)

[Performance Evaluation of Pathfinding Algorithms - Harinder Kaur Sidhu](https://scholar.uwindsor.ca/cgi/viewcontent.cgi?article=9230&context=etd)
