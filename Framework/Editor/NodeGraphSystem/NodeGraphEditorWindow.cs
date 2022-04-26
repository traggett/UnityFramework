using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			public sealed class NodeGraphEditorWindow : EditorWindow, NodeGraphEditor.IEditorWindow
			{
				private static readonly string kWindowTag = "NodeGraphEditor";
				private static readonly string kWindowWindowName = "Node Graph";
				private static readonly string kWindowTitle = "Node Graph Editor";

				private NodeGraphEditor _nodeGraphEditor;

				#region Menu Stuff
				private static NodeGraphEditorWindow _instance = null;

				[MenuItem("Window/Node Graph Editor")]
				private static void CreateWindow()
				{
					// Get existing open window or if none, make a new one:
					_instance = (NodeGraphEditorWindow)GetWindow(typeof(NodeGraphEditorWindow), false, kWindowWindowName);

					if (_instance._nodeGraphEditor == null || _instance._nodeGraphEditor.GetEditorWindow() == null)
					{
						_instance._nodeGraphEditor = NodeGraphEditor.CreateInstance<NodeGraphEditor>();
						_instance._nodeGraphEditor.Init(kWindowTitle, _instance, kWindowTag);
					}
				}

				[MenuItem("Assets/Load NodeGraph")]
				private static void MenuLoadNodeGraph()
				{
					if (Selection.activeObject is NodeGraph nodeGraph)
					{
						Load(nodeGraph);
					}
				}

				[MenuItem("Assets/Load NodeGraph", true)]
				private static bool ValidateMenuLoadNodeGraph()
				{
					return Selection.activeObject is NodeGraph;
				}
				#endregion

				public static void Load(NodeGraph nodeGraph)
				{
					if (_instance == null)
						CreateWindow();

					_instance.CreateEditor();

					_instance._nodeGraphEditor.Load(nodeGraph);
					_instance.Focus();
				}

				private void CreateEditor()
				{
					if (_nodeGraphEditor == null || _nodeGraphEditor.GetEditorWindow() == null)
					{
						_instance._nodeGraphEditor = NodeGraphEditor.CreateInstance<NodeGraphEditor>();
						_instance._nodeGraphEditor.Init(kWindowTitle, _instance, kWindowTag);
					}
				}

				#region EditorWindow
				void Update()
				{
					if (_nodeGraphEditor != null)
						_nodeGraphEditor.UpdateEditor();
				}

				void OnGUI()
				{
					if (_instance == null)
						CreateWindow();

					CreateEditor();

					Vector2 windowSize = new Vector2(this.position.width, this.position.height);
					_nodeGraphEditor.Render(windowSize);
				}

				void OnDestroy()
				{
					if (_nodeGraphEditor != null)
						_nodeGraphEditor.OnQuit();
				}
				#endregion

				#region IEditorWindow
				public void DoRepaint()
				{
					Repaint();
				}
				#endregion
			}
		}
	}
}