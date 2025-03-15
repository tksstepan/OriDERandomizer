	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;

	public class Archive
	{
		public Archive()
		{
			this.MemoryStream = new MemoryStream();
		}

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

		public void WriteMemoryStreamToBinaryWriter(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((int)this.MemoryStream.Length);
			this.MemoryStream.WriteTo(binaryWriter.BaseStream);
		}

		public void ReadMemoryStreamFromBinaryReader(BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			this.MemoryStream.SetLength((long)num);
			binaryReader.Read(this.MemoryStream.GetBuffer(), 0, num);
		}

		public bool Reading
		{
			get
			{
				return !this.m_write;
			}
		}

		public bool Writing
		{
			get
			{
				return this.m_write;
			}
		}

		public void ResetStream()
		{
			this.MemoryStream.Position = 0L;
		}

		public void WriteMode()
		{
			this.ResetStream();
			this.m_write = true;
		}

		public void ReadMode()
		{
			this.m_memoryStream.Position = 0L;
			this.m_write = false;
		}

		public void Serialize(ref float value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref int value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref bool value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref string value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref Vector2 value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref Vector3 value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref Quaternion value)
		{
			value = this.Serialize(value);
		}

		public void Serialize(ref Dictionary<int,int> value)
		{
			value = this.Serialize(value);
		}

		public float Serialize(float value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadSingle();
		}

		public int Serialize(int value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadInt32();
		}

		public bool Serialize(bool value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadBoolean();
		}

		public string Serialize(string value)
		{
			if (this.m_write)
			{
				this.m_binaryWriter.Write(value);
				return value;
			}
			return this.m_binaryReader.ReadString();
		}

		public Vector2 Serialize(Vector2 value)
		{
			value.x = this.Serialize(value.x);
			value.y = this.Serialize(value.y);
			return value;
		}

		public Vector3 Serialize(Vector3 value)
		{
			value.x = this.Serialize(value.x);
			value.y = this.Serialize(value.y);
			value.z = this.Serialize(value.z);
			return value;
		}

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
				pairs = pairs.TrimEnd(',');

				this.m_binaryWriter.Write(pairs);
				return value;
			}
			value.Clear();
			pairs = this.m_binaryReader.ReadString();
			foreach(string pair in pairs.Split(',')) {
				string[] kandv = pair.Split(':');
				value[int.Parse(kandv[0])] = int.Parse(kandv[1]);							
			}
			return value;
		}

		public void SerializeVersion(ref int version)
		{
		}

		private MemoryStream m_memoryStream = new MemoryStream();

		private BinaryReader m_binaryReader;

		private BinaryWriter m_binaryWriter;

		private bool m_write;
	}
