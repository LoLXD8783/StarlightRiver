﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class JumboShrimp : Ingredient
	{
		public JumboShrimp() : base("+10% damage and movement speed when underwater", 3600 * 2, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 25);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			if (Player.wet)//unsure if this is for being in water
			{
				Player.GetDamage(DamageClass.Generic) += 0.1f * multiplier;
				Player.moveSpeed += Player.moveSpeed * (0.1f * multiplier);
			}
		}
	}
}