﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dressing : Ingredient
	{
		public Dressing() : base("Food buffs are 5% more effective\nSlightly improved life regeneration\n+10% duration", 300, IngredientType.Seasoning, 1.1f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 15);
		}

		public override void BuffEffects(Player Player, float multiplier)//placeholder until drop methods for ingedients are finalized, original stats: +10% duration, chance for enemies to drop ingredients increased
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
			Player.lifeRegen += (int)(1.5f * multiplier + 0.05f);
		}
	}
}