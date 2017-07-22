# UnityFramework
Framework for Unity that includes support for NodeGraphs and Statemachines together with editors for both.
It also has a system for easy localisation, a flexible save game system, loads of helpful math functions and more!

# Features

### Statemachine Editor

![Alt text](ReadmeAssets/Statemachine.png?raw=true "The statemachine editor.")
![Alt text](ReadmeAssets/Timeline.png?raw=true "The statemachine editor.")

The statemachine editor allows you to create statemachines that can be used to drive logic in your project or be used for things like branching dialgoue.
Statemachines are serialised to text assets (as xml) and they support cross scene references for gameobjects/components so can control your games flow whilst it loads / unloads different scenes.
Different types of states are supported:
- Timeline states which can trigger events (and its easy to write/add your own)
- Coroutine states that simply run and wait for a coroutine on a monobehaviour
- Conditional states which will make the statemachine remain in the state until a condition is met. These can be used to branch logic or wait for an player action (its also easy to write/add your own conditions for branching logic)

### NodeGraph Editor

![Alt text](ReadmeAssets/NodeGraphEditor.png?raw=true "The node graph editor.")

The node graph editor allows you to create node graphs that can be used to drive animations, blend values on materials, change light colors etc and are easily extendible to add your own nodes.
Node graphs are likewise serialised to text assets (as xml) so can be reused throughout your project. 

The component that runs the nodegraph can pass it unique data values via 'input nodes'.
This means for example the same node graph that idly oscillates an object (eg a floating rock) can be reused on different objects each with different values for oscillation height and speed - you dont have to change the node graphs, instead simply use different input values on the component.

Node graphs can also have output nodes which allow them to be chained - one Node Graphs output can be hooked up to the input of another.
This allows controlling a load of nodegraphs from one single one, eg controlling a scenes lighting and post effects based on a nodegraph that relates to the suns position in the sky.
