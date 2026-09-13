using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent;
using Filters = Terraria.Graphics.Effects.Filters;

// TODO : 提供屏幕朝向震动以及卡肉等攻击效果 制作被打出血的粒子特效
/* TODO : 刀砍快慢以及后摇啥的 提供多种情况AI 通过链式调用来调整整体的状态机 例如 于Move.State 点击左键 进入Attack.State 然后
          在Attack.State中用右键或者特殊按键的时候 切换为特殊State  */
namespace ArknightsMod.Content.SwingHelper
{
    public class SwingHelper
    {
        public SwingHelper(int index,int catmullScale,bool isBackArm = false)
        {
            SwingUseTime = index;
            StabUseTime = index;
            CatmullScale = catmullScale;
            this.index = index;
            dashMaxTime = index;
            oldPos = new Vector2[index];
            oldHandPos = new Vector2[index];
            oldWorldPos = new Vector2[index];
            oldWorldHandPos = new Vector2[index];
            oldRot = new float[index];
            oldPlayerPos = new  Vector2[index];
            this.isBackArm = isBackArm;
        }
        #region 提供的着色器资源

        public static Texture2D lightTex = TextureAssets.Extra[98].Value;
        /// <summary>
        /// 消融
        /// </summary>
        public static Effect Dissolve;
        /// <summary>
        /// 内流动
        /// </summary>
        public static Effect Flow;
        /// <summary>
        /// 噪声流动
        /// </summary>
        /// <summary>
        /// 赛博闪烁效果
        /// </summary>
        public static Effect BladeFlicker;
        /// <summary>
        /// /屏幕扭曲效果
        /// </summary>
        public static Asset<Effect> BladeWarp;
        /// <summary>
        /// 沿刀光轨迹的局部空间扭曲
        /// </summary>
        public static Asset<Effect> BladeSlashWarp;
        /// <summary>
        /// 水墨晕染效果
        /// </summary>
        public static Effect BladeInk;
        /// <summary>
        /// 测试用UuU
        /// </summary>
        public static Effect TestFX;
		/// <summary>
		/// 像素化着色器
		/// </summary>
        public static Effect Pixelate;
		/// <summary>
		/// 噪声流动着色器
		/// </summary>
        public static Effect NoiseTrail;


	        public enum SwingEffect
	        {
	        Zero,
	        Dissolve,
	        Flow,
	        Flicker,
	        Warp,
	        SlashWarp,
	        Ink,
	        Test
        }

        public enum TripTex
        {
	        Streamline,
	        Afterimage
        }
        #endregion

        #region 基础的字段及属性
        /// <summary>
        /// 是否为后手
        /// </summary>
        public bool isBackArm;
        /// <summary>
        /// 绑定的弹幕
        /// </summary>
        public Projectile proj;
        /// <summary>
        /// 弹幕贴图
        /// </summary>
        public Texture2D projTexture;
		/// <summary>
		/// 挥舞的额外运行方法
		/// </summary>
        public Action swingAction;
		/// <summary>
		/// 冲刺的额外运行方法
		/// </summary>
        public Action dashAction;
		/// <summary>
		/// 戳刺的额外运行方法
		/// </summary>
        public Action stabAction;
		/// <summary>
		/// 等待的额外运行方法
		/// </summary>
        public Action waitAction;
        /// <summary>
        /// 贴图大小
        /// </summary>
        public Vector2 texSize => projTexture.Size();
        /// <summary>
        /// 大小倍率
        /// </summary>
        public Vector2 scale = new Vector2(1,1);
        /// <summary>
        /// 剑体长度
        /// </summary>
        public float Length => (texSize).Length();

        public Vector2 texLength => new Vector2(Length, 0);
        public Vector2 handleLength;
        public Vector2 swordLength;

        /// <summary>
        /// 实际剑方向（SwordAHandCon 算好的，DrawBlade 用它对齐柄）
        /// </summary>
        public float swordRot;

        /// <summary>
        /// 绑定的玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 卡肉时间
        /// </summary>
        public float lagTime;
        /// <summary>
        /// 鼠标弧度
        /// </summary>
        public float mouseRad;

        private float startRad;
        private float endRad;
        /// <summary>
        /// 手部弧度
        /// </summary>
        public float armRad;
        /// <summary>
        /// 挥舞总弧度
        /// </summary>
        public float swingRad;
		/// <summary>
		/// 剑朝向
		/// </summary>
        public float swordDir;
		/// <summary>
		/// 当前冲刺时间
		/// </summary>
        public float dashTime;
		/// <summary>
		/// 冲刺所需时间
		/// </summary>
        public float dashMaxTime;
		/// <summary>
		/// 冲刺方向
		/// </summary>
        public float dashRad;
		/// <summary>
		/// 戳刺方向
		/// </summary>
        public float stabRad;
        /// <summary>
        /// 剑柄位置
        /// </summary>
        public Vector2 setoff;
		/// <summary>
		/// 剑柄位置
		/// </summary>
        public Vector2 oldSetoff;
        /// <summary>
        /// 剑体所使用的顶点结构体
        /// </summary>
        public List<Vertex> sword = new List<Vertex>();
        public List<Vertex> trip = new List<Vertex>();
        public Vector2[] swordPos_Draw = new Vector2[4];
        /// <summary>
        /// 剑与手的角度 默认为二者垂直
        /// </summary>
        public float swordRad = MathF.PI / 2;
        /// <summary>
        /// 记录次数/各类动作使用时间
        /// </summary>
        private int index;

