using UnityEngine;
using UnityEditor;
using System.IO;

public class GlossOnlyPacker
{
    [MenuItem("Tools/Create Gloss Map as Metallic Alpha")]
    static void PackGloss()
    {
        string glossPath = "Assets/_Television_set/Sources/Materials/Tv_3_4_gloss.png";
        Texture2D gloss = AssetDatabase.LoadAssetAtPath<Texture2D>(glossPath);

        int width = gloss.width;
        int height = gloss.height;

        Texture2D packed = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float glossValue = gloss.GetPixel(x, y).grayscale;
                packed.SetPixel(x, y, new Color(0f, 0f, 0f, glossValue)); // Black RGB, gloss in alpha
            }
        }

        packed.Apply();
        byte[] pngData = packed.EncodeToPNG();
        File.WriteAllBytes("Assets/Textures/GlossAsMetallic.png", pngData);
        AssetDatabase.Refresh();

        Debug.Log("Gloss-as-metallic-alpha packed texture created at: Assets/Textures/GlossAsMetallic.png");
    }
}
