using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.SerializedFiles;
using TGASharpLib;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperGUI.Exporters
{
	public class TextureArrayAssetExporter : IAssetExporter
	{
		public static bool ExportTexture(Texture2DArray textureArray, string path)
		{
			for (int i = 0; i < textureArray.Depth; i++)
			{
				byte[] buffer = (byte[])textureArray.GetImageData(i);
				if (buffer == null || buffer.Length == 0)
				{
					return false;
				}

				string filePath = path.Insert(path.LastIndexOf('.'), "_" + i.ToString());

				using (Stream exportStream = FileUtils.Create(filePath))
				using (DirectBitmap bitmap = ConvertToBitmap(textureArray, buffer))
				{
					if (bitmap == null)
					{
						return false;
					}
					else
					{
						// despite the name, this packing works for different formats
						if (textureArray.GfxFormat == GraphicsFormat.RGBA_DXT5_UNorm)
						{
							TextureConverter.UnpackNormal(bitmap.BitsPtr, bitmap.Bits.Length);
						}

						switch (Properties.Settings.Default.ExportImageType)
						{
							case "tga":
								TGA tGA = new TGA(bitmap.DrawingBitmap);
								tGA.Save(exportStream);
								break;

							case "png":
								bitmap.Save(exportStream, ImageFormat.Png);
								break;

							case "bmp":
								bitmap.Save(exportStream, ImageFormat.Bmp);
								break;

							case "jpg":
								bitmap.Save(exportStream, ImageFormat.Jpeg);
								break;
						}
					}
				}
			}

			return true;
		}

		private static DirectBitmap ConvertToBitmap(Texture2DArray texArray, byte[] data)
		{
			switch (texArray.GfxFormat)
			{
				case GraphicsFormat.RGBA_DXT1_SRGB:
					return TextureConverter.DXTTextureToBitmap(texArray.Width, texArray.Height, TextureFormat.DXT1, data);

				case GraphicsFormat.RGBA_DXT5_SRGB:
				case GraphicsFormat.RGBA_DXT5_UNorm:
					return TextureConverter.DXTTextureToBitmap(texArray.Width, texArray.Height, TextureFormat.DXT5, data);

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{texArray.GfxFormat}'");
					return null;
			}
		}

		public bool IsHandle(Object asset, ExportOptions options)
		{
			if (asset.ClassID == ClassIDType.Texture2DArray)
			{
				Texture2DArray texture = (Texture2DArray)asset;
				return texture.IsValidData;
			}
			return true;
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			Texture2DArray texture = (Texture2DArray)asset;
			if (!ExportTexture(texture, path))
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
				return false;
			}

			return true;
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextureArrayExportCollection(this, (Texture2DArray)asset, Properties.Settings.Default.ExportImageType);
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
