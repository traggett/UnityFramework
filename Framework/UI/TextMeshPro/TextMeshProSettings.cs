using System;
using UnityEngine;
using TMPro;

namespace Framework
{
	namespace UI
	{
		namespace TextMeshPro
		{
			[Serializable]
			public class TextMeshProSettings
			{
				//Font settings
				public TMP_FontAsset _font;
				public Material _fontMaterial;
				public FontStyles _fontStyle;
				public float _fontSize;
				//Auto sizing
				public bool _autoSize;
				public float _fontSizeMin;
				public float _fontSizeMax;
				public float _characterWidthAdjustment;
				public float _lineSpacingAdjustment;
				//Color
				public Color _vertexColor;
				public bool _enableVertexGradient;
				public VertexGradient _colorGradient;
				//Spacing
				public float _characterSpacing;
				public float _wordSpacing;
				public float _lineSpacing;
				public float _paragraphSpacing;
				public TextAlignmentOptions _alignment;
				public bool _enableWordWrapping;
				public TextOverflowModes _overflowMode;
				public TextureMappingOptions _horizontalMapping;
				public TextureMappingOptions _verticalMapping;
				//Extra Settings
				public Vector4 _margin;
				public VertexSortingOrder _geometrySortingOrder;
				public bool _isTextObjectScaleStatic;
				public bool _richText;
				public bool _raycastTarget;
				public bool _maskable;
				public bool _parseCtrlCharacters;
				public bool _useMaxVisibleDescender;
				public TMP_SpriteAsset _spriteAsset;
				public TMP_StyleSheet _styleSheet;
				public bool _enableKerning;
				public bool _extraPadding;

				public void Apply(TMP_Text textMesh)
				{
					textMesh.font = _font;
					textMesh.fontSharedMaterial = _fontMaterial;
					textMesh.fontStyle = _fontStyle;
					textMesh.fontSize = _fontSize;
					textMesh.enableAutoSizing = _autoSize;
					textMesh.fontSizeMin = _fontSizeMin;
					textMesh.fontSizeMax = _fontSizeMax;
					textMesh.characterWidthAdjustment = _characterWidthAdjustment;
					textMesh.lineSpacingAdjustment = _lineSpacingAdjustment;
					textMesh.color = _vertexColor;
					textMesh.enableVertexGradient = _enableVertexGradient;
					textMesh.colorGradient = _colorGradient;
					textMesh.characterSpacing = _characterSpacing;
					textMesh.wordSpacing = _wordSpacing;
					textMesh.lineSpacing = _lineSpacing;
					textMesh.paragraphSpacing = _paragraphSpacing;
					textMesh.alignment = _alignment;
					textMesh.enableWordWrapping = _enableWordWrapping;
					textMesh.overflowMode = _overflowMode;
					textMesh.horizontalMapping = _horizontalMapping;
					textMesh.verticalMapping = _verticalMapping;
					textMesh.margin = _margin;
					textMesh.geometrySortingOrder = _geometrySortingOrder;
					textMesh.isTextObjectScaleStatic = _isTextObjectScaleStatic;
					textMesh.richText = _richText;
					textMesh.raycastTarget = _raycastTarget;
					textMesh.maskable = _maskable;
					textMesh.parseCtrlCharacters = _parseCtrlCharacters;
					textMesh.useMaxVisibleDescender = _useMaxVisibleDescender;
					textMesh.spriteAsset = _spriteAsset;
					textMesh.styleSheet = _styleSheet;
					textMesh.enableKerning = _enableKerning;
					textMesh.extraPadding = _extraPadding;
				}

				public static TextMeshProSettings FromTextMesh(TMP_Text textMesh)
				{
					return new TextMeshProSettings
					{
						_font = textMesh.font,
						_fontMaterial = textMesh.fontMaterial,
						_fontStyle = textMesh.fontStyle,
						_fontSize = textMesh.fontSize,
						_autoSize = textMesh.enableAutoSizing,
						_fontSizeMin = textMesh.fontSizeMin,
						_fontSizeMax = textMesh.fontSizeMax,
						_characterWidthAdjustment = textMesh.characterWidthAdjustment,
						_lineSpacingAdjustment = textMesh.lineSpacingAdjustment,
						_vertexColor = textMesh.color,
						_enableVertexGradient = textMesh.enableVertexGradient,
						_colorGradient = textMesh.colorGradient,
						_characterSpacing = textMesh.characterSpacing,
						_wordSpacing = textMesh.wordSpacing,
						_lineSpacing = textMesh.lineSpacing,
						_paragraphSpacing = textMesh.paragraphSpacing,
						_alignment = textMesh.alignment,
						_enableWordWrapping = textMesh.enableWordWrapping,
						_overflowMode = textMesh.overflowMode,
						_horizontalMapping = textMesh.horizontalMapping,
						_verticalMapping = textMesh.verticalMapping,
						_margin = textMesh.margin,
						_geometrySortingOrder = textMesh.geometrySortingOrder,
						_isTextObjectScaleStatic = textMesh.isTextObjectScaleStatic,
						_richText = textMesh.richText,
						_raycastTarget = textMesh.raycastTarget,
						_maskable = textMesh.maskable,
						_parseCtrlCharacters = textMesh.parseCtrlCharacters,
						_useMaxVisibleDescender = textMesh.useMaxVisibleDescender,
						_spriteAsset = textMesh.spriteAsset,
						_styleSheet = textMesh.styleSheet,
						_enableKerning = textMesh.enableKerning,
						_extraPadding = textMesh.extraPadding
					};
				}
			}
		}
	}
}