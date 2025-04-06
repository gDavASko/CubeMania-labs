using UnityEngine;

public class CubeGenerator: MonoBehaviour
{
    [SerializeField] private MeshFilter _render;
    [SerializeField] private Vector2[] _uvs;
    public void Start()
    { 
        var mesh = Create(11, 16);

        _render.mesh = mesh;
        /*_render.mesh.RecalculateBounds();
        _render.mesh.RecalculateTangents();
        _render.mesh.RecalculateNormals();
        _render.mesh.Optimize();*/
    }

    public Mesh Create(int indexTexture, int texturesInLine)
    {
        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
        };

        var x = indexTexture % texturesInLine;

        var y = texturesInLine / indexTexture;
        var positionInTextureMin = new Vector2(x/(float)texturesInLine, 1 - y/(float)texturesInLine); // это координаты квадратика травы в текстуре, верхний левый угол
        var positionInTextureMax = new Vector2((x+1)/(float)texturesInLine, 1 - (y + 1)/(float)texturesInLine); // это координаты квадратика травы в текстуре, нижний правый угол

        _uvs = new[] 
        {
                new Vector2(positionInTextureMin.x, positionInTextureMin.y), // верхняя левая ближняя вершина
                new Vector2(positionInTextureMax.x, positionInTextureMin.y), // верхняя левая дальняя вершина
                new Vector2(positionInTextureMin.x, positionInTextureMax.y), // верхняя правая ближняя вершина
                new Vector2(positionInTextureMin.x, positionInTextureMax.y), // верхняя правая дальняя вершина

                new Vector2(positionInTextureMax.x, positionInTextureMin.y), // нижняя левая ближняя вершина
                new Vector2(positionInTextureMax.x, positionInTextureMax.y), // нижняя левая дальняя вершина
        };

        mesh.uv = _uvs;

        mesh.triangles = new[] {
                0, 1, 2, // перед
                3, 4, 5,
            };

        mesh.RecalculateNormals();

        return mesh;
    }
}
