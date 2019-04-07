using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000806 RID: 2054
[ExecuteInEditMode]
public class GameWorldArea : MonoBehaviour
{
	// Token: 0x06002C7C RID: 11388
	public GameWorldArea()
	{
	}

	// Token: 0x170006F5 RID: 1781
	// (get) Token: 0x06002C7D RID: 11389
	public Bounds Bounds
	{
		get
		{
			return new Bounds(this.BoundingTransform.position, this.BoundingTransform.localScale);
		}
	}

	// Token: 0x170006F6 RID: 1782
	// (get) Token: 0x06002C7E RID: 11390
	public Rect BoundingRect
	{
		get
		{
			return new Rect
			{
				width = this.BoundingTransform.lossyScale.x,
				height = this.BoundingTransform.lossyScale.y,
				center = this.BoundingTransform.position
			};
		}
	}

	// Token: 0x06002C7F RID: 11391
	public bool InsideFace(Vector3 worldPosition)
	{
		Vector3 position = this.BoundaryCage.transform.InverseTransformPoint(worldPosition);
		return this.BoundaryCage.FindFaceAtPositionFaster(position) != null;
	}

	// Token: 0x040027FB RID: 10235
	private const float PIXELS_PER_UNIT = 5f;

	// Token: 0x040027FC RID: 10236
	public List<GameWorldArea.WorldMapIcon> Icons = new List<GameWorldArea.WorldMapIcon>();

	// Token: 0x040027FD RID: 10237
	public MessageProvider AreaName;

	// Token: 0x040027FE RID: 10238
	public MessageProvider LowerAreaName;

	// Token: 0x040027FF RID: 10239
	public string AreaNameString;

	// Token: 0x04002800 RID: 10240
	public CageStructureTool CageStructureTool;

	// Token: 0x04002801 RID: 10241
	public Transform BoundingTransform;

	// Token: 0x04002802 RID: 10242
	public Texture WorldMapTexture;

	// Token: 0x04002803 RID: 10243
	public string AreaIdentifier = string.Empty;

	// Token: 0x04002804 RID: 10244
	public CageStructureTool BoundaryCage;

	// Token: 0x04002805 RID: 10245
	public Condition VisitableCondition;

	// Token: 0x02000807 RID: 2055
	[Serializable]
	public class WorldMapIcon
	{
		// Token: 0x06002C80 RID: 11392
		public WorldMapIcon(SceneMetaData.WorldMapIcon worldMapIcon)
		{
			this.Guid = new MoonGuid(worldMapIcon.Guid);
			this.Position = worldMapIcon.Position;
			this.Icon = worldMapIcon.Icon;
			this.IsSecret = worldMapIcon.IsSecret;
		}

		// Token: 0x06003812 RID: 14354
		public WorldMapIcon()
		{
		}

		// Token: 0x04002806 RID: 10246
		public MoonGuid Guid;

		// Token: 0x04002807 RID: 10247
		public WorldMapIconType Icon;

		// Token: 0x04002808 RID: 10248
		public Vector2 Position;

		// Token: 0x04002809 RID: 10249
		public bool IsSecret;
	}
}
