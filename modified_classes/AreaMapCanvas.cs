using System;
using UnityEngine;

public class AreaMapCanvas : MonoBehaviour
{
	public void Awake()
	{
		this.RuntimeArea = GameWorld.Instance.FindRuntimeArea(this.Area);
		this.Mask = this.Area.WorldMapTexture;
	}

	public void ResetMap()
	{
		if (this.Area.VisitableCondition && !this.Area.VisitableCondition.Validate(null))
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			base.gameObject.SetActive(true);
		}
		this.MapPlaneTexture.localScale = new Vector3(this.Bounds.size.x, this.Bounds.size.y);
		this.MapPlaneTexture.localPosition = this.Bounds.center;
		if (this.WorldMapTexture)
		{
			this.MapPlaneTexture.GetComponent<Renderer>().material.SetTexture(ShaderProperties.MainTexture, this.WorldMapTexture);
		}
		this.UpdateAreaMaskTextureA();
		if (this.m_addToMap)
		{
			InstantiateUtility.Destroy(this.m_addToMap);
		}
		this.SetFade(0f);
	}

	public Texture WorldMapTexture
	{
		get
		{
			return this.Area.WorldMapTexture;
		}
	}

	public Bounds Bounds
	{
		get
		{
			return this.Area.Bounds;
		}
	}

	public CageStructureTool CageStructureTool
	{
		get
		{
			return this.Area.CageStructureTool;
		}
	}

	public Vector2 WorldMapTextureSize
	{
		get
		{
			return new Vector2((float)this.WorldMapTexture.width, (float)this.WorldMapTexture.height);
		}
	}

	public RenderTexture GenerateAreaMaskMaskTexture()
	{
		int width = (int)Mathf.Min(1024f, this.Bounds.size.x * (float)this.PixelsPerUnit);
		int height = (int)Mathf.Min(1024f, this.Bounds.size.y * (float)this.PixelsPerUnit);
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		temporary.name = "worldMapCanvas";
		Graphics.SetRenderTarget(temporary);
		GL.Clear(false, true, Color.clear);
		GL.PushMatrix();
		GL.LoadIdentity();
		GL.LoadPixelMatrix(this.Bounds.min.x + 0.5f, this.Bounds.max.x + 0.5f, this.Bounds.min.y + 0.5f, this.Bounds.max.y + 0.5f);
		Matrix4x4 localToWorldMatrix = this.CageStructureTool.transform.localToWorldMatrix;
		GL.MultMatrix(localToWorldMatrix);
		Material material = new Material(this.SetRGBAShader);
		material.SetColor(ShaderProperties.Color, Color.white / 2f);
		material.SetPass(0);
		GL.Begin(4);
		GL.Color(Color.white);
		for (int i = 0; i < this.CageStructureTool.Faces.Count; i++)
		{
			CageStructureTool.Face face = this.CageStructureTool.Faces[i];
			for (int j = 0; j < face.Triangles.Count; j++)
			{
				int index = face.Triangles[j];
				GL.Vertex(this.CageStructureTool.VertexByIndex(face.Vertices[index]).Position);
			}
		}
		GL.End();
		GL.PopMatrix();
		return temporary;
	}

	public Color GetColor(WorldMapAreaState worldState)
	{
		switch (worldState)
		{
		case WorldMapAreaState.Hidden:
			if (AreaMapUI.Instance.DebugNavigation.UndiscoveredMapVisible)
			{
				return Color.white;
			}
			return Color.clear;
		case WorldMapAreaState.Discovered:
			return Color.red;
		case WorldMapAreaState.Visited:
			return Color.white;
		default:
			return Color.red;
		}
	}

	public RenderTexture GenerateAreaMaskTexture()
	{
		int width = (int)Mathf.Min(1024f, this.Bounds.size.x * (float)this.PixelsPerUnit);
		int height = (int)Mathf.Min(1024f, this.Bounds.size.y * (float)this.PixelsPerUnit);
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		temporary.name = "worldmapCanvas";
		Graphics.SetRenderTarget(temporary);
		GL.Clear(false, true, Color.clear);
		GL.PushMatrix();
		GL.LoadIdentity();
		GL.LoadPixelMatrix(this.Bounds.min.x + 0.5f, this.Bounds.max.x + 0.5f, this.Bounds.min.y + 0.5f, this.Bounds.max.y + 0.5f);
		Material material = new Material(this.SetRGBAShader);
		material.SetColor(ShaderProperties.Color, Color.white / 2f);
		material.SetPass(0);
		GL.Begin(4);
		Matrix4x4 localToWorldMatrix = this.CageStructureTool.transform.localToWorldMatrix;
		foreach (CageStructureTool.Face face in this.CageStructureTool.Faces)
		{
			WorldMapAreaState faceState = this.RuntimeArea.GetFaceState(face.ID);
			GL.Color(this.GetColor(faceState));
			foreach (int index in face.Triangles)
			{
				GL.Vertex(localToWorldMatrix.MultiplyPoint(this.CageStructureTool.VertexByIndex(face.Vertices[index]).Position));
			}
		}
		GL.End();
		GL.PopMatrix();
		RenderTexture result = this.BlurTextures(temporary);
		RenderTexture.ReleaseTemporary(temporary);
		return result;
	}

	public void Update()
	{
		if (this.m_areaMaskTextureA && !this.m_areaMaskTextureA.IsCreated())
		{
			this.UpdateAreaMaskTextureA();
		}
		if (this.m_areaMaskTextureB && !this.m_areaMaskTextureB.IsCreated())
		{
			this.UpdateAreaMaskTextureB();
		}
	}

	public void UpdateAreaMaskTextureA()
	{
		if (this.m_areaMaskTextureA)
		{
			UnityEngine.Object.DestroyObject(this.m_areaMaskTextureA);
		}
		this.m_areaMaskTextureA = this.GenerateAreaMaskTexture();
		this.MapPlaneTexture.GetComponent<Renderer>().material.SetTexture(ShaderProperties.MapMaskTextureA, this.m_areaMaskTextureA);
	}

	public void UpdateAreaMaskTextureB()
	{
		if (this.m_areaMaskTextureB)
		{
			UnityEngine.Object.DestroyObject(this.m_areaMaskTextureB);
		}
		this.m_areaMaskTextureB = this.GenerateAreaMaskTexture();
		this.MapPlaneTexture.GetComponent<Renderer>().material.SetTexture(ShaderProperties.MapMaskTextureB, this.m_areaMaskTextureB);
	}

	public void SetFade(float fade)
	{
		this.MapPlaneTexture.GetComponent<Renderer>().material.SetFloat(ShaderProperties.MapFade, fade);
	}

	public void OnDestroy()
	{
		this.Release();
	}

	public void Release()
	{
		if (this.m_areaMaskTextureA)
		{
			UnityEngine.Object.DestroyObject(this.m_areaMaskTextureA);
			this.m_areaMaskTextureA = null;
		}
		if (this.m_areaMaskTextureB)
		{
			UnityEngine.Object.DestroyObject(this.m_areaMaskTextureB);
			this.m_areaMaskTextureB = null;
		}
	}

	public RenderTexture BlurTextures(Texture originalTexture)
	{
		Texture mask = this.Mask;
		Material material = new Material(this.WorldMapBlurShader);
		material.SetTexture(ShaderProperties.MaskTex, mask);
		int width = originalTexture.width;
		int height = originalTexture.height;
		Vector2 vector = new Vector2(1.5f / this.MapPlaneTexture.localScale.x, 1.5f / this.MapPlaneTexture.localScale.y);
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		temporary.name = "worldMapCanvas";
		temporary2.name = "worldMapCanvasB";
		Texture texture = originalTexture;
		RenderTexture renderTexture = temporary;
		RenderTexture renderTexture2 = temporary2;
		renderTexture.name = "current";
		renderTexture2.name = "next";
		int num = 5;
		for (int i = 0; i < num; i++)
		{
			material.SetVector(ShaderProperties.BlurSize, new Vector4(vector.x, vector.y, 0f, 0f) * (1f + (float)i / 6f));
			material.SetVector(ShaderProperties.TextureScalingAndOffset, new Vector4(1f, 1f, 0f, 0f));
			RenderTexture.active = renderTexture;
			Graphics.Blit(texture, renderTexture, material);
			texture = renderTexture;
			renderTexture = renderTexture2;
			renderTexture2 = (RenderTexture)texture;
		}
		RenderTexture renderTexture3 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
		renderTexture3.hideFlags = HideFlags.DontSave;
		Graphics.Blit(texture, renderTexture3);
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		return renderTexture3;
	}

	public void ReleaseAreaMaskTextureB()
	{
		if (this.m_areaMaskTextureB)
		{
			UnityEngine.Object.DestroyObject(this.m_areaMaskTextureB);
			this.m_areaMaskTextureB = null;
		}
	}

	public GameWorldArea Area;

	public RuntimeGameWorldArea RuntimeArea;

	public Shader WorldMapBlurShader;

	public Transform MapPlaneTexture;

	public Texture Mask;

	public int PixelsPerUnit = 5;

	private GameObject m_addToMap;

	private RenderTexture m_areaMaskTextureA;

	private RenderTexture m_areaMaskTextureB;

	public Shader SetRGBAShader;
}