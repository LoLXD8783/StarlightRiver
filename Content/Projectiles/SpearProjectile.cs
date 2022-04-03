﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Projectiles
{
	public abstract class SpearProjectile : ModProjectile
    {
        public int Duration;
        public float Min;
        public float Max;

        protected SpearProjectile(int duration, float minOff, float maxOff)
        {
            Duration = duration;
            Min = minOff;
            Max = maxOff;
        }

        public virtual void SafeSetDefaults() { }

        public virtual void SafeAI() { }

        public sealed override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 19;
            Projectile.friendly = true;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.melee = true;
            SafeSetDefaults();
        }

        public sealed override void AI()
        {
            SafeAI();

            Player Player = Main.player[Projectile.owner];

            Player.heldProj = Projectile.whoAmI;
            Player.ItemTime = Player.ItemAnimation;

            int realDuration = (int)(Duration * Player.meleeSpeed);
            if (Projectile.timeLeft == Duration) Projectile.timeLeft = realDuration;
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);

            Projectile.rotation = MathHelper.Pi * (3 / 4f) + Projectile.velocity.ToRotation();
            float progress = Projectile.timeLeft > realDuration / 2f ? (realDuration - Projectile.timeLeft) / (realDuration / 2f) : Projectile.timeLeft / (realDuration / 2f);
            Projectile.Center = Player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * Min, Projectile.velocity * Max, progress);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[Projectile.type], (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY), Main.projectileTexture[Projectile.type].Frame(), Color.White, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
            return false;
        }
    }
}
