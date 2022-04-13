using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(NodeEditorGUI), true)]
			public sealed class NodeEditorGUIInspector : SerializedObjectEditorGUIInspector<Node>
			{
				/*
				public override bool RenderObjectProperties(GUIContent label)
				{
					EditorGUI.BeginChangeCheck();
					GetEditableObject()._editorDescription = EditorGUILayout.DelayedTextField("Editor Description", GetEditableObject()._editorDescription);

					bool dataChanged = EditorGUI.EndChangeCheck();

					//Render Inputs
					bool renderedFirstInput = false;
					{
						foreach (NodeEditorField input in _inputNodeFields)
						{
							if (!renderedFirstInput)
							{
								EditorGUILayout.Separator();
								EditorGUILayout.LabelField("Inputs", EditorStyles.boldLabel);
								EditorGUILayout.Separator();
								renderedFirstInput = true;
							}

							string fieldName = StringUtils.FromCamelCase(input._name);
							TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(input._fieldInfo);
							GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

							bool fieldChanged = false;
							object nodeFieldObject = input.GetValue();
							nodeFieldObject = SerializationEditorGUILayout.ObjectField(nodeFieldObject, labelContent, ref fieldChanged);
							if (fieldChanged)
							{
								dataChanged = true;
								input.SetValue(nodeFieldObject);
							}
						}
					}

					//Render other properties
					bool renderedFirstProperty = false;
					{
						SerializedObjectMemberInfo[] serializedFields = SerializedObjectMemberInfo.GetSerializedFields(GetEditableObject().GetType());
						foreach (SerializedObjectMemberInfo serializedField in serializedFields)
						{
							if (!serializedField.HideInEditor() && !SystemUtils.IsSubclassOfRawGeneric(typeof(NodeInputFieldBase<>), serializedField.GetFieldType()))
							{
								if (!renderedFirstProperty)
								{
									EditorGUILayout.Separator();
									EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
									EditorGUILayout.Separator();
									renderedFirstProperty = true;
								}

								string fieldName = StringUtils.FromCamelCase(serializedField.GetID());
								TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(serializedField);
								GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

								bool fieldChanged = false;
								object nodeFieldObject = serializedField.GetValue(GetEditableObject());
								nodeFieldObject = SerializationEditorGUILayout.ObjectField(nodeFieldObject, labelContent, ref fieldChanged);
								if (fieldChanged)
								{
									dataChanged = true;
									serializedField.SetValue(GetEditableObject(), nodeFieldObject);
								}
							}
						}
					}

					return dataChanged;
				}
				*/
			}
		}
	}
}