        public Vector2 swordScale = new Vector2(1, 1);  // X=剑身长，Y=剑身宽
		/// <summary>
		/// 挥舞所需时间
		/// </summary>
        public int SwingUseTime;
		/// <summary>
		/// 戳刺所需时间
		/// </summary>
		public int StabUseTime;
        /// <summary>
        /// 细分曲线倍数
        /// </summary>
        private int CatmullScale;
        //保存的old信息 -- 剑尖位置 旋转值 手位置 (以及两个世界位置)
        private Vector2[] oldPos;
        private float[] oldRot;
        private Vector2[] oldHandPos;
        private Vector2[] oldWorldPos;
        private Vector2[] oldWorldHandPos;

        private Vector2[] oldPlayerPos;
        #endregion

        #region 专门给部分方法所提供的字段及属性
        /// <summary>
        /// 挥砍时间
        /// </summary>
        public int swingTime;
        /// <summary>
        /// 戳刺时间
        /// </summary>
        public int stabTime;
        /// <summary>
        /// 行进速率
        /// </summary>
        public float walkPhase;
        #endregion

        #region 工具变量
        /// <summary>
        /// 剑柄位置
        /// </summary>
        public Vector2 handlePos;
		/// <summary>
		/// 剑体尾部
		/// </summary>
        public Vector2 swordEnd;
		/// <summary>
		/// 剑体首部
		/// </summary>
        public Vector2 swordHead;
        /// <summary>
        /// 剑尖位置
        /// </summary>
        public Vector2 swordPos;
        /// <summary>
        /// 进度
        /// </summary>
        public float progress;
        /// <summary>
        /// 玩家速度
        /// </summary>
        public float playerX;
        #endregion

        #region 蓄力所需字段
        /// <summary>
        /// 当前蓄力时间
        /// </summary>
        public float Chargetime;
        /// <summary>
        /// 最大蓄力时间
        /// </summary>
        public float MaxChargetime;
        /// <summary>
        /// 最大蓄力比例倍率
        /// </summary>
        public float MaxChargeScale_size;
        /// <summary>
        /// 最大蓄力伤害倍率
        /// </summary>
        public float MaxChargeScale_damage;
        /// <summary>
        /// 蓄力比例
        /// </summary>
        public float ChargeProgress;
        #endregion

        #region 设置挥舞帮助的参数

        public SwingHelper SetSetoff(Vector2 setoff) {
	        this.setoff = setoff;
	        oldSetoff = setoff;
	        return this;
        }

        public SwingHelper SetSwingAction(Action action) {
	        swingAction = action;
	        return this;
        }

        public SwingHelper SetDachAction(Action action) {
	        dashAction = action;
	        return this;
        }

        public SwingHelper SetWaitAction(Action action) {
	        waitAction = action;
	        return this;
        }

        public SwingHelper SetStabAction(Action action) {
	        stabAction = action;
	        return this;
        }
        public SwingHelper SetTex(Texture2D tex)
        {
            projTexture = tex;
            return this;
        }
        public SwingHelper SetPlayer(Player player)
        {
            this.player = player;
            mP = player.GetModPlayer<ModifyScreenPosPlayer>();
            return this;
        }
        public SwingHelper SetProj(Projectile proj)
        {
            this.proj = proj;
            return this;
        }
        public SwingHelper SetSwingRad(float rad)
        {
            this.swingRad = rad;
            return this;
        }

        public SwingHelper SetScale(Vector2 scale)
        {
            // scale 只表示缩放比例，不能携带方向；负值会把剑轴镜像到另一侧。
            this.scale = new Vector2(MathF.Abs(scale.X), MathF.Abs(scale.Y));
            if (this.scale.X < 0.0001f)
                this.scale.X = 1f;
            if (this.scale.Y < 0.0001f)
                this.scale.Y = 1f;
            return this;
        }
        public SwingHelper SetcatmullScale(int catmullScale)
        {
            this.CatmullScale = catmullScale;
            return this;
        }

        public SwingHelper SetIndex(int index)
        {
            SwingUseTime = index;
            this.index = index;
            dashMaxTime = index;
            oldPos = new Vector2[index];
            oldHandPos = new Vector2[index];
            oldWorldPos = new Vector2[index];
            oldWorldHandPos = new Vector2[index];
            oldRot = new float[index];
            oldPlayerPos = new Vector2[index];
            return this;
        }

        public SwingHelper ReloadIndex()
        {
            oldPos = new Vector2[index];
            oldHandPos = new Vector2[index];
            oldWorldPos = new Vector2[index];
            oldWorldHandPos = new Vector2[index];
            oldRot = new float[index];
            oldPlayerPos = new Vector2[index];
            return this;
        }

        public SwingHelper ResetTime(int lagtime = 0) {
	        dashTime = 0;
	        swingTime = 0;
	        lagTime = lagtime;
	        stabTime = 0;
	        Chargetime = 0;
	        return this;
        }
        /// <summary>
        /// 保存鼠标朝向
        /// </summary>
        /// <param name="rad"></param>
        public SwingHelper PointMouseRad(float rad)
        {
	        // 非均匀缩放以本次攻击鼠标方向为局部坐标轴，而不是固定世界 X/Y 轴。
	        mouseRad = rad;
	        float half = swingRad / 2f;
	        if (player.direction == 1)
	        {
		        startRad = rad - half;   // 朝右：身后(-90°) → 前方(+90°)
		        endRad   = rad + half;
	        }
	        else
	        {
		        startRad = rad + half;   // 朝左：身后(+90°) → 前方(-90°)
		        endRad   = rad - half;
	        }
	        if (endRad > MathF.PI * 2f)
	        {
		        endRad -= MathF.PI * 2f;
		        startRad -= MathF.PI * 2f;
	        }
	        if (startRad > endRad)
		        startRad -= MathF.PI * 2f;
	        return this;
        }

