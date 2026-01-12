using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class JoystickTextureCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Joystick Textures")]
    public static void CreateJoystickTextures()
    {
        CreateCircleTexture("JoystickBG", 256, new Color(0.2f, 0.2f, 0.2f, 0.5f), true);
        CreateCircleTexture("JoystickHandle", 128, new Color(0.8f, 0.8f, 0.8f, 0.8f), false);
        
        AssetDatabase.Refresh();
        Debug.Log("Joystick textures created in Assets/Textures/UI/");
    }

    private static void CreateCircleTexture(string name, int size, Color color, bool hasRing)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        float innerRadius = hasRing ? radius * 0.7f : 0f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= radius)
                {
                    if (hasRing && distance > innerRadius)
                    {
                        // Ring area
                        float alpha = Mathf.SmoothStep(0, 1, (radius - distance) / (radius - innerRadius));
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                    }
                    else if (!hasRing)
                    {
                        // Solid circle with anti-aliasing
                        float alpha = Mathf.SmoothStep(0, 1, (radius - distance) / 2f);
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * Mathf.Min(alpha, 1f)));
                    }
                    else
                    {
                        // Transparent center
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        string path = $"Assets/Textures/UI/{name}.png";
        System.IO.Directory.CreateDirectory("Assets/Textures/UI");
        
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        
        AssetDatabase.ImportAsset(path);
        
        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        DestroyImmediate(texture);
    }
}
#endif
