using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// One-off tool to slice the "ChatGPT Image Mar 4, 2026, 10_20_52 AM.png" UI kit
/// sheet into individual sprites by flood-filling connected opaque regions.
/// Run via Tools > Slice UI Kit Sheet.
/// </summary>
public static class UIKitAutoSlicer
{
    const string TargetPath = "Assets/Game/Images/ChatGPT Image Mar 4, 2026, 10_20_52 AM.png";
    const byte AlphaThreshold = 10;
    const byte WhiteThreshold = 245; // pixels with R,G,B all >= this are treated as background
    const int MinArea = 500;
    const int Padding = 3;

    [MenuItem("Tools/Log UI Kit Slices")]
    public static void LogSlices()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetPath);
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
        dataProvider.InitSpriteEditorDataProvider();
        foreach (SpriteRect r in dataProvider.GetSpriteRects())
        {
            Debug.Log($"[UIKitAutoSlicer] {r.name}: x={r.rect.x:0} y={r.rect.y:0} w={r.rect.width:0} h={r.rect.height:0}");
        }
    }

    [MenuItem("Tools/Slice UI Kit Sheet")]
    public static void SliceUIKitSheet()
    {
        var importer = AssetImporter.GetAtPath(TargetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[UIKitAutoSlicer] No TextureImporter at {TargetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.isReadable = true;
        importer.SaveAndReimport();

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetPath);
        if (texture == null)
        {
            Debug.LogError($"[UIKitAutoSlicer] Could not load texture at {TargetPath}");
            return;
        }

        List<RectInt> boxes = FindOpaqueIslands(texture);

        // Reading order: top-to-bottom in ~30px bands, then left-to-right within a band.
        boxes.Sort((a, b) =>
        {
            int rowA = (texture.height - a.yMax) / 30;
            int rowB = (texture.height - b.yMax) / 30;
            if (rowA != rowB) return rowA.CompareTo(rowB);
            return a.xMin.CompareTo(b.xMin);
        });

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = new List<SpriteRect>();
        for (int i = 0; i < boxes.Count; i++)
        {
            RectInt b = boxes[i];
            spriteRects.Add(new SpriteRect
            {
                name = $"UIKit_{i:00}",
                rect = new Rect(b.xMin, b.yMin, b.width, b.height),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero
            });
        }

        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        var assetImporter = dataProvider.targetObject as AssetImporter;
        assetImporter.SaveAndReimport();

        Debug.Log($"[UIKitAutoSlicer] Sliced {spriteRects.Count} sprites from {TargetPath}");
    }

    static List<RectInt> FindOpaqueIslands(Texture2D tex)
    {
        int w = tex.width, h = tex.height;
        Color32[] pixels = tex.GetPixels32();
        var opaque = new bool[w * h];
        int nearWhite = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            bool isBackground = p.a < AlphaThreshold ||
                                 (p.r >= WhiteThreshold && p.g >= WhiteThreshold && p.b >= WhiteThreshold);
            opaque[i] = !isBackground;
            if (isBackground) nearWhite++;
        }
        Debug.Log($"[UIKitAutoSlicer] {nearWhite}/{pixels.Length} pixels classified as background");

        var visited = new bool[w * h];
        var results = new List<RectInt>();
        var stack = new Stack<int>();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;
                if (visited[idx] || !opaque[idx]) continue;

                int minX = x, maxX = x, minY = y, maxY = y, area = 0;
                stack.Push(idx);
                visited[idx] = true;

                while (stack.Count > 0)
                {
                    int cur = stack.Pop();
                    int cx = cur % w, cy = cur / w;
                    area++;
                    if (cx < minX) minX = cx;
                    if (cx > maxX) maxX = cx;
                    if (cy < minY) minY = cy;
                    if (cy > maxY) maxY = cy;

                    TryPush(cx - 1, cy, w, h, opaque, visited, stack);
                    TryPush(cx + 1, cy, w, h, opaque, visited, stack);
                    TryPush(cx, cy - 1, w, h, opaque, visited, stack);
                    TryPush(cx, cy + 1, w, h, opaque, visited, stack);
                    TryPush(cx - 1, cy - 1, w, h, opaque, visited, stack);
                    TryPush(cx + 1, cy - 1, w, h, opaque, visited, stack);
                    TryPush(cx - 1, cy + 1, w, h, opaque, visited, stack);
                    TryPush(cx + 1, cy + 1, w, h, opaque, visited, stack);
                }

                if (area >= MinArea)
                {
                    int rx = Mathf.Max(0, minX - Padding);
                    int ry = Mathf.Max(0, minY - Padding);
                    int rw = Mathf.Min(w, maxX + Padding + 1) - rx;
                    int rh = Mathf.Min(h, maxY + Padding + 1) - ry;
                    results.Add(new RectInt(rx, ry, rw, rh));
                }
            }
        }

        return results;
    }

    static void TryPush(int x, int y, int w, int h, bool[] opaque, bool[] visited, Stack<int> stack)
    {
        if (x < 0 || x >= w || y < 0 || y >= h) return;
        int idx = y * w + x;
        if (visited[idx] || !opaque[idx]) return;
        visited[idx] = true;
        stack.Push(idx);
    }
}
