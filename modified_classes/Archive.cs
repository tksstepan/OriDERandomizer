	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;

	// Token: 0x020005FE RID: 1534
	public class Archive
	{
		// Token: 0x060020EB RID: 8427 RVA: 0x0001BE6E File Offset: 0x0001A06E
		public Archive()
		{
			this.MemoryStream = new MemoryStream();
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x060020EC RID: 8428 RVA: 0x0001BE8C File Offset: 0x0001A08C
		// (set) Token: 0x060020ED RID: 8429 RVA: 0x000959AC File Offset: 0x00093BAC
		public MemoryStream MemoryStream
		{
			get
			{
				return this.m_memoryStream;
			}
			set
			{
				if (this.m_memoryStream != null)
				{
					((IDisposable)this.m_memoryStream).Dispose();
				}
				if (this.m_binaryReader != null)
				{
					((IDisposable)this.m_binaryReader).Dispose();
				}
				if (this.m_binaryWriter != null)
				{
					((IDisposable)this.m_binaryWriter).Dispose();
				}
				this.m_memoryStream = value;
				this.m_binaryReader = new BinaryReader(this.m_memoryStream);
				this.m_binaryWriter = new BinaryWriter(this.m_memoryStream);
			}
		}

		// Token: 0x060020EE RID: 8430 RVA: 0x0001BE94 File Offset: 0x0001A094
		public void WriteMemoryStreamToBinaryWriter(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((int)this.MemoryStream.Length);
			this.MemoryStream.WriteTo(binaryWriter.BaseStream);
		}

		// Token: 0x060020EF RID: 8431 RVA: 0x00095A24 File Offset: 0x00093C24
		public void ReadMemoryStreamFromBinaryReader(BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			this.MemoryStream.SetLength((long)num);
			binaryReader.Read(this.MemoryStream.GetBuffer(), 0, num);
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x060020F0 RID: 8432 RVA: 0x0001BEB9 File Offset: 0x0001A0B9
		public bool Reading
		{
			get
			{
				return !this.m_write;
			}
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x060020F1 RID: 8433 RVA: 0x0001BEC4 File Offset: 0x0001A0C4
		public bool Writing
		{
			get
			{
				return this.m_write;
			}
		}

		// Token: 0x060020F2 RID: 8434 RVA: 0x0001BECC File Offset: 0x0001A0CC
		public void ResetStream()
		{
			this.MemoryStream.Position = 0L;
		}

		// Token: 0x060020F3 RID: 8435 RVA: 0x0001BEDB File Offset: 0x0001A0DB
		public void WriteMode()
		{
			this.ResetStream();
			this.m_write = true;
		}

		// Token: 0x060020F4 RID: 8436 RVA: 0x0001BEEA File Offset: 0x0001A0EA
		public void ReadMode()
		{
			this.m_memoryStream.Position = 0L;
			this.m_write = false;
		}

		// Token: 0x060020F5 RID: 8437 RVA: 0x0001BF00 File Offset: 0x0001A100
		public void Serialize(ref float value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020F6 RID: 8438 RVA: 0x0001BF0C File Offset: 0x0001A10C
		public void Serialize(ref int value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020F7 RID: 8439 RVA: 0x0001BF18 File Offset: 0x0001A118
		public void Serialize(ref bool value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020F8 RID: 8440 RVA: 0x0001BF24 File Offset: 0x0001A124
		public void Serialize(ref string value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020F9 RID: 8441 RVA: 0x0001BF30 File Offset: 0x0001A130
		public void Serialize(ref Vector2 value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020FA RID: 8442 RVA: 0x0001BF44 File Offset: 0x0001A144
		public void Serialize(ref Vector3 value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020FB RID: 8443 RVA: 0x0001BF58 File Offset: 0x0001A158
		public void Serialize(ref Quaternion value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref Dictionary<int,int> value)
		{
			value = this.Serialize(value);
		}

		// Token: 0x060020FC RID: 8444 RVA: 0x0001BF6C File Offset: 0x0001A16C
		public float Serialize(float value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadSingle();
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x0001BF92 File Offset: 0x0001A192
		public int Serialize(int value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadInt32();
		}

		// Token: 0x060020FE RID: 8446 RVA: 0x0001BFB8 File Offset: 0x0001A1B8
		public bool Serialize(bool value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadBoolean();
		}

		// Token: 0x060020FF RID: 8447 RVA: 0x0001BFDE File Offset: 0x0001A1DE
		public string Serialize(string value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadString();
		}

		// Token: 0x06002100 RID: 8448 RVA: 0x0001C004 File Offset: 0x0001A204
		public Vector2 Serialize(Vector2 value)
		{
			value.x = this.Serialize(value.x);
			value.y = this.Serialize(value.y);
			return value;
		}

		// Token: 0x06002101 RID: 8449 RVA: 0x0001C02F File Offset: 0x0001A22F
		public Vector3 Serialize(Vector3 value)
		{
			value.x = this.Serialize(value.x);
			value.y = this.Serialize(value.y);
			value.z = this.Serialize(value.z);
			return value;
		}

		// Token: 0x06002102 RID: 8450 RVA: 0x00095A5C File Offset: 0x00093C5C
		public Quaternion Serialize(Quaternion value)
		{
			value.x = this.Serialize(value.x);
			value.y = this.Serialize(value.y);
			value.z = this.Serialize(value.z);
			value.w = this.Serialize(value.w);
			return value;
		}
		public Dictionary<int,int> Serialize(Dictionary<int,int> value)
		{
			String pairs = "";
			if (this.m_write)
			{
				foreach(int key in value.Keys) {
					pairs += key.ToString() + ":"+value[key].ToString()+",";	
				}
				pairs.TrimEnd(',');

				this.m_binaryWriter.Write(pairs);
				return value;
			}
			pairs = this.m_binaryReader.ReadString();
			foreach(string pair in pairs.Split(',')) {
				string[] kandv = pair.Split(':');
				value[int.Parse(kandv[0])] = int.Parse(kandv[1]);							
			}
			return value;

		}


		// Token: 0x06002103 RID: 8451 RVA: 0x000028E7 File Offset: 0x00000AE7
		public void SerializeVersion(ref int version)
		{
		}

		// Token: 0x04001D1E RID: 7454
		private MemoryStream m_memoryStream = new MemoryStream();

		// Token: 0x04001D1F RID: 7455
		private BinaryReader m_binaryReader;

		// Token: 0x04001D20 RID: 7456
		private BinaryWriter m_binaryWriter;

		// Token: 0x04001D21 RID: 7457
		private bool m_write;
	}
