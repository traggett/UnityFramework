using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Framework
{
	using Utils;

	namespace UI
	{
		namespace TextMeshPro
		{
			[RequireComponent(typeof(TextMeshProUGUI))]
			public class TextMeshProLines : MonoBehaviour
			{
				#region Public Data
				public enum LineStyle
				{
					Underline,
					Strikethrough
				}
				public LineStyle _lineStyle;

				public bool ShowLines
				{
					get
					{
						return _showingLines;
					}
					set
					{
						if (value != _showingLines)
						{
							_showingLines = value;
							UpdateShowing();
						}
					}
				}

				public bool _showLinesOnEnable;
				public bool _animated;
				public float _linePadding;
				public float _lineShift;
				public float _lineHeight;
				public Color _lineColor;
				public float _speed;
				public float _acceleration;
				public float _endOfLinePause;
				public PrefabInstancePool _linePrefabPool;
				#endregion

				#region Private Functions
				private bool _showingLines;
				private TextMeshProUGUI _textMesh;
				private Image[] _lines = new Image[0];
				private int _lineIndex;
				private float _currentLineSpeed;
				private float _currentLinePauseTimer;
				#endregion

				private void Awake()
				{
					_textMesh = GetComponent<TextMeshProUGUI>();
				}

				private void OnEnable()
				{
					if (_showLinesOnEnable)
					{
						ShowLines = true;
					}
				}

				private void LateUpdate()
				{
					if (_showingLines)
					{
						UpdateLines(Time.deltaTime);
					}
				}

				private void UpdateShowing()
				{
					if (_showingLines)
					{
						_lines = new Image[0];
						_lineIndex = 0;
						_currentLineSpeed = _speed;
						if (this.isActiveAndEnabled)
							UpdateLines(0f);
					}
					else
					{
						ClearLines();
					}
				}

				private void UpdateLines(float deltaTime)
				{
					//keep lines in sync
					TMP_TextInfo textInfo = _textMesh.textInfo;
					float lineHeight = _lineHeight * _textMesh.fontSize;

					//Need to add new lines
					if (_lines.Length < textInfo.lineCount)
					{
						Image[] lines = new Image[textInfo.lineCount];

						for (int i = 0; i < lines.Length; i++)
						{
							if (i < _lines.Length)
							{
								lines[i] = _lines[i];
							}
							else
							{
								lines[i] = _linePrefabPool.Instantiate(_textMesh.rectTransform).GetComponent<Image>();
								lines[i].rectTransform.pivot = new Vector2(0f, 0.5f);
								lines[i].rectTransform.anchorMin = new Vector2(0f, 0f);
								lines[i].rectTransform.anchorMax = new Vector2(0f, 0f);
								lines[i].rectTransform.sizeDelta = new Vector2(0f, lineHeight);
								lines[i].color = _lineColor;
							}
						}

						_lines = lines;

						if (!_animated)
							_lineIndex = textInfo.lineCount;
					}
					//Need to remove lines
					else if (_lines.Length > textInfo.lineCount)
					{
						Image[] lines = new Image[textInfo.lineCount];

						for (int i = 0; i < _lines.Length; i++)
						{
							if (i < textInfo.lineCount)
							{
								lines[i] = _lines[i];
							}
							else
							{
								_linePrefabPool.Destroy(_lines[i].gameObject);
							}
						}

						_lines = lines;

						if (_lineIndex > textInfo.lineCount)
						{
							_lineIndex = textInfo.lineCount;
							_currentLineSpeed = _speed;
						}
					}

					//Update animations
					if (_animated && _lineIndex < _lines.Length && deltaTime > 0f)
					{
						RectTransform currentLine = _lines[_lineIndex].rectTransform;
						currentLine.anchoredPosition = GetLinePosition(textInfo.lineInfo[_lineIndex], out float lineWidth);

						float currentWidth = RectTransformUtils.GetWidth(currentLine);

						if (currentWidth < lineWidth)
						{
							_currentLineSpeed += _acceleration * deltaTime;
							currentWidth += _currentLineSpeed * deltaTime;

							if (currentWidth >= lineWidth)
							{
								currentWidth = lineWidth;
								_currentLinePauseTimer = _endOfLinePause;
							}
						}
						else
						{
							currentWidth = lineWidth;
							_currentLinePauseTimer -= deltaTime;

							if (_currentLinePauseTimer <= 0f)
							{
								_currentLineSpeed = _speed;
								_lineIndex++;
							}
						}

						currentLine.sizeDelta = new Vector2(currentWidth, lineHeight);
					}

					//Update shown lines
					for (int i = 0; i < Mathf.Min(_lines.Length, _lineIndex); i++)
					{
						_lines[i].rectTransform.anchoredPosition = GetLinePosition(textInfo.lineInfo[i], out float lineWidth);
						_lines[i].rectTransform.sizeDelta = new Vector2(lineWidth, lineHeight);
						_lines[i].color = _lineColor;
					}
				}

				public bool IsAnimating()
				{
					return _lines != null && _animated && _lineIndex < _lines.Length;
				}

				private Vector2 GetLinePosition(TMP_LineInfo lineInfo, out float lineWidth)
				{
					RectTransform textTransform = (RectTransform)this.transform;

					float linePadding = _linePadding * _textMesh.fontSize;
					float lineShift = _lineShift * _textMesh.fontSize;

					lineWidth = lineInfo.lineExtents.max.x - lineInfo.lineExtents.min.x + (linePadding * 2f);

					//Line extents is from pivot point
					float pivotX = textTransform.pivot.x * textTransform.rect.width;
					float x = pivotX + lineInfo.lineExtents.min.x - linePadding;

					float y;

					switch (_lineStyle)
					{
						case LineStyle.Strikethrough:
							y = lineInfo.baseline + lineShift + _textMesh.fontScale * _textMesh.font.faceInfo.strikethroughOffset;
							break;
						case LineStyle.Underline:
						default:
							y = lineInfo.baseline + lineShift;
							break;
					}

					//Line extents is from pivot point
					float pivotY = textTransform.pivot.y * textTransform.rect.height;
					y += pivotY;

					return new Vector2(x, y);
				}

				private void ClearLines()
				{
					for (int i = 0; i < _lines.Length; i++)
					{
						_linePrefabPool.Destroy(_lines[i].gameObject);
					}

					_lines = new Image[0];
				}
			}
		}
	}
}