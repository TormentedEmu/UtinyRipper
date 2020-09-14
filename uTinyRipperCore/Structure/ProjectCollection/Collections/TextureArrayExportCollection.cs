using System.IO;
using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Project
{
	public sealed class TextureArrayExportCollection : AssetExportCollection
	{
		string ExportType = "tga";

		public TextureArrayExportCollection(IAssetExporter assetExporter, Texture2DArray asset, string exportType) : base(assetExporter, asset)
		{
			ExportType = exportType;
		}

		public TextureArrayExportCollection(IAssetExporter assetExporter, Texture2DArray asset) :
			base(assetExporter, asset)
		{
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath;
			string fileName;
			string resFileName;

			if (container.TryGetAssetPathFromAssets(Assets, out Object asset, out string assetPath))
			{
				resFileName = Path.GetFileName(assetPath);
			}
			else
			{
				resFileName = Asset.GetOriginalName();
			}

			string subFolder = Asset.ExportPath;
			subPath = Path.Combine(dirPath, subFolder);
			fileName = GetUniqueFileName(subPath, resFileName);
			string assetName = Path.GetFileNameWithoutExtension(fileName);
			subPath = Path.Combine(subPath, assetName);
			if (!DirectoryUtils.Exists(subPath))
			{
				DirectoryUtils.CreateVirtualDirectory(subPath);
			}

			string filePath = Path.Combine(subPath, $"{fileName}.{GetExportExtension(asset)}");
			bool result = ExportInner(container, filePath);
			if (result)
			{
				Meta meta = new Meta(Asset.GUID, CreateImporter(container));
				ExportMeta(container, meta, filePath);
				return true;
			}

			return false;
		}

		protected override string GetExportExtension(Object asset)
		{			
			return ExportType;
		}
	}
}
