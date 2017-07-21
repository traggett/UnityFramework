# UnityFramework
Framework for Unity that includes support for xml based NodeGraphs and Statemachines together with fancy editors for both.

# Features

### Statemachine Editor

![Alt text](ReadmeAssets/Statemachine.png?raw=true "The statemachine editor.")
![Alt text](ReadmeAssets/Timeline.png?raw=true "The statemachine editor.")

Allows you to visually create Statemachines to drive logic in your project or be used for things like branching dialgoue.
Statemachines are serialised to text (as xml) so can be reused throughout your project.
They support cross scene gameobject referencing and can be easily extendible.
It supports Timeline states which can trigger events (and its easy to write your own), Coroutine states that just run a IEnumerator function on a monobehaviour, and Conditional states which will make the statemachine remain in the state until a condition is met. This is used to branch logic or wait for an player action.

### NodeGraph Editor

![Alt text](ReadmeAssets/NodeGraphEditor.png?raw=true "The node graph editor.")

The node graph editor allows you to create node graphs that can be used to drive animations, blend values on materials, change light colors etc and are easily extendible to add your own nodes.
Node graphs are serialised to text (as xml) so can be reused throughout your project.
They also take inputs from components in scenes so a single node graph can be.