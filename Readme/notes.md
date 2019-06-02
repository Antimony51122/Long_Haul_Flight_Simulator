
## Note 3



**6:43**

agent could head towards its target completely and enter dead end
- completely unobstructed for quite some time, only to find much later along its value that is navigated down a dead end and target unreachable
- in order to head to the target, the agent might heading away from its goal initially to get out from the dead-end to start moving towards its goal






### Local Avoidance System **21:35**


A local avoidance system is employed to push dynamic objects away from each other during agent steering

in Unity 5, you can alter the navigation mesh at run-time by carving objects into it. This is useful for when we know a dynamic object will never move again




**Double Update Conflict**:

<!-- ![Double Update Conflict](NotesAssets/navigation_path_finding/double_conflict.jpg) -->




## Note 5

All objects in the scene originally not marked as being navigation static, essentially the navigation mesh generation process sees them as being dynamic objects and therefore their polygons should not be incorporated into enough mesh generation

object set `static` in the Inspector panel doesn't move w.r.t any subsystems.

making the following `static` for navigation mesh path finding not to ignore:

- debris
- ground
- liftshaft: have some basement infrastructure pillar can't be ignored
- battleBus

things doesn't necessarily need to be static: 

- wires
- decals (etc. on the wall)



## Note 9

**NavMesh Obstacles**
Similar to a collider object but used for the navigation system. Even though objects may have a collider on them, an agent on a navigation path will traverse through them without a NavMesh Obstacle (the collider is linked to the physics system, not the navigation system). 

Though a NavMesh Obstacle will stop a nav-agent from passing through an object it will not cause the agent to recompute it's path. This is because the object is not part of the nav mesh. With Unity 5+ the nav mesh obstacle has an option called 'carve'. This option causes the object to carve out a silhouette of itself from the nav mesh.

nav mesh obstacle: agent not possible to walk through any nav mesh obstacle

- tick `carve`
- untick `carve only statio` under `carve`



## Note 12:



state machine:



prevent from overwrite a animation behaviour during another:

in this case: 


- turning left: `Turning` < 1
- turning right: `Turning` > 1
- walk: `isWalking` --> `true`

> prevent `isWalking` overwrite turning behaviour, add `Turning` equals `0` condition to walking as a barrier to ban the overwrite

walk back to idle, add another transition: `Turning` notEquals `0`: whenever `Turning` not equals zero, transit back to Idle than to turning








#### AI State Machine
On an actor in the scene, add a capsule collider and position it to roughly fit around the model. Then add Rigidbody component with the 'Is Kinematic' option selected. This disables the physics on this entity. Go to the inspector and in the top right open the 'Layer' drop down. Add a layer called 'AI Entity'. Select the actor and change it's layer to the new 'AI Entity' layer (hit 'no' on the pop-up regarding it's children). Create another trigger called 'AI Entity Trigger'. Go to Edit > Project Settings > Physics. In the layer interaction matrix, de-select every row in the 'AI Entity' column and every row in the 'AI Entity Trigger' column except 'AI Entity'. Now the two new layers exclusively interact with one another. 

Create an empty game object called 'Omni Zombie' and set it to the 'AI Entity' layer. Make the actor a child of this object. Make another empty game object a child of 'Omni Zombie'. Call this second child 'Target Trigger' and set it to the 'AI Entity Trigger'. Add a spherical collider component to this trigger object. This trigger, in game, will be moved to the zombie's destination. The sphere will act as a trigger, notifying the zombie it has arrived at it's target.


