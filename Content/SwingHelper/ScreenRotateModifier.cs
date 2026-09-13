using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
    public class ScreenRotateModifier : ICameraModifier
    {
        public float Rotation;        // 逻辑状态：当前角
        public float TargetRotation;  // 逻辑状态：目标角
        public float LerpSpeed = 0.1f;

        public bool Finished => Math.Abs(Rotation - TargetRotation) < 0.01f;

        public void Update(ref CameraInfo cameraPosition)
        {
            Rotation = MathHelper.Lerp(Rotation, TargetRotation, LerpSpeed);
        }

        public string UniqueIdentity { get; }

        public void ApplyTo(ref Matrix matrix)
        {
            matrix = Matrix.CreateRotationZ(Rotation) * matrix;
        }
    }
}

