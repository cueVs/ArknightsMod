using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace ArknightsMod.Content.Graphics3D;

/// <summary>
/// 3D 绘制工具：相机、矩阵计算
/// 坐标系：Z 增加方向指向屏幕内
/// </summary>
public static class GraphicsUtils
{
    public static Vector2 ScreenResolution => new(Main.screenWidth, Main.screenHeight);
    public static Vector2 ScreenCenter => Main.screenPosition + ScreenResolution / 2;

    public static Vector3 CameraPos(float fov)
        => new(ScreenCenter, CameraZ(fov));

    public static float CameraZ(float fov)
    {
        float viewWidth = Main.screenWidth / Main.Transform.M11;
        float factor = (float)Main.screenHeight / Main.screenWidth;
        return factor * -viewWidth / 2 / MathF.Tan(fov / 2);
    }

    /// <summary>
    /// 获取 View × Projection 矩阵（透视投影）
    /// </summary>
    public static Matrix GetVPMatrix(float fov = MathF.PI / 3f, float near = 10, float far = 5000)
    {
        Vector3 cameraPos = CameraPos(fov);
        Matrix view = Matrix.CreateLookAt(cameraPos, cameraPos + new Vector3(0, 0, 1), Vector3.Down);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(fov, Main.graphics.GraphicsDevice.Viewport.AspectRatio, near, far);

        Matrix grav = Matrix.Identity;
        if (Main.LocalPlayer.gravDir == -1)
            grav = Matrix.CreateScale(1, -1, 1);

        return view * projection * grav;
    }
}
