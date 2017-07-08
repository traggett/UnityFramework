using System.Collections.Generic;
using System;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class NodeGraph : ISerializationCallbackReceiver
		{
			#region Public Data
			public Node[] _nodes = new Node[0];
			#endregion

			#region Private Data
			private List<Node> _nodeUpdateList;
			private float _time;
			#endregion

			#region Public Interface
			public static void FixupNodeRefs(NodeGraph nodeGraph, object node)
			{
				if (node != null)
				{
					object[] nodeFieldObjects = SerializedObjectMemberInfo.GetSerializedFieldInstances(node);

					foreach (object nodeFieldObject in nodeFieldObjects)
					{
						INodeInputField nodeField = nodeFieldObject as INodeInputField;

						if (nodeField != null)
						{
							nodeField.SetParentNodeGraph(nodeGraph);
						}

						FixupNodeRefs(nodeGraph, nodeFieldObject);
					}
				}
			}

			public Node GetNode(int nodeId)
			{
				foreach (Node node in _nodes)
				{
					if (node._nodeId == nodeId)
					{
						return node;
					}
				}

				return null;
			}

			public Node[] GetInputNodes()
			{
				List<Node> inputNodes = new List<Node>();

				foreach (Node node in _nodes)
				{
					if (SystemUtils.IsSubclassOfRawGeneric(typeof(InputNode<>), node.GetType()))
					{
						inputNodes.Add(node);
					}
				}

				return inputNodes.ToArray();
			}

			public Node[] GetOutputNodes()
			{
				List<Node> outputNodes = new List<Node>();

				foreach (Node node in _nodes)
				{
					if (SystemUtils.IsSubclassOfRawGeneric(typeof(OutputNode<,>), node.GetType()))
					{
						outputNodes.Add(node);
					}
				}

				return outputNodes.ToArray();
			}

			public void Init()
			{
				foreach (Node node in _nodes)
				{
					node.Init();
				}

				//Work out node update order based on dependencies
				_nodeUpdateList = new List<Node>();
				List<Node> nodesLeftToAdd = new List<Node>(_nodes);
				
				while (nodesLeftToAdd.Count > 0)
				{
					Node[] orderedNodes = GetOrderedNodesFromNode(nodesLeftToAdd[0]);

					foreach (Node orderedNode in orderedNodes)
					{
						nodesLeftToAdd.Remove(orderedNode);
					}

					_nodeUpdateList.AddRange(orderedNodes);
				}

				_time = 0.0f;
			}

			public void Update(float deltaTime)
			{
				_time += deltaTime;

				foreach (Node node in _nodeUpdateList)
				{
					node.Update(_time, deltaTime);
				}
			}
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				FixupNodeRefs(this, this);
			}
			#endregion

			#region Private Functions
			private Node[] GetOrderedNodesFromNode(Node node)
			{
				Dictionary<Node, int> nodePathRatings = new Dictionary<Node, int>();
				nodePathRatings[node] = 0;

				AddNodeDependancies(node, nodePathRatings, 0);

				//Once dictionary is updated then add nodes in order of index
				List<Node> nodes = new List<Node>(nodePathRatings.Keys);
				nodes.Sort((i1, i2) => nodePathRatings[i1].CompareTo(nodePathRatings[i2]));

				return nodes.ToArray();
			}

			private void AddNodeDependancies(Node node, Dictionary<Node, int> nodePathRatings, int currentIndex)
			{
				//Find nodes that output to this node
				Node[] linkedNodes = GetNodesLinkingToNode(node);

				foreach (Node linkedNode in linkedNodes) 
				{
					bool alreadyTravelledNode = nodePathRatings.ContainsKey(linkedNode);
					nodePathRatings[linkedNode] = currentIndex - 1;
					if (!alreadyTravelledNode)
						AddNodeDependancies(linkedNode, nodePathRatings, currentIndex - 1);
				}

				//Find all nodes that take this node as input? How to stop cyclic looping? Never want to add same node twice   
				//How to find nodes that this outputs to? Loop through nodes and find nodes where linked.

				linkedNodes = GetNodesLinkedFromNode(node);

				foreach (Node linkedNode in linkedNodes)
				{
					bool alreadyTravelledNode = nodePathRatings.ContainsKey(linkedNode);
					nodePathRatings[linkedNode] = currentIndex + 1;
					if (!alreadyTravelledNode)
						AddNodeDependancies(linkedNode, nodePathRatings, currentIndex + 1);
				}
			}

			private Node[] GetNodesLinkingToNode(Node node)
			{
				//Find NodeInputFieldBase fields in node
				List<Node> nodes = new List<Node>();

				object[] nodeFieldObjects = SerializedObjectMemberInfo.GetSerializedFieldInstances(node);

				foreach (object nodeFieldObject in nodeFieldObjects)
				{
					if (SystemUtils.IsSubclassOfRawGeneric(typeof(NodeInputFieldBase<>), nodeFieldObject.GetType()))
					{
						//Check linked node
						object nodeSourceId = SerializedObjectMemberInfo.GetSerializedFieldInstance(nodeFieldObject, "sourceNodeId");

						if (nodeSourceId != null)
						{
							Node linkedNode = GetNode((int)nodeSourceId);
							if (linkedNode != null)
								nodes.Add(linkedNode);
						}
					}
				}

				return nodes.ToArray();
			}

			private Node[] GetNodesLinkedFromNode(Node node)
			{
				List<Node> referenedNodes = new List<Node>();

				foreach (Node otherNode in _nodes)
				{
					if (otherNode != node)
					{
						Node[] linkedNodes = GetNodesLinkingToNode(otherNode);

						foreach (Node linkedNode in linkedNodes)
						{
							if (linkedNode == node)
							{
								referenedNodes.Add(otherNode);
							}
						}
					}
				}

				return referenedNodes.ToArray();
			}
			#endregion
		}
	}
}