        public SwingHelper SetDashRad(float rad) {
	        dashRad = rad;
	        return this;
        }

        public SwingHelper SetStabRad(float rad) {
	        stabRad = rad;
	        return this;
        }
        #endregion

        private Matrix uTransform = Matrix.CreateOrthographicOffCenter(
            0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        private int i = 0;

        public Color GetDominantColor(Texture2D texture) {
	        Color[] pixels = new Color[texture.Width * texture.Height];
	        texture.GetData(pixels);

	        //将颜色存放到字典里面
	        Dictionary<Color, int> colorFrequency = new Dictionary<Color, int>();

	        foreach (Color pixel in pixels) {
		        if (pixel.A > 100) {
			        //RGB颜色为16进制 用这个算法去除重复成分
			        Color simplified = new Color(
				        (byte)(pixel.R / 16 * 16),
				        (byte)(pixel.G / 16 * 16),
				        (byte)(pixel.B / 16 * 16)
			        );

			        if (colorFrequency.ContainsKey(simplified))
				        colorFrequency[simplified]++;
			        else
				        colorFrequency[simplified] = 1;
		        }
	        }

	        // 返回字典里面最多的
	        return colorFrequency.OrderByDescending(x => x.Value).First().Key;
        }

        #region 动作方法
		/// <summary>
        /// 移动时剑的AI
        /// </summary>
        public virtual void Move() {
	        swordDir = 1;
            playerX = Math.Abs(player.velocity.X);
            if (Math.Abs(player.velocity.Y) > 0.01f) {
                armRad = MathHelper.ToRadians(-20f);
                walkPhase = 0f;
            }
            else if (playerX > 0.1f) {
                walkPhase += 0.12f + playerX * 0.04f;
                progress = (MathF.Sin(walkPhase) + 1f) * 0.5f;
                armRad = MathHelper.ToRadians(MathHelper.Lerp(50f, -20f, progress));
            }
            else {
                walkPhase = 0f;
                armRad = 0f;
            }
            SwordAHandCon(-MathF.PI/2f * player.direction, armRad + MathF.PI/2f *  player.direction,texLength.Length(),handleLength.Length(),swordLength.Length());
            proj.rotation = armRad * -player.direction;
        }

        /// <summary>
        /// 蓄力/等待时剑AI
        /// </summary>
        public virtual bool Wait(RotationHelper.SwingDir swingDir = RotationHelper.SwingDir.plus,float swordtohand = 0)
        {
            swordRad = RotationHelper.GetSwingRotation(startRad, endRad,swingTime,SwingUseTime,player.direction,scale
	            ,texLength,handleLength,swordLength,out float length,out float handlelen,out float swordlen,
	            out float SwordDir);
            swordDir = SwordDir;
            SwordAHandCon(swordtohand, startRad,length,handlelen,swordlen);
            Chargetime = MathF.Min(Chargetime+1, MaxChargetime);
            ChargeProgress = Chargetime / MaxChargetime;
            if (ChargeProgress >= 1)
	            return true;
            return false;
        }

        public virtual bool Stab(float swordtohand = 0) {
	        scale = new Vector2(1, 1);
	        if (stabTime / (float)StabUseTime < 0.3f) {
		        swordRad = RotationHelper.GetSwingRotation(stabRad, stabRad,swingTime,SwingUseTime,player.direction,scale
			        ,texLength,handleLength,swordLength,out float length,out float handlelen,out float swordlen,
			        out float SwordDir);
		        swordDir = SwordDir;
		        SwordAHandCon(swordtohand,swordRad,length,handlelen,swordlen,true,Player.CompositeArmStretchAmount.None,false);
	        }
	        else {
		        swordRad = RotationHelper.GetSwingRotation(stabRad, stabRad,swingTime,SwingUseTime,player.direction,scale
			        ,texLength,handleLength,swordLength,out float length,out float handlelen,out float swordlen,
			        out float SwordDir);
		        swordDir = SwordDir;
		        SwordAHandCon(swordtohand,swordRad,length,handlelen,swordlen,true,Player.CompositeArmStretchAmount.Full,false);
	        }
	        setoff = (1-(stabTime / (float)StabUseTime)) * (oldSetoff * 2);
	        stabAction?.Invoke();
	        if (stabTime / StabUseTime >= 1f) {
		        setoff = oldSetoff;
		        return true;
	        }

	        stabTime++;
	        return false;
        }

        /// <summary>
        /// 基础挥砍
        /// </summary>
        public virtual bool Swing(RotationHelper.SwingDir swingDir = RotationHelper.SwingDir.plus,float swordtohand = 0)
        {
            if (swingTime > SwingUseTime)
            {
	            if (Filters.Scene["BladeWarp"].IsActive())
		            Filters.Scene["BladeWarp"].Deactivate();
	            if (Filters.Scene["BladeSlashWarp"].IsActive())
		            Filters.Scene["BladeSlashWarp"].Deactivate();
	            return true;
            }
            swordRad = RotationHelper.GetSwingRotation(startRad, endRad,swingTime,SwingUseTime,player.direction,scale
	            ,texLength,handleLength,swordLength,out float length,out float handlelen,out float swordlen,
	            out float SwordDir,swingDir);
            swordDir = SwordDir;
            swingAction?.Invoke();
            SwordAHandCon(swordtohand,swordRad,length,handlelen,swordlen,true,Player.CompositeArmStretchAmount.Full,true);
            if (lagTime == 0)
	            swingTime++;
            else
	            lagTime--;
            return false;
        }

        public bool Dash(float Length,RotationHelper.SwingDir swingDir = RotationHelper.SwingDir.plus) {
	        swordRad = RotationHelper.GetSwingRotation(startRad, endRad,swingTime,SwingUseTime,player.direction,scale
		        ,texLength,handleLength,swordLength,out float length,out float handlelen,out float swordlen,
		        out float SwordDir,swingDir);
	        SwordAHandCon(0,swordRad,length,handlelen,swordlen,false,Player.CompositeArmStretchAmount.Full,true);
	        SavePlayerPos(player.position + new Vector2(0f, player.gfxOffY));
	        dashAction?.Invoke();
	        Vector2 vector2 = new Vector2(Length, 0).RotatedBy(dashRad);
	        vector2 /= dashMaxTime;
	        player.velocity += vector2;
	        dashTime++;
	        if (dashTime >= dashMaxTime)
		        return true;
	        return false;
        }

        /// <summary>
        /// 剑尖自动跟随对应点位
        /// </summary>
        /// <param name="swordToHand"></param>
        /// <param name="hand"></param>
        /// <param name="handPlayerDir"></param>
        public virtual void SwordAHandCon(float swordToHand,float hand,float length,float handleLen,float swordlen,
	        bool savePos = false,Player.CompositeArmStretchAmount armType = Player.CompositeArmStretchAmount.Full,bool handPlayerDir = true)
        {
            Vector2 ScaleByAttackAxis(Vector2 vector)
            {
                return (vector.RotatedBy(-mouseRad) * scale).RotatedBy(mouseRad);
            }

            // 手臂和剑身必须共用同一变换，否则缩放后会出现手与剑方向不一致。
            Vector2 handDirection = ScaleByAttackAxis(Vector2.UnitX.RotatedBy(hand));
            if (handDirection.LengthSquared() < 0.0001f)
                handDirection = Vector2.UnitX.RotatedBy(hand);
            float visualHandRotation = handDirection.ToRotation();

            float rawSwordRotation = hand + swordToHand;
            if (handPlayerDir && player.direction == -1)
                rawSwordRotation += MathF.PI;
            Vector2 swordDirection = ScaleByAttackAxis(Vector2.UnitX.RotatedBy(rawSwordRotation));
            if (swordDirection.LengthSquared() < 0.0001f)
                swordDirection = Vector2.UnitX.RotatedBy(rawSwordRotation);
            float visualSwordRotation = swordDirection.ToRotation();

            float armAngle;
            if(handPlayerDir)
                armAngle = visualHandRotation - MathF.PI / 2f * player.direction;
            else
                armAngle = visualHandRotation - MathF.PI / 2f;
            if (isBackArm) {
	            player.SetCompositeArmBack(true, armType, armAngle);
	            handlePos = player.GetBackHandPosition(armType, armAngle);
            }
            else {
	            player.SetCompositeArmFront(true, armType, armAngle);
	            handlePos = player.GetFrontHandPosition(armType, armAngle);
	            player.heldProj = proj.whoAmI;
            }

            swordPos = handlePos + swordDirection * length;
            swordEnd = handlePos + swordDirection * handleLen;
            swordHead = handlePos + swordDirection * swordlen;
            proj.Center = swordPos;
            swordRot = visualSwordRotation;
            if(savePos)
                SavePos(swordHead, swordRot, swordEnd);
        }

        #endregion

        #region 材质选择
		/// <summary>
        /// 水墨蔓延半径（Ink 效果用，每帧涨大）
        /// </summary>
        private float inkSpread;
        private float time => (float)Main.timeForVisualEffects * 0.05f;

        public void ApplyShader(SwingEffect effect) {
	        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
	        projection = Main.GameViewMatrix.ZoomMatrix * projection;

	        switch (effect) {
		        case SwingEffect.Zero:
			        break;
		        case SwingEffect.Dissolve:
			        Dissolve.Parameters["uTransform"].SetValue(projection);
			        Dissolve.Parameters["uTime"].SetValue(time);
			        Dissolve.Parameters["uDissolve"].SetValue(0.3f);
			        Dissolve.Parameters["uNoiseScale"].SetValue(2.0f);
			        Main.graphics.GraphicsDevice.Textures[1] = ModContent
				        .Request<Texture2D>("ArknightsMod/Content/SwingHelper/Images/Hz").Value;
			        Dissolve.CurrentTechnique.Passes[0].Apply();
			        break;
		        case SwingEffect.Flow:
			        Flow.Parameters["uTransform"].SetValue(projection);
			        Flow.Parameters["uTime"].SetValue(time);
			        Flow.Parameters["uFlowSpeed"].SetValue(0.5f); // 流动速度
			        // slot 1 = 流动贴图（uImage1），slot 0 由外层设为蒙版
			        Main.graphics.GraphicsDevice.Textures[1] = ModContent
				        .Request<Texture2D>("ArknightsMod/Content/SwingHelper/Images/Lava").Value;
			        Flow.CurrentTechnique.Passes[0].Apply();
			        break;
		        case SwingEffect.Flicker:
			        BladeFlicker.Parameters["uTransform"].SetValue(projection);
			        BladeFlicker.Parameters["uTime"].SetValue((float)Main.GlobalTimeWrappedHourly);
			        BladeFlicker.Parameters["uIntensity"].SetValue(0.5f);
			        BladeFlicker.CurrentTechnique.Passes[0].Apply();
			        break;
		        case SwingEffect.Ink:
			        BladeInk.Parameters["uTransform"].SetValue(projection);
			        BladeInk.Parameters["uTime"].SetValue((float)Main.GlobalTimeWrappedHourly);


			        BladeInk.Parameters["uInkColor"].SetValue(new Vector3(0.12f, 0.28f, 0.22f));

			        BladeInk.Parameters["uWashColor"].SetValue(new Vector3(0.85f, 0.95f, 0.85f));

			        BladeInk.Parameters["uAccentColor"].SetValue(new Vector3(0.90f, 0.20f, 0.20f));

			        BladeInk.Parameters["uSpreadPos"].SetValue(new Vector2(0f, 0.5f));
			        inkSpread += 0.02f;
			        if (inkSpread > 1.4f)
				        inkSpread = 0f;
			        BladeInk.Parameters["uSpreadRadius"].SetValue(inkSpread);
			        BladeInk.Parameters["uDry"].SetValue(Math.Min(inkSpread * 0.5f, 0.8f)); // 内部逐渐干涸

			        BladeInk.CurrentTechnique.Passes[0].Apply();
			        break;
		        case SwingEffect.Warp:
			        // 滤镜已在 Load 注册（EffectLoad.cs），这里只需激活
			        var warpShader = Filters.Scene["BladeWarp"].GetShader();
			        warpShader.UseOpacity(0.7f); // 整体强度
			        var center = (swordPos - Main.screenPosition)
			                     / new Vector2(Main.screenWidth, Main.screenHeight);
			        warpShader.Shader.Parameters["uCenter"].SetValue(center); // 扭曲中心（归一化 UV）
			        warpShader.Shader.Parameters["uRadius"].SetValue(0.18f); // 半径
			        warpShader.Shader.Parameters["uStrength"].SetValue(0.02f); // 扭曲量
			        Filters.Scene.Activate("BladeWarp", Main.LocalPlayer.Center);
			        break;
		        case SwingEffect.SlashWarp:
			        // 将刀光历史位置转换为屏幕 UV，Shader 对这些刀身截面取并集。
			        var slashShader = Filters.Scene["BladeSlashWarp"].GetShader();
			        int segmentCount = Math.Min(index, 16);
			        Vector4[] segments = new Vector4[16];
			        Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
			        for (int n = 0; n < segmentCount; n++)
			        {
				        Vector2 hand = oldHandPos[n] / screenSize;
				        Vector2 tip = oldPos[n] / screenSize;
				        segments[n] = new Vector4(hand.X, hand.Y, tip.X, tip.Y);
			        }
			        slashShader.Shader.Parameters["uSegments"].SetValue(segments);
			        slashShader.Shader.Parameters["uSegmentCount"].SetValue(segmentCount);
			        slashShader.Shader.Parameters["uWidth"].SetValue(32f / Main.screenHeight);
			        slashShader.Shader.Parameters["uStrength"].SetValue(0.018f);
			        slashShader.Shader.Parameters["uChromatic"].SetValue(0.006f);
			        slashShader.UseOpacity(0.9f);
			        Filters.Scene.Activate("BladeSlashWarp", Main.LocalPlayer.Center);
			        break;
		        case SwingEffect.Test:
			        var fx = TestFX;
			        fx.Parameters["uTransform"].SetValue(projection);
			        fx.CurrentTechnique.Passes["Base"].Apply();
			        break;
	        }
        }

        public void TexChance(TripTex tex) {
	        switch (tex) {
		        case TripTex.Afterimage:
			        Main.graphics.GraphicsDevice.Textures[0] = ModContent
				        .Request<Texture2D>("ArknightsMod/Content/SwingHelper/Images/Extra_209").Value;
			        break;
		        case TripTex.Streamline:
			        Main.graphics.GraphicsDevice.Textures[0] = ModContent
				        .Request<Texture2D>("ArknightsMod/Content/SwingHelper/Images/SlashTex").Value;
			        break;
	        }//贴图选择
        }
        #endregion

        #region 绘制
		/// <summary>
        /// 绘制剑体
        /// </summary>
        public virtual void DrawBlade(SpriteBatch sb, bool handPlayerDir = true)
        {
	        Vector2 Length = swordPos - handlePos;
	        Vector2 handPos = handlePos + setoff.RotatedBy(swordRot);
	        handPos -= Main.screenPosition;
	        Vector2 halfPos = Length / 2f;
	        if (scale.X != 1)
		        halfPos = halfPos.RotatedBy(TransformHelper.CalculateTiltAngle(projTexture, scale.X));
	        else
		        halfPos = halfPos.RotatedBy(TransformHelper.CalculateTiltAngle(projTexture, scale.Y));
	        Vector2 halfWidth = new Vector2(-halfPos.Y, halfPos.X);
	        swordPos_Draw =
	        [
		        handPos + halfPos - halfWidth * player.direction * swordDir,   //左上
		        handPos + Length,                 //右上
		        handPos,                          //左下
		        handPos + halfPos + halfWidth * player.direction * swordDir  //右下
	        ];
	        sword.Clear();
	        for (int i = 0; i < 4; i++)
		        sword.Add(default);
	        {
		        sword[0] = new Vertex(swordPos_Draw[0], new Vector3(0, 0, 0), Color.White);
		        sword[1] = new Vertex(swordPos_Draw[2], new Vector3(0, 1, 0), Color.White);
		        sword[2] = new Vertex(swordPos_Draw[1], new Vector3(1, 0, 0), Color.White);
		        sword[3] = new Vertex(swordPos_Draw[3], new Vector3(1, 1, 0), Color.White);
	        }
	        sb.End();
	        sb.Begin(
		        SpriteSortMode.Immediate,
		        BlendState.AlphaBlend,
		        SamplerState.AnisotropicClamp,
		        DepthStencilState.None,
		        RasterizerState.CullNone,
		        null,
		        Main.GameViewMatrix.TransformationMatrix
	        );
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        Main.graphics.GraphicsDevice.Textures[0] = projTexture;
	        if (sword.Count >= 4)
		        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, sword.ToArray(), 0,
			        sword.Count - 2);
	        sb.End();
	        sb.Begin();
        }

