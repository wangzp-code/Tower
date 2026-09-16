using UnityEditor;
using UnityEngine;

public class IconTextureImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.StartsWith("Assets/Resources/Icons/") || assetPath.StartsWith("Assets/Resources/UI/"))
        {
            var importer = assetImporter as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.alphaIsTransparency = true;
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
            }
        }
    }
}
