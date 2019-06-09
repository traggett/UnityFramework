using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public enum TEXCOORD : int
		{
			TEXCOORD0,
			TEXCOORD1,
			TEXCOORD2,
			TEXCOORD3,
			TEXCOORD4,
			TEXCOORD5,
			TEXCOORD6,
			TEXCOORD7,
			TEXCOORD8,
			TEXCOORD9,
			TEXCOORD10,
			TEXCOORD11,
			TEXCOORD12,
			TEXCOORD13,
			TEXCOORD14,
			TEXCOORD15
		}

		public static class GraphicsUtils
		{
			private static Material _basicBlitMaterial;

			public static void CameraBlit(Camera camera, Texture source, RenderTexture dest)
			{
				CameraBlit(camera, source, dest, GetBlitMaterial(), 0);
			}
			
			//Does a blit respecting the cameras view rect.
			public static void CameraBlit(Camera camera, Texture source, RenderTexture dest, Material mat, int pass = -1)
			{
				Graphics.SetRenderTarget(dest);

				// Set up the simple Matrix
				GL.PushMatrix();
				GL.LoadOrtho();
				mat.mainTexture = source;

				for (int i= (pass == -1 ? 0 : pass); i<mat.passCount; i++)
				{
					if (mat.SetPass(i))
					{
						GL.Begin(GL.QUADS);
						//GL.RenderTargetBarrier();

						float xMin, xMax, yMin, yMax;

						if (NeedToAdjustForCamera(camera, source, dest))
						{
							xMin = camera.rect.xMin;
							xMax = camera.rect.xMax;
							yMin = camera.rect.yMin;
							yMax = camera.rect.yMax;
						}
						else
						{
							xMin = 0.0f;
							xMax = 1.0f;
							yMin = 0.0f;
							yMax = 1.0f;
						}
						
						if (source != null && source.texelSize.y < 0.0f)
						{
							yMin = 1.0f - yMin;
							yMax = 1.0f - yMax;
						}

						GL.TexCoord2(0.0f, 0); GL.Vertex3(xMin, yMin, 0.1f);
						GL.TexCoord2(1.0f, 0); GL.Vertex3(xMax, yMin, 0.1f);
						GL.TexCoord2(1.0f, 1); GL.Vertex3(xMax, yMax, 0.1f);
						GL.TexCoord2(0.0f, 1); GL.Vertex3(xMin, yMax, 0.1f);

						GL.Flush();
						GL.End();
					}

					//Only render single pass if specified
					if (pass != -1)
						break;
				}
				
				GL.PopMatrix();
				mat.mainTexture = null;
			}

			public static RenderTexture CreateTemporyCopy(RenderTexture texture)
			{
				RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height, texture.depth, texture.format);

				Graphics.Blit(texture, temp, GetBlitMaterial(), 0);

				return temp;
			}

			public static Vector2 GetCameraViewport(Vector2 screenPos, Camera camera)
			{
				Vector2 viewportPos;

				viewportPos.x = (screenPos.x - camera.rect.x) / camera.rect.width;
				viewportPos.y = (screenPos.y - camera.rect.y) / camera.rect.height;

				return viewportPos;
			}

			private static bool NeedToAdjustForCamera(Camera camera, Texture source, RenderTexture dest)
			{
				if (dest == null || source == null)
					return true;

				if (source.width == dest.width && source.height == dest.height)
					return false;

				if (source.width == dest.width / 2 && source.height == dest.height / 2)
					return false;

				if (source.width == dest.width / 4 && source.height == dest.height / 4)
					return false;

				if (source.width / 2 == dest.width && source.height / 2 == dest.height)
					return false;

				if (source.width / 4 == dest.width && source.height / 4 == dest.height)
					return false;

				return true;
			}

			private static Material GetBlitMaterial()
			{
				if (_basicBlitMaterial == null)
				{
					Shader shader = Shader.Find("Hidden/Internal-GUITextureBlit");
					_basicBlitMaterial = new Material(shader);
				}

				return _basicBlitMaterial;
			}
		}
	}
}