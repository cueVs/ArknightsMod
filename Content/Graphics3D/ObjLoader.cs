using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using System.IO;

namespace ArknightsMod.Content.Graphics3D;

public static class ObjLoader
{
    /// <summary>
    /// 从 .obj + .mtl 加载模型，返回带法线的顶点数组
    /// 支持 f v vn 和 f v//vn 格式
    /// </summary>
    public static Vertex3D[] LoadFromMod(string objPath)
    {
        var mod = ModContent.GetInstance<ArknightsMod>();
        string objText;
        using (var reader = new StreamReader(mod.GetFileStream(objPath)))
            objText = reader.ReadToEnd();

        // ---- 加载材质颜色 ----
        var mtlColors = new Dictionary<string, Color>();
        string objDir = Path.GetDirectoryName(objPath)?.Replace('\\', '/') ?? "";
        foreach (var line in objText.Split('\n', '\r'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("mtllib "))
            {
                string mtlPath = objDir + "/" + trimmed[7..].Trim();
                try { LoadMtl(mod.GetFileStream(mtlPath), mtlColors); }
                catch { }
            }
        }

        // ---- 解析顶点、法线、UV、三角面 ----
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var texcoords = new List<Vector2>();
        var verts = new List<Vertex3D>();
        Color currentColor = Color.White;

        foreach (var rawLine in objText.Split('\n', '\r'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v":
                    positions.Add(new(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;

                case "vt":
                    texcoords.Add(new(float.Parse(parts[1]), float.Parse(parts[2])));
                    break;

                case "vn":
                    normals.Add(new(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;

                case "usemtl":
                    if (mtlColors.TryGetValue(parts[1], out var c))
                        currentColor = c;
                    break;

                case "f":
                    // 解析 f v/vt/vn 或 f v//vn 或 f v
                    var indices = new List<(int v, int vt, int vn)>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var sub = parts[i].Split('/');
                        int vi = int.Parse(sub[0]) - 1;
                        int vti = sub.Length > 1 && sub[1].Length > 0 ? int.Parse(sub[1]) - 1 : -1;
                        int vni = sub.Length >= 3 && sub[2].Length > 0 ? int.Parse(sub[2]) - 1 : -1;
                        indices.Add((vi, vti, vni));
                    }
                    // 三角剖分
                    for (int i = 1; i < indices.Count - 1; i++)
                    {
                        AddTriangle(verts, positions, normals, texcoords, indices[0], indices[i], indices[i + 1], currentColor);
                    }
                    break;
            }
        }

        return verts.ToArray();
    }

    private static void AddTriangle(List<Vertex3D> verts, List<Vector3> positions, List<Vector3> normals,
        List<Vector2> texcoords,
        (int v, int vt, int vn) a, (int v, int vt, int vn) b, (int v, int vt, int vn) c, Color color)
    {
        Vector3 na = default, nb = default, nc = default;
        if (a.vn >= 0 && a.vn < normals.Count) na = normals[a.vn];
        if (b.vn >= 0 && b.vn < normals.Count) nb = normals[b.vn];
        if (c.vn >= 0 && c.vn < normals.Count) nc = normals[c.vn];

        Vector2 ta = Vector2.Zero, tb = Vector2.Zero, tc = Vector2.Zero;
        if (a.vt >= 0 && a.vt < texcoords.Count) ta = texcoords[a.vt];
        if (b.vt >= 0 && b.vt < texcoords.Count) tb = texcoords[b.vt];
        if (c.vt >= 0 && c.vt < texcoords.Count) tc = texcoords[c.vt];

        verts.Add(new(positions[a.v], color, na) { TexCoord = ta });
        verts.Add(new(positions[b.v], color, nb) { TexCoord = tb });
        verts.Add(new(positions[c.v], color, nc) { TexCoord = tc });
    }

    private static void LoadMtl(Stream stream, Dictionary<string, Color> colors)
    {
        using var reader = new StreamReader(stream);
        string currentMtl = "";
        foreach (var rawLine in reader.ReadToEnd().Split('\n', '\r'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            if (parts[0] == "newmtl")
                currentMtl = parts[1];
            else if (parts[0] == "Kd" && parts.Length >= 4 && currentMtl != "")
                colors[currentMtl] = new Color(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
        }
    }
}
