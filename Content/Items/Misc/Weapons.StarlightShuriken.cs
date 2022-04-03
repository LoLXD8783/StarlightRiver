﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    class StarlightShuriken : ModItem
    {
        public int amountToThrow = 3;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Shuriken");
            Tooltip.SetDefault("Toss a volley of magical shurikens\nLanding every shuriken decreases the amount thrown by 1\nThrow a powerful glaive when you would throw only 1 shuriken");
        }

        public override void SetDefaults()
        {
            Item.damage = 18;
            Item.magic = true;
            Item.mana = 10;
            Item.width = 18;
            Item.height = 34;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.knockBack = 0f;
            Item.shoot = ModContent.ProjectileType<StarShuriken>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.Green;
		}

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            var aim = Vector2.Normalize(new Vector2(speedX, speedY));

			Helper.PlayPitched("Magic/ShurikenThrow", 0.7f, (3 - amountToThrow) / 3f * 0.6f, Player.Center);

            if(amountToThrow == 1)
			{
				Projectile.NewProjectile(Player.Center + aim * 20 + new Vector2(0, -12), aim * 8f, ModContent.ProjectileType<StarGlaive>(), (int)(damage * 1.5f), knockBack, Player.whoAmI);

                amountToThrow = 3;
                return false;
            }

            for(int k = 0; k < amountToThrow; k++)
			{
                float maxAngle = amountToThrow == 3 ? 0.21f : 0.16f;
                int i = Projectile.NewProjectile(Player.Center, aim.RotatedBy(-(maxAngle / 2f) + (k / (float)(amountToThrow - 1)) * maxAngle) * 6.5f, type, damage, knockBack, Player.whoAmI, 0, amountToThrow);
                var proj = Main.projectile[i].ModProjectile as StarShuriken;
                proj.creator = this;
			}

            amountToThrow--;

            if (amountToThrow <= 0)
                amountToThrow = 3;

            return false;
        }
    }

	class StarShuriken : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public StarlightShuriken creator;

		public Color color = new Color(100, 210, 255);

		public ref float State => ref Projectile.ai[0];

		public ref float numThrown => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.timeLeft = 150;
			Projectile.extraUpdates = 6;
			Projectile.magic = true;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.scale = 0.85f;
		}

		public override void AI()
		{
			if(creator is null && Projectile.timeLeft == 149)
			{
				Helper.PlayPitched("Magic/ShurikenThrow", 0.7f, 0, Projectile.Center);
			}

			color = numThrown == 3 ? new Color(100, 210, 255) : new Color(150, 150, 255);

			Projectile.rotation += 0.05f;
			Projectile.velocity *= 0.998f;

			Projectile.velocity.Y += 0.01f;

			if (Main.netMode != NetmodeID.Server)
			{
				if (Main.rand.Next(10) == 0)
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(8), ModContent.DustType<Dusts.Glow>(), Projectile.velocity * -Main.rand.NextFloat(0.6f), 0, color * 0.4f, 0.5f);

				if (Main.rand.Next(30) == 0)
				{
					var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(8), ModContent.DustType<Dusts.AuroraFast>(), Projectile.velocity * Main.rand.NextFloat(1, 1.5f), 0, color * 0.4f, 0.5f);
					d.customData = Main.rand.NextFloat(0.8f, 1.1f);
				}

				ManageCaches();
				ManageTrail();
			}

			if (Projectile.owner != Main.myPlayer && State == 0)
				findIfHit();
		}

		private void findIfHit()
		{
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.immune[Projectile.owner] <= 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
			{
				OnHitNPC(NPC, 0, 0, false);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;
			var texOutline = ModContent.Request<Texture2D>(Texture + "Outline").Value;
			var drawColor = color;

			if (Projectile.timeLeft < 30)
				drawColor = color * (Projectile.timeLeft / 30f);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor * 0.3f, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);
			spriteBatch.Draw(texOutline, Projectile.Center - Main.screenPosition, null, drawColor * 1.5f, Projectile.rotation, texOutline.Size() / 2, Projectile.scale, 0, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (State == 0)
			{
				Projectile.timeLeft = 30;
				Projectile.damage = 0;
				Projectile.velocity *= 0;
				Projectile.penetrate = -1;
				Projectile.tileCollide = false;

				State = 1;
			}
		}

		public override bool PreKill(int timeLeft)
		{
			if (Projectile.penetrate == 1 && creator != null)
				creator.amountToThrow = 3;

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(40 * 4), factor => factor * 16, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return new Color(color.R, color.G - 30, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(Projectile.timeLeft / 150f * 3.14f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);
		}
	}

	class StarGlaive : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public Color color = new Color(240, 100, 255);

		public ref float State => ref Projectile.ai[0];
		public ref float BounceSoundCooldown => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 62;
			Projectile.height = 62;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 3;
			Projectile.magic = true;
			Projectile.friendly = true;
			Projectile.penetrate = 20;
			Projectile.scale = 0.85f;
		}

		public override void AI()
		{
			Projectile.rotation += 0.06f;

			if(BounceSoundCooldown > 0)
				BounceSoundCooldown--;

			if (Projectile.velocity.Length() < 8)
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;

			if (Main.netMode != NetmodeID.Server)
			{
				if (Main.rand.Next(15) == 0)
				{
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(30), ModContent.DustType<Dusts.Glow>(), Projectile.velocity * -Main.rand.NextFloat(1), 0, color * 0.8f, 0.3f);

					var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(30), ModContent.DustType<Dusts.AuroraFast>(), Projectile.velocity * Main.rand.NextFloat(2, 3), 0, color * 0.4f, 0.5f);
					d.customData = Main.rand.NextFloat(0.8f, 1.1f);
				}

				ManageCaches();
				ManageTrail();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.penetrate--;

			if (Projectile.penetrate > 0)
			{
				if (BounceSoundCooldown <= 0)
				{
					Helper.PlayPitched("Magic/ShurikenBounce", 0.35f, 0, Projectile.Center);
					BounceSoundCooldown = 15;
				}

				Projectile.velocity *= -1;
				return false;
			}

			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;
			var texOutline = ModContent.Request<Texture2D>(Texture + "Outline").Value;
			var drawColor = color;

			if (Projectile.timeLeft < 30)
				drawColor = color * (Projectile.timeLeft / 30f);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor * 0.3f, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);
			spriteBatch.Draw(texOutline, Projectile.Center - Main.screenPosition, null, drawColor * 1.5f, Projectile.rotation, texOutline.Size() / 2, Projectile.scale, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation, tex.Size() / 2, Projectile.scale * 1.5f, 0, 0);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(40 * 4), factor => factor * 32, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return new Color(color.R, color.G - 30, color.B) * 0.3f * factor.X * (float)Math.Sin(Projectile.timeLeft / 600f * 3.14f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);
		}
	}
}
