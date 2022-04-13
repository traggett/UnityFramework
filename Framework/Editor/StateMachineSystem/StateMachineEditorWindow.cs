﻿using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	
	namespace StateMachineSystem
	{
		namespace Editor
		{
			public sealed class StateMachineEditorWindow : EditorWindow, StateMachineEditor.IEditorWindow
			{
				private static readonly string kWindowTag = "StateMachineEditor";
				private static readonly string kWindowWindowName = "State Machine";
				private static readonly string kWindowTitle = "State Machine Editor";

				private StateMachineEditor _stateMachineEditor;
				
				#region Menu Stuff
				private static StateMachineEditorWindow _instance = null;

				[MenuItem("Window/State Machine Editor")]
				private static void CreateWindow()
				{
					// Get existing open window or if none, make a new one:
					_instance = (StateMachineEditorWindow)GetWindow(typeof(StateMachineEditorWindow), false, kWindowWindowName);
				}

				[MenuItem("Assets/Load State Machine")]
				private static void MenuLoadTimeline()
				{
					TextAsset asset = Selection.activeObject as TextAsset;
					if (asset != null)
					{
						Load(asset);
					}
				}

				[MenuItem("Assets/Load State Machine", true)]
				private static bool ValidateMenuLoadTimeline()
				{
					TextAsset asset = Selection.activeObject as TextAsset;

					if (asset != null)
					{
						return Serializer.DoesAssetContainObject<StateMachine>(asset);
					}

					return false;
				}
				#endregion

				public static void Load(TextAsset textAsset)
				{
					if (_instance == null)
						CreateWindow();

					string fileName = AssetDatabase.GetAssetPath(textAsset);
					_instance.Load(fileName);
					_instance.Focus();
				}

				public void Load(string fileName)
				{
					if (_instance == null)
						CreateWindow();

					CreateEditor();

					_stateMachineEditor.Load(fileName);
				}

				private void CreateEditor()
				{
					if (_stateMachineEditor == null || _stateMachineEditor.GetEditorWindow() == null)
					{
						StateMachineEditorStyle style = new StateMachineEditorStyle();

						_stateMachineEditor = StateMachineEditor.CreateInstance<StateMachineEditor>();
						_stateMachineEditor.Init(kWindowTitle, this, kWindowTag, style);
					}
				}

				#region EditorWindow
				void Update()
				{
					if (_stateMachineEditor != null)
						_stateMachineEditor.UpdateEditor();
				}

				void OnGUI()
				{
					if (_instance == null)
						CreateWindow();

					CreateEditor();

					Vector2 windowSize = new Vector2(this.position.width, this.position.height);
					_stateMachineEditor.Render(windowSize);
				}

				void OnDestroy()
				{
					if (_stateMachineEditor != null)
						_stateMachineEditor.OnQuit();
				}
				#endregion

				#region IEditorWindow
				public void DoRepaint()
				{
					Repaint();
				}

				public void OnSelectObject(ScriptableObject obj)
				{
					Selection.activeObject = obj;
					GUIUtility.keyboardControl = 0;
				}

				public void OnDeselectObject(ScriptableObject obj)
				{
					if (Selection.activeObject == obj)
						Selection.activeObject = null;
				}
				#endregion
			}
		}
	}
}