using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.Vitric
{
    public class RefractiveBlade : ModItem
    {
        public int combo;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Refractive Blade");
            Tooltip.SetDefault("Swing in any direction \nHold down to launch a laser");
        }

        public override void SetDefaults()
        {
            item.damage = 56;
            item.width = 60;
            item.height = 60;
            item.useTime = 22;
            item.useAnimation = 22;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 7;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 2, 20, 0);
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<RefractiveBladeProj>();
            item.shootSpeed = 0.1f;
            item.noUseGraphic = true;
        }

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            if(player.altFunctionUse == 2)
			{
                if(!Main.projectile.Any(n => n.active && n.type == ProjectileType<RefractiveBladeLaser>() && n.owner == player.whoAmI))
                    Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<RefractiveBladeLaser>(), (int)(damage * 0.2f), knockBack, player.whoAmI, 0, 120);

                return false;
			}

            if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<RefractiveBladeLaser>() && n.owner == player.whoAmI))
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 0, combo);

            combo++;

            if (combo > 1)
                combo = 0;

            return false;
		}
	}

    public class RefractiveBladeProj : ModProjectile, IDrawPrimitive
    {
        int direction = 0;
        float maxTime = 0;
        float maxAngle = 0;

		private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";

        public ref float StoredAngle => ref projectile.ai[0];
        public ref float Combo => ref projectile.ai[1];

        public float Timer => 300 - projectile.timeLeft;
        public Player Owner => Main.player[projectile.owner];
        public float SinProgress => (float)Math.Sin((1 - Timer / maxTime) * 3.14f);

        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;

            projectile.timeLeft = 300;
        }

		public override void AI()
		{
            if (Timer == 0)
            {
                StoredAngle = projectile.velocity.ToRotation();
                projectile.velocity *= 0;

                Helper.PlayPitched("Effects/FancySwoosh", 1, Combo);

                switch(Combo)
				{
                    case 0:
                        direction = 1;
                        maxTime = 22;
                        maxAngle = 4;
                        break;
                    case 1:
                        direction = -1;
                        maxTime = 15;
                        maxAngle = 2;
                        break;
				}
            }

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;

            projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * (70 + (float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 40);
            projectile.rotation = targetAngle + 1.57f * 0.5f;

            ManageCaches();
            ManageTrail();

            var color = new Color(255, 140 + (int)(40 * SinProgress), 105);

            Lighting.AddLight(projectile.Center, color.ToVector3() * SinProgress);

            if(Main.rand.Next(2) == 0)
                Dust.NewDustPerfect(projectile.Center, DustType<Glow>(), Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.2f);

            if (Timer >= maxTime)
                projectile.timeLeft = 0;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.velocity += Vector2.UnitX.RotatedBy((target.Center - Owner.Center).ToRotation()) * 10 * target.knockBackResist;

            Helper.CheckLinearCollision(Owner.Center, projectile.Center, target.Hitbox, out Vector2 hitPoint); //here to get the point of impact, ideally we dont have to do this twice but for some reasno colliding hook dosent have an actual npc ref, soo...

            if (Helper.IsFleshy(target))
            {
                Helper.PlayPitched("Impacts/FireBladeStab", 0.3f, -0.2f, projectile.Center);

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(hitPoint, DustType<Glow>(), Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.25f) * Main.rand.NextFloat(5), 0, new Color(255, 105, 105), 0.5f);

                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(2, 8), 0, default, Main.rand.NextFloat(1, 2));
                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(3, 15), 0, default, Main.rand.NextFloat(1, 2));
                }
            }

            else
            {
                Helper.PlayPitched("Impacts/Clink", 0.5f, 0, projectile.Center);

                for (int k = 0; k < 30; k++)
                {
                    Dust.NewDustPerfect(hitPoint, DustType<Glow>(), Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 8), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.3f, 0.7f));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (Helper.CheckLinearCollision(Owner.Center, projectile.Center, targetHitbox, out Vector2 hitPoint))
                return true;

            return false;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = GetTexture(Texture);
            var texGlow = GetTexture(Texture + "Glow");

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;
            var pos = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * ((float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 20) - Main.screenPosition;

            spriteBatch.Draw(tex, pos, null, lightColor, projectile.rotation, new Vector2(0, tex.Height), 1.1f, 0, 0);
            spriteBatch.Draw(texGlow, pos, null, Color.White, projectile.rotation, new Vector2(0, texGlow.Height), 1.1f, 0, 0);

            return false;
		}

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(Vector2.Lerp(projectile.Center, Owner.Center, 0.15f));
                }
            }

            cache.Add(Vector2.Lerp(projectile.Center, Owner.Center, 0.15f));

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * (50 + 40 * Timer / maxTime), factor =>
            {
                if (factor.X >= 0.8f)
                    return Color.White * 0;

                return new Color(255, 120 + (int)(factor.X * 70), 80) * (factor.X * SinProgress );
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Vector2.Lerp(projectile.Center, Owner.Center, 0.15f) + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }
    }

	public class RefractiveBladeLaser : ModProjectile, IDrawAdditive
	{
        public Vector2 endPoint;
        public float LaserRotation;

        public ref float Charge => ref projectile.ai[0];
        public ref float MaxTime => ref projectile.ai[1];

        public int LaserTimer => (int)MaxTime - projectile.timeLeft;
        public Player Owner => Main.player[projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";

        public override void SetDefaults()
		{
            projectile.timeLeft = 300;
            projectile.width = 2;
            projectile.height = 2;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;

            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 5;
        }

		public override void AI()
		{
            projectile.Center = Owner.Center;
            LaserRotation = (Main.MouseWorld - Owner.Center).ToRotation();

            if (Main.mouseRight)
			{
                if (Charge < 65)
                    Charge++;

                projectile.timeLeft = (int)MaxTime + 1;
                return;
			}
            else if (Charge < 60)
			{
                projectile.timeLeft = 0;
                return;
			}

			for(int k = 0; k < 1000; k++)
			{
                Vector2 posCheck = projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                if (Helper.PointInTile(posCheck) || k == 999)
                {
                    endPoint = posCheck;
                    break;
                }
            }
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (LaserTimer > 0 && Helper.CheckLinearCollision(Owner.Center, endPoint, targetHitbox, out Vector2 colissionPoint))
            {
                Dust.NewDustPerfect(colissionPoint, DustType<Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 100), Main.rand.NextFloat());
                return true;
            }

            return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            target.velocity += Vector2.UnitX.RotatedBy(LaserRotation) * 0.25f * target.knockBackResist;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            spriteBatch.Draw(GetTexture(Texture), Owner.Center - Main.screenPosition, null, Color.White, LaserRotation + 1.57f / 2, new Vector2(0, GetTexture(Texture).Height), 1, 0, 0);
            return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch) 
		{
            if(LaserTimer <= 0)
			{
                var texShine = GetTexture(AssetDirectory.Assets + "Keys/GlowVerySoft");
                var texFlare = GetTexture(AssetDirectory.GUI + "ItemGlow");

                Vector2 progressPos = Owner.Center + Vector2.SmoothStep(Vector2.Zero, Vector2.UnitX.RotatedBy(LaserRotation) * 120, Charge / 65f) - Main.screenPosition;
                Color progressColor = Color.White * Helper.BezierEase(Charge / 65f);

                spriteBatch.Draw(texShine, progressPos, null, progressColor, 0, texShine.Size() / 2, (float)Math.Sin(Charge / 65f * 3.14f) * 0.5f, 0, 0);

                if(Charge > 40)
				{
                    float progress = (Charge - 40) / 25f;
                    Color flareColor = Color.White * (float)Math.Sin(progress * 3.14f);

                    spriteBatch.Draw(texFlare, Owner.Center + Vector2.UnitX.RotatedBy(LaserRotation) * 100 - Main.screenPosition, null, flareColor, Helper.BezierEase(progress) * 1, texFlare.Size() / 2, (float)Math.Sin(progress * 3.14f) * 0.5f, 0, 0);
                }

                return;
			}

            int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f); //Just a copy/paste of the boss laser. Need to tune this later
            var color = new Color(255, 160 + sin, 40 + sin / 2);

            var texBeam = GetTexture(AssetDirectory.MiscTextures + "BeamCore");
            var texBeam2 = GetTexture(AssetDirectory.MiscTextures + "BeamTrail");
            var texDark = GetTexture(AssetDirectory.MiscTextures + "GradientBlack");

            Vector2 origin = new Vector2(0, texBeam.Height / 2);
            Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

            var effect = StarlightRiver.Instance.GetEffect("Effects/GlowingDust");

            effect.Parameters["uColor"].SetValue(color.ToVector3());

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            float height = texBeam.Height / 4f;
            int width = (int)(projectile.Center - endPoint).Length();

            if (LaserTimer < 20)
                height = texBeam.Height / 4f * LaserTimer / 20f;

            if (LaserTimer > (int)MaxTime - 40)
                height = texBeam.Height / 4f * (1 - (LaserTimer - ((int)MaxTime - 40)) / 40f);


            var pos = projectile.Center - Main.screenPosition;

            var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
            var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

            var source = new Rectangle((int)((LaserTimer / 20f) * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
            var source2 = new Rectangle((int)((LaserTimer / 45f) * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

            spriteBatch.Draw(texBeam, target, source, color, LaserRotation, origin, 0, 0);
            spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, LaserRotation, origin2, 0, 0);

            for (int i = 0; i < width; i += 10)
            {
                Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);

                if (Main.rand.Next(20) == 0)
                    Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * i, DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.35f);
            }

            var opacity = height / (texBeam.Height / 2f) * 0.75f;

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            if (Owner == Main.LocalPlayer)
            {
                spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation + 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation - 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation - 3.14f, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
            }

            spriteBatch.Draw(GetTexture(Texture), Owner.Center - Main.screenPosition, null, Color.White, LaserRotation + 1.57f / 2, new Vector2(0, GetTexture(Texture).Height), 1, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            var impactTex = GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");
            var impactTex2 = GetTexture(AssetDirectory.GUI + "ItemGlow");
            var glowTex = GetTexture(AssetDirectory.Assets + "GlowTrail");

            spriteBatch.Draw(glowTex, target, source, color * 0.95f, LaserRotation, new Vector2(0, glowTex.Height / 2), 0, 0);

            spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (height * 0.012f), 0, impactTex.Size() / 2, 3.8f, 0, 0);
            spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color * (height * 0.05f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.38f, 0, 0);

            for (int k = 0; k < 4; k++)
            {
                float rot = Main.rand.NextFloat(6.28f);
                int variation = Main.rand.Next(30);

                color.G -= (byte)variation;

                Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * width + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(40), DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * 1, 0, color, 0.2f - (variation * 0.02f));
            }
        }
	}
}