using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class TexturePacker : MonoBehaviour
{
    [Tooltip("Список текстур для объединения")]
    public List<Texture2D> texturesToPack;
    
    [Tooltip("Ширина сетки (количество текстур по горизонтали)")]
    public int gridWidth = 4;
    
    [Tooltip("Высота сетки (количество текстур по вертикали)")]
    public int gridHeight = 4;
    
    [Tooltip("Разрешение выходной текстуры (должно быть степенью двойки)")]
    public int outputResolution = 1024;
    
    [Tooltip("Путь для сохранения текстуры (относительно папки Assets)")]
    public string savePath = "Textures/PackedTexture.png";
    
    [Tooltip("Размер каждой текстуры на выходной текстуре")]
    public Vector2 textureSize = new Vector2(128, 128);
    
    [ContextMenu("Pack Textures")]
    public void PackTextures()
    {
        if (texturesToPack == null || texturesToPack.Count == 0)
        {
            Debug.LogError("Список текстур пуст!");
            return;
        }
        
        // Проверка разрешения на степень двойки
        if (!IsPowerOfTwo(outputResolution))
        {
            Debug.LogError("Разрешение должно быть степенью двойки (128, 256, 512, 1024, 2048, 4096)!");
            return;
        }
        
        // Создаем текстуру для объединенных текстур
        Texture2D packedTexture = new Texture2D(outputResolution, outputResolution, TextureFormat.RGBA32, false);
        
        // Заполняем текстуру прозрачным цветом
        Color[] clearColors = new Color[outputResolution * outputResolution];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }
        packedTexture.SetPixels(clearColors);
        
        int textureIndex = 0;
        
        // Размещаем текстуры в сетке
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (textureIndex >= texturesToPack.Count)
                    break;
                
                Texture2D texture = texturesToPack[textureIndex];
                if (texture != null)
                {
                    PlaceTextureOnTexture(packedTexture, texture, x, y);

                    Debug.LogError($"Place texture {texture.name} at {x}, {y}");
                }
                
                textureIndex++;
            }
        }
        
        // Применяем изменения к текстуре
        packedTexture.Apply();
        
        // Сохраняем текстуру как PNG файл
        SaveTextureAsPNG(packedTexture, savePath);
        
        Debug.Log($"Текстуры успешно объединены и сохранены по пути: {savePath}");
    }
    
    private void PlaceTextureOnTexture(Texture2D targetTexture, Texture2D sourceTexture, int gridX, int gridY)
    {
        // Создаем новую текстуру для масштабирования
        Texture2D scaledTexture = ScaleTexture(sourceTexture, (int)textureSize.x, (int)textureSize.y);
        
        // Вычисляем позицию в сетке
        int startX = (int)(gridX * (textureSize.x + 1));
        int startY = targetTexture.height - (int)((gridY + 1) * (textureSize.y + 1));
        
        // Копируем пиксели масштабированной текстуры
        for (int y = 0; y < scaledTexture.height; y++)
        {
            for (int x = 0; x < scaledTexture.width; x++)
            {
                if (startX + x >= targetTexture.width || startY + y >= targetTexture.height || startY + y < 0)
                    continue;
                
                Color pixelColor = scaledTexture.GetPixel(x, y);
                targetTexture.SetPixel(startX + x, startY + y, pixelColor);
            }
        }
    }
    
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        // Создаем новую текстуру нужного размера
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        
        // Масштабирование с билинейной интерполяцией
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // Нормализованные координаты
                float normalizedX = x / (float)targetWidth;
                float normalizedY = y / (float)targetHeight;
                
                // Получаем пиксель с билинейной интерполяцией
                Color pixelColor = source.GetPixelBilinear(normalizedX, normalizedY);
                result.SetPixel(x, y, pixelColor);
            }
        }
        
        result.Apply();
        return result;
    }
    
    private void SaveTextureAsPNG(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath, filePath);
        
        // Создаем директорию, если её нет
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        
        File.WriteAllBytes(fullPath, bytes);
        
        // Обновляем ассеты в редакторе
        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    
    private bool IsPowerOfTwo(int x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }
}