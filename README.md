# Coordinated Group Movement (AI-Formations)

Research on coordinated movement of AI in groups, and certain formations while navigating through a world. Based on RTS games and various other researches, includes RTS style selections and input. 

The project is made using Unity (version 2020.1.13f1) and C# (version 7.3).

## Goal

Coordinated Group Movement are often used in RTS games, to move a group of units in a pattern fit for the given situation. The main focus of this project is to attempt to recreate this behavior, in my own way and figuring out what the best approach to this would be. Since there are over a hundred different ways to go over formations, I will start with keeping the current formation when selecting a group of units and later look into dealing with obstacles while keeping the formation intact, pre-defined patterns, handling different unit sizes/speeds within the same formation and more. 

To embrace the RTS style, I will also try to simulate some of the input and controls available in these games. A few of these controls being: rectangular (multi-) selection, movement commands, grouping/ungrouping units. This is not part of the actual research, but feels like a perfect fit.

## Project Info

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

## Implementation Process

**Game Engine**

Initially, I wanted to use Unreal Engine due to the already existing top-down template and engine architecture. Due to limited time and no previous knowledge of C++ in the engine, I ended up switching to Unity. 

Unity has decent physics simulation and a component called nav mesh agent, which should suffice for the goal of this project. Since there was no decent template available, I chose to start with a simple 3D empty project. To have some graphical representation, I added Universal Render Pipeline together with some assets from the Unity Asset Store **(see [sources](https://github.com/MrEezeh/AI-Formations#sources))**.

**RTS Selection**

Usually Top-Down RTS games give you the option to click and drag to select multiple units at once, so this is what I tried to recreate. The easiest way I could think of, was to make a plane with a collision attached that would extend upwards. This way I could scale this plane when holding down and dragging the mouse around, and upon releasing get all the overlapping objects. Some additional input: holding down "CTRL" will add more units to your current select, and pressing "ESC" to clear your selection.

To visualise the selected units, I added a green circle underneath them.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/rts-selection.gif" alt="rts-selection example" width="500" />

**Navigation**

When it comes to navigating in unity, it's fairly straight forward. All I had to do is: attach a Nav Mesh Agent component to my units, configure some variables and build the Navigation Mesh. Once that was done, I can tell the component to move my unit around.

The result isn't too bad for now, but there is definitely more work to do to properly navigate the units in formation.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/navigation.gif" alt="navigation example" width="500" />

**Grouping**

One last thing before starting with the real deal, is to add grouping functionality. To group up the units, I made a simple array to keep track of the index used and set a limit of four groups in total. Then I made some materials to distinguish each of the groups and added the code to group and ungroup selected units. I also made sure selecting one unit from a group selects the full group.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/grouping.gif" alt="grouping example" width="500" />

**Player Data**

In my current model, the player keeps track of some data to make sure we can easily group and ungroup units. I store both units and leaders in seperate lists, since the unit list is only made for undividual units. This way I can tell the units to just simply move to a point, while the leaders will command their own units.

Then I also keep track of which groups are currently taken, together with an array of materials used to change unit color.

**Start of Formations**

Before we can start actually moving one of the groups, it is important to take a look at how this can be done. Let's begin with the most important parts of a group, what essentially makes a group before we can start moving it.

**The Leader**

Starting with the most important role, the leader is in charge of keeping the group together and will be responsible for coordinating the movement. Where the leader goes, the group will follow it essentially works as the "pivot point / center" of the group. But that is not his only task, in a game environment he must also keep track of some data. Amount of units in the group, current formation and position of each unit, speed of the group, etc.

When it comes to selecting a leader, you have a few options:

**1. Random unit**

Choosing a random unit is a simple solution but comes with some disadvantages. Upon creating a group, one unit will take on the role as leader and hold all the necessary data. All units will follow his movement, this is the main issue. A random unit will never be the same when creating a group, and since all group members follow the leader the movement will never be the same between two similar groups. You can change the position of the leader within the group, but that doesn't quite solve the issue since your leader can not break the formation, often making groups where the leader can not be in the center behave different from the rest.

**2. Virtual unit**

To fix the above issues, we can create a new unit that doesn't have a visual representation. This unit can be placed anywhere in the formation without the player noticing and gives us more control over the group. Most of the time you will want this unit to be in the center, making formations behave as you would expect. In some cases games will make the leader stronger, so to fix this you would still have to assign a random unit the leader role while keeping all the data stored in this virtual unit.

**3. Most important unit**

This type is often the case in video games (at least visually), where one unit will be chosen as leader based on different stats (strongest becomes leader). This comes with the advantage of being able to adapt the formation to protect the leader, making a square around it or forming a line behind them. The same principles apply as with a random unit.

As you can see in the image below, the position of the leader has great influence on how the group moves.

![leader differences](https://github.com/MrEezeh/AI-Formations/blob/main/Images/leaders.jpg)

For this research, I will use a virtual unit since it has the most flexibility and is easy to add using prefabs in Unity.

Time to create the leader, for this I created a simple prefab based on the actual unit and changed the script to hold some data that we can later use to access the units and ID of the group, which is used for the material color. I also added a SetTransform method which allows us to reposition the leader when needed.

```
    groupID = -1;
    units = empty;
    
    Function SetTransform(location, rotation)
        Set position = location;
        Set rotation = rotation;
```

To decide the spawn position, I first ungroup all the groups currently selected. This makes sure that we have a free group slot and by writing all the selected units back into a list we can easily loop over them which will be needed later on. In case we only had a single group, I decided the group shouldn't reform but simply add units to the current leaders group. To make this work, I added a boolean that will tell us to destroy the leader or not.

```
        For each selectedLeader in data.selectedLeader
            For each unit in selectedLeader.units
                Add unit to data.selectedUnits;
            
        If (destroyLeader)
            Set data.groups[selectedLeader.groupID] = false;
```

Once this is done, we can loop over our selected units and calculate a position for the new leader. Here we also have a few options, either we set the leader in the middle of all selected units essentially making sure that all units arrive at roughly the same time, or we can use an average position which makes already close units barely have to move while far away units have to travel towards them.

I think calculating the middle of the group makes more sense, since it will make regrouping look more like an actual group being formed. So this is how I ended up implementing the leader spawning. Keep in mind there only needs to be a new leader spawned if we had no current leaders or multiple leaders.

```
        location minimumPos = (infinity, infinity, infinity);
        location maximumPos = (-infinity, -infinity, -infinity);
        
        For each unit in data.selectedUnits
            If (unit.position < minimumPos)
                Set minimumPos = unit.position;
            Else if (unit.position > maximumPos)
                Set maximumPos = unit.position;

        middle = (minimum + maximum) / 2;
        
        If (newLeader)
            Set leader = leader spawned in middle
            Set leader.groupID = freeIndex;
            Set data.groups[freeIndex] = true;
        Else
            Set leader = data.selectedLeaders[0];
            Call leader.SetTransform(middle, zeroRotation);
            Clear leader.units;
```

Then all that is left to do, is assign all the units to the current leader. In case we are using an old leader, there will be some wasted operations here but that's not a big deal as long as the group sizes are relatively small.

```cs
        For each unit in data.selectedUnits
            Add unit to leader.units;
            Call unit.SetLeader(leader);
```

**The Formation**

Every group also has a formation, usually formations are strategic positions to gain an advantage in a fight due to a stronger offensive or defensive position. This means that the core of a formation needs to remain intact whenever possible. Formations also require some data, such as: rows, columns, units per row/column, location of each unit, etc. To start off easy we can add this formation data to our leader, and set all the locations relative to the leader's position.

As a test formation, I implemented a basic line formation that scales with amount of units in the group.

```cs
        location currentPosition = (0, 0, 0);
        Set currentPosition.x = -unitWidth * (amountUnits / 2) + (unitWidth / 2);

        index = 0;
        For (index < amountUnits)
            Spawn point;
            Set point.relativePosition = currentPosition;
            Add point to transformPoints;
            Set currentPosition.x = currentPosition.x + unitWidth;
```

To have the units move to their corresponding point, we can simply tell each of the NavMeshAgent to navigate to the transform positions.

```cs
    index = 0;
    for (index < amountUnits)
        Call units[index].SetTarget(transformPoints[index].position);
```

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/line-formation.gif" alt="line-formation example" width="500" />

So this is basically all it takes to create a formation, but we still can't move around the group. In order to do so, we have to manually update the virtual leader's position using the nav mesh. Using the NavMeshAgent would make our leader collide with the units, and start pushing the group around.

**Formation Movement**

First of all, let's define the data needed to make the pathfinding work in a similar way as the NavMeshAgent. NavMeshAgent component uses a maximum speed and stopping distance to reach the target location along a path. On top of this, let's save the actual target position and our current velocity so we can reuse this data. To make sure we don't get weird behavior, it's also smart to initialise these values at the start.

It should look somewhat like this:

```cs
    speed = 3;
    stopDistance = 0.1;
    path = empty;
    targetLocation = (0, 0, 0);
    velocity = (0, 0, 0);
    
    Function Start()
        Set path = new path;
        Set targetLocation = currentLocation;
```

To update the data and path, we simply change our target and check if the distance is further away than the allowed StopDistance. Once we have a path, we can get the direction towards the next point and move around leader over time. To have some sort of rotation implemented, I used the current velocity and atan2 to get the rotation angle. At the end of the update function, we can then command our units to move back onto the transform points making them follow every movement and rotation the leader makes.

```cs
    Every frame
        deltaTime = time since last frame;
    
        If (distance(position, targetLocation) > stopDistance)
            Calculate path using NavMesh
            Set direction = (path.point - position).normalized;
            Set velocity = direction * speed;
        Else
            Set velocity = (0, 0, 0);

        If (velocity != (0, 0, 0))
            Set position = position + (velocity * deltaTime);
            Set atan = Call atan(velocity.y, velocity.x)
            Set rotation = 90 - (atan * degrees);

        Call MoveUnits();
    }
```

Now that this is implemented we're starting to see the results of moving in group, but it is far from perfect. There is currently only one formation that has no limitations, rotation is instant and makes the group collide with eachother while trying to reach the transform points. The units also don't look like a group when they need to get past obstacles, they either split up or walk straight into the walls.

Overall I'm quite happy with the progress so far.

<img src="https://github.com/MrEezeh/AI-Formations/blob/main/Gifs/formation-movement.gif" alt="formation-movement example" width="500" />

## Functionality

**Grouping**

**Formations**

**Steering**

**Pathfinding**

**Extra**

## Result

*Video/Gif*

## Sources

**Assets**

[Basic Motions FREE Pack - Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/basic-motions-free-pack-154271)

**Similar Work**

[Implementing Coordinated Movement - Dave Pottinger](https://www.gamasutra.com/view/feature/3314/coordinated_unit_movement.php?print=1)

[Coordinated Movement - Jeremiah Warm](http://www.jeremiahwarm.com/coordinated-movement.php)

[Group Movement in a RTS game - Marc Latorre](https://marclafr.github.io/Research-Group-Movement-RTS-/)

[RTS Group Movement - Sandra Alvarez](https://sandruski.github.io/RTS-Group-Movement/)

**Useful Links**

[Introduction to A* - Amit Patel](http://theory.stanford.edu/~amitp/GameProgramming/AStarComparison.html)
