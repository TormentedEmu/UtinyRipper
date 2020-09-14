using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public class Texture2DArray : Texture
	{
		public ColorSpace ColorSpace { get; set; }
		public GraphicsFormat GfxFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Depth { get; set; }
		public int MipCount { get; set; }
		public uint DataSize { get; set; }
		public GLTextureSettings TextureSettings;
		public bool IsReadable { get; set; }
		public uint Size { get; set; }
		public StreamingInfo StreamData;

		private Dictionary<int, byte[]> ImagesData;

		public Texture2DArray(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			ColorSpace = (ColorSpace)reader.ReadInt32();
			GfxFormat = (GraphicsFormat)reader.ReadInt32();
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			Depth = reader.ReadInt32();
			ImagesData = new Dictionary<int, byte[]>(Depth);
			MipCount = reader.ReadInt32();
			DataSize = reader.ReadUInt32();
			TextureSettings.Read(reader);
			IsReadable = reader.ReadBoolean();
			reader.AlignStream();
			Size = reader.ReadUInt32();

			if (Size == 0)
			{
				StreamData.Read(reader);
			}
			else
			{
				int imgSize = (int)DataSize / Depth;
				for (int i = 0; i < Depth; i++)
				{
					byte[] data = reader.ReadBytes(imgSize);
					ImagesData.Add(i, data);
				}

				reader.AlignStream();
				byte[] endBytes = reader.ReadBytes(12);
			}
		}

		public byte[] GetImageData(int index)
		{
			if (ImagesData.Count == 0)
			{
				if (StreamData.IsSet)
				{
					byte[] bytes = StreamData.GetContent(File);
					int imgSize = (int)DataSize / Depth;
					int curIndex = 0;
					for (int i = 0; i < Depth; i++)
					{
						byte[] data = new byte[imgSize];
						Array.Copy(bytes, curIndex, data, 0, imgSize);
						curIndex += imgSize;
						ImagesData.Add(i, data);
					}
				}
			}

			if (ImagesData.ContainsKey(index))
			{
				return ImagesData[index];
			}

			return null;
		}

		public bool IsValidData
		{
			get
			{
				if (Size > 0 || StreamData.IsSet)
				{
					return true;
				}

				return ImagesData.Count > 0;
			}
		}

	}
}