        public virtual void DrawStabLight(SpriteBatch sb,Color lightcolor,Vector2 scale ) {
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        Vector2 orig = lightTex.Size() / 2;

	        sb.Draw(lightTex,swordHead - Main.screenPosition,null,lightcolor,swordRot + MathF.PI/2
		        ,lightTex.Size()/2,scale ,SpriteEffects.None,0);
	        sb.End();
	        sb.Begin();
        }

        /// <summary>
        /// 绘制刀光拖尾
        /// </summary>
        public virtual void DrawTrip(SwingEffect en, Color Tripcolor, SpriteBatch sb,TripTex tex = TripTex.Streamline)
        {
	        trip.Clear();
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length-1; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue ;
			        float progress = i / (float)TriphandPos.Length;
			        trip.Add(new Vertex(TriphandPos[i] - Main.screenPosition, new Vector3(progress, 0, 0), Tripcolor));
			        trip.Add(new Vertex(TripswordPos[i] - Main.screenPosition, new Vector3(progress, 1, 0), Tripcolor));
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length-1; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue ;
			        float progress = i / (float)TriphandPos.Length;
			        trip.Add(new Vertex(TriphandPos[i] , new Vector3(progress, 0, 0), Tripcolor));
			        trip.Add(new Vertex(TripswordPos[i], new Vector3(progress, 1, 0), Tripcolor));
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        ApplyShader(en);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        TexChance(tex);
	        if (trip.Count >= 3)
		        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, trip.ToArray(), 0,
			        trip.Count - 2);
	        sb.End();
	        sb.Begin();
        }
		/// <summary>
		/// 绘制弯刀拖尾
		/// </summary>
		/// <param name="en"></param>
		/// <param name="Tripcolor"></param>
		/// <param name="sb"></param>
        public virtual void DrawTrip(SwingEffect en, Color[] Tripcolor, SpriteBatch sb,TripTex tex =  TripTex.Streamline)
        {
	        trip.Clear();
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length-1; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue ;
			        float progress = i / (float)TriphandPos.Length;
			        trip.Add(new Vertex(TriphandPos[i] - Main.screenPosition, new Vector3(progress, 0, 0), Tripcolor[0]));
			        trip.Add(new Vertex(TripswordPos[i] - Main.screenPosition, new Vector3(progress, 1, 0), Tripcolor[1]));
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length-1; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue ;
			        float progress = i / (float)TriphandPos.Length;
			        trip.Add(new Vertex(TriphandPos[i] , new Vector3(progress, 0, 0), Tripcolor[0]));
			        trip.Add(new Vertex(TripswordPos[i], new Vector3(progress, 1, 0), Tripcolor[1]));
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        ApplyShader(en);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        TexChance(tex);
	        if (trip.Count >= 3)
		        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, trip.ToArray(), 0,
			        trip.Count - 2);
	        sb.End();
	        sb.Begin();
        }

        public virtual void DrawTrip(SwingEffect en, Color Tripcolor, SpriteBatch sb,float rot,int point = 16,TripTex tex = TripTex.Afterimage)
        {
	        trip.Clear();
	        List<Vertex>[] tripPos = new List<Vertex>[point-1];
	        for (int j = 0; j < point-1; j++) {
		        tripPos[j] = new List<Vertex>();
	        }
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j] - Main.screenPosition,new Vector3(progress,progress2,0),Tripcolor));
				        tripPos[m].Add(new Vertex(a[j+1] - Main.screenPosition,new Vector3(progress,progress3,0),Tripcolor));
			        }
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j],new Vector3(progress,progress2,0),Tripcolor));
				        tripPos[m].Add(new Vertex(a[j+1],new Vector3(progress,progress3,0),Tripcolor));
			        }
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        ApplyShader(en);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        TexChance(tex);
	        for (int i = 0; i < point-1; i++) {
		        if (tripPos[i].Count > 3) {
			        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tripPos[i].ToArray(), 0,
				        tripPos[i].Count - 2);
		        }
	        }
	        sb.End();
	        sb.Begin();
        }

        public virtual void DrawTrip(SwingEffect en, Color[] Tripcolor, SpriteBatch sb,float rot,int point = 16,TripTex tex = TripTex.Afterimage)
        {
	        trip.Clear();
	        List<Vertex>[] tripPos = new List<Vertex>[point-1];
	        for (int j = 0; j < point-1; j++) {
		        tripPos[j] = new List<Vertex>();
	        }
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j] - Main.screenPosition,new Vector3(progress,progress2,0),Tripcolor[0]));
				        tripPos[m].Add(new Vertex(a[j+1] - Main.screenPosition,new Vector3(progress,progress3,0),Tripcolor[1]));
			        }
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j],new Vector3(progress,progress2,0),Tripcolor[0]));
				        tripPos[m].Add(new Vertex(a[j+1],new Vector3(progress,progress3,0),Tripcolor[1]));
			        }
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        ApplyShader(en);
	        TexChance(tex);
	        for (int i = 0; i < point-1; i++) {
		        if (tripPos[i].Count > 3) {
			        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tripPos[i].ToArray(), 0,
				        tripPos[i].Count - 2);
		        }
	        }
	        sb.End();
	        sb.Begin();
        }

		public virtual void DrawTrip(SwingEffect en, Color[] Tripcolor, SpriteBatch sb,float rot,Texture2D tex,int point = 16)
        {
	        trip.Clear();
	        List<Vertex>[] tripPos = new List<Vertex>[point-1];
	        for (int j = 0; j < point-1; j++) {
		        tripPos[j] = new List<Vertex>();
	        }
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j] - Main.screenPosition,new Vector3(progress,progress2,0),Tripcolor[0]));
				        tripPos[m].Add(new Vertex(a[j+1] - Main.screenPosition,new Vector3(progress,progress3,0),Tripcolor[1]));
			        }
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j],new Vector3(progress,progress2,0),Tripcolor[0]));
				        tripPos[m].Add(new Vertex(a[j+1],new Vector3(progress,progress3,0),Tripcolor[1]));
			        }
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        ApplyShader(en);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        Main.graphics.GraphicsDevice.Textures[0] = tex;
	        for (int i = 0; i < point-1; i++) {
		        if (tripPos[i].Count > 3) {
			        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tripPos[i].ToArray(), 0,
				        tripPos[i].Count - 2);
		        }
	        }
	        sb.End();
	        sb.Begin();
        }

	    public virtual void DrawTrip(SwingEffect en, Color Tripcolor, SpriteBatch sb,float rot,Texture2D tex,int point = 16)
        {
	        trip.Clear();
	        List<Vertex>[] tripPos = new List<Vertex>[point-1];
	        for (int j = 0; j < point-1; j++) {
		        tripPos[j] = new List<Vertex>();
	        }
	        if (mP.modifyScreenPos) {
		        GetCatmullPos(oldWorldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldWorldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j] - Main.screenPosition,new Vector3(progress,progress2,0),Tripcolor));
				        tripPos[m].Add(new Vertex(a[j+1] - Main.screenPosition,new Vector3(progress,progress3,0),Tripcolor));
			        }
		        }
	        }
	        else {
		        GetCatmullPos(oldHandPos, out Vector2[] TriphandPos);
		        GetCatmullPos(oldPos, out Vector2[] TripswordPos);
		        for (int i = 0; i < TriphandPos.Length; i++)
		        {
			        if (TriphandPos[i] == Vector2.Zero)
				        continue;
			        List<Vector2> a;
			        if (player.direction == 1)
				        a = CircularArcPoints(TripswordPos[i] , TriphandPos[i]
					        , 300,point);
			        else
				        a = CircularArcPoints(TriphandPos[i]
					        , TripswordPos[i] , 300,point);
			        float progress = i / (float)TriphandPos.Length;
			        for (int j=0,m=-1;j<a.Count-1;j++) {
				        float progress2 = player.direction==-1? j / (float)a.Count: 1-(j / (float)a.Count);
				        float progress3 =  player.direction==-1? (j+1) / (float)a.Count: 1-((j+1) / (float)a.Count);
				        m += 1;
				        tripPos[m].Add(new Vertex(a[j],new Vector3(progress,progress2,0),Tripcolor));
				        tripPos[m].Add(new Vertex(a[j+1],new Vector3(progress,progress3,0),Tripcolor));
			        }
		        }
	        }
	        sb.End();
	        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
		        SamplerState.AnisotropicClamp, DepthStencilState.None,
		        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	        ApplyShader(en);
	        Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        Main.graphics.GraphicsDevice.Textures[0] = tex;
	        for (int i = 0; i < point-1; i++) {
		        if (tripPos[i].Count > 3) {
			        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tripPos[i].ToArray(), 0,
				        tripPos[i].Count - 2);
		        }
	        }
	        sb.End();
	        sb.Begin();
        }

	    public void DrawPlayer(SpriteBatch sb) {
		    // 投射物 PreDraw 不能嵌套调用玩家渲染器，只提交残影数据，实际绘制由 ModPlayer.DrawPlayer 完成。
		    player.GetModPlayer<PlayerAfterimageDrawPlayer>().QueueAfterimages(oldPlayerPos, index, 0.5f);
	    }

        #endregion

        #region 攻击特殊效果 -- 卡肉/视线追踪/屏幕抖动......
        /// <summary>
        /// 碰撞点
        /// </summary>
        public float point;

        /// <summary>
        /// 判断碰撞方法
        /// </summary>
        /// <param name="targetHitbox">目标碰撞箱</param>
        /// <returns></returns>
        public bool Colliding(Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                handlePos,
                swordPos,
                20f,
                ref point);
        }

        public void SwordPunchCameraModifier(Vector2 hitPos, Vector2 hitDir, float strength, float time = 4f,
            int frames = 6)
        {
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                hitPos, hitDir, strength, time, frames));
        }

        public void ScreenRotModify(float rot)
        {
            Main.instance.CameraModifiers.Add(new ScreenRotateModifier
            {
                TargetRotation = MathHelper.ToRadians(rot)  // 其他字段有默认值
            });
        }

        public ModifyScreenPosPlayer mP;


        /// <summary>
        /// 屏幕位置修改
        /// </summary>
        /// <param name="WorldPos"></param>
        public void ScreenPosModify(Vector2 WorldPos)
        {
	        mP.ScreenPosition = WorldPos;
	        mP.modifyScreenPos = true;
        }

        #endregion

        #region 数据存储/读取
        public void SavePos(Vector2 pos,float rot,Vector2 swordend)
        {
            for (int i = index -1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
                oldHandPos[i] = oldHandPos[i - 1];
                oldWorldPos[i] = oldWorldPos[i - 1];
                oldWorldHandPos[i] = oldWorldHandPos[i - 1];
            }
            oldPos[0] = pos-Main.screenPosition;
            oldRot[0] = rot;
            oldHandPos[0] = swordend-Main.screenPosition;
            oldWorldHandPos[0] = swordend;
            oldWorldPos[0] = pos;
        }
		/// <summary>
		/// 记录玩家位置
		/// </summary>
		/// <param name="playerPos"></param>
        public void SavePlayerPos(Vector2 playerPos) {
	        for (int i = index - 1; i > 0; i--) {
		        oldPlayerPos[i] = oldPlayerPos[i - 1];
	        }
	        oldPlayerPos[0] = playerPos;
        }

        /// <summary>
		/// 圆弧取点（等角度间隔，真正对称的圆弧）
		/// </summary>
		/// <param name="start">起点</param>
		/// <param name="end">终点</param>
		/// <param name="arc">弧度（角度制，>0 逆时针弯，<0 顺时针弯）</param>
		/// <param name="samples">点数</param>
		public static List<Vector2> CircularArcPoints(Vector2 start, Vector2 end, float arc, int samples = 16)
		{
		    List<Vector2> points = new(samples);
		    Vector2 chord = end - start;
		    float chordLen = chord.Length();
		    if (chordLen < 0.001f)
		        return points;

		    float theta = MathHelper.ToRadians(arc);
		    theta = Math.Clamp(theta, -MathF.PI + 0.01f, MathF.PI - 0.01f);

		    // 直线退化
		    if (Math.Abs(theta) < 0.001f)
		    {
		        for (int i = 0; i < samples; i++)
		            points.Add(Vector2.Lerp(start, end, (float)i / (samples - 1)));
		        return points;
		    }

		    float sign = MathF.Sign(theta);
		    float absTheta = Math.Abs(theta);
		    float radius = chordLen / (2f * MathF.Sin(absTheta / 2f));  // 半径恒正
		    if (radius < 0.001f)
		        return points;

		    Vector2 dir = chord / chordLen;
		    Vector2 perp = new Vector2(-dir.Y, dir.X);  // 垂直方向
		    Vector2 bulge = perp * sign;                // 弧凸向
		    Vector2 mid = (start + end) * 0.5f;
		    float centerDist = radius * MathF.Cos(absTheta / 2f);
		    Vector2 center = mid - bulge * centerDist;  // 圆心在凸向反侧

		    float angleStart = MathF.Atan2(start.Y - center.Y, start.X - center.X);
		    float angleEnd   = MathF.Atan2(end.Y - center.Y, end.X - center.X);
		    float angleBulge = MathF.Atan2(bulge.Y, bulge.X);

		    // 有向扫角（先走短路径）
		    float sweep = angleEnd - angleStart;
		    while (sweep > MathF.PI) sweep -= MathF.Tau;
		    while (sweep < -MathF.PI) sweep += MathF.Tau;

		    // 短路径必须经过凸向，否则走长路（保证弯向正确）
		    float midAngle = angleStart + sweep / 2f;
		    float diff = MathF.Atan2(MathF.Sin(midAngle - angleBulge),
		                             MathF.Cos(midAngle - angleBulge));
		    if (Math.Abs(diff) > MathF.PI / 2f)
		        sweep = sweep > 0 ? sweep - MathF.Tau : sweep + MathF.Tau;

		    for (int i = 0; i < samples; i++)
		    {
		        float t = (float)i / (samples - 1);
		        float a = angleStart + sweep * t;
		        points.Add(center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius);
		    }
		    return points;
		}
        /// <summary>
        /// 生成两点间的弯曲弧线点
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="arc">弯曲弧度（角度制，>0 逆时针弯，<0 顺时针弯）</param>
        /// <param name="samples">返回的点数</param>
        public static List<Vector2> ArcPoints(Vector2 start, Vector2 end, float arc, int samples = 16)
        {
	        List<Vector2> points = new(samples);

	        // 弦方向
	        Vector2 chord = end - start;
	        float chordLen = chord.Length();
	        if (chordLen < 0.001f)
		        return points;

	        // 由弧度求弧高（弦中点垂直于弦方向的偏移）
	        float arcRad = MathHelper.ToRadians(arc);
	        // 弧半径：r = 弦长 / (2 * sin(弧角/2))，弧高 = r * (1 - cos(弧角/2))
	        float r = chordLen / (2f * MathF.Sin(arcRad / 2f));
	        float sag = r * (1f - MathF.Cos(arcRad / 2f));

	        Vector2 dir = chord / chordLen;
	        Vector2 perp = new Vector2(-dir.Y, dir.X);
	        Vector2 mid = (start + end) * 0.5f;
	        Vector2 control = mid + perp * sag;  // 控制点（弧顶点）

	        // 二次贝塞尔扫过
	        for (int i = 0; i < samples; i++)
	        {
		        float t = (float)i / (samples - 1);
		        float u = 1 - t;
		        points.Add(u * u * start + 2 * u * t * control + t * t * end);
	        }
	        return points;
        }

        public static Vector2 CatMullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2 * p1) +
                (-p0 + p2) * t +
                (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
                (-p0 + 3 * p1 - 3 * p2 + p3) * t3
            );
        }
        /// <summary>
        /// 获取细分曲线坐标
        /// </summary>
        /// <param name="pos">坐标数组</param>
        /// <param name="catmullPos">返回值</param>
        public void GetCatmullPos(Vector2[] pos, out Vector2[] catmullPos)
        {
            catmullPos = new Vector2[index * CatmullScale];
            int k = 0;
            for (int i = 0; i < pos.Length - 1; i++)
            {

                Vector2 p0 = i > 0 ? pos[i - 1] : pos[i];
                Vector2 p1 = pos[i];
                Vector2 p2 = pos[i + 1];
                Vector2 p3 = i + 2 < pos.Length ? pos[i + 2] : pos[i + 1];
                if(p0==Vector2.Zero|| p1==Vector2.Zero|| p2==Vector2.Zero|| p3==Vector2.Zero)
                    continue;
                catmullPos[k++] = p1;
                for (int j = 1; j < CatmullScale; j++)
                {
                    float t = (float)j / CatmullScale;
                    catmullPos[k++] = CatMullRom(p0, p1, p2, p3, t);
                }
            }
            catmullPos[k] = pos[^1];
        }
        #endregion
    }
}
