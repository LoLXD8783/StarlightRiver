﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Flour : Ingredient
	{
		public Flour() : base("Regain 1 hp a second", 7200, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.value = 100;
			Item.rare = ItemRarityID.White;
			Item.value = Item.buyPrice(silver: 20);//higher than others since only this directly crafts into 2 other foods
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			int interval = (int)(60 / multiplier);//(amount-per-second * multiplier)

			if (Player.GetModPlayer<StarlightPlayer>().Timer % interval == 0)
				Player.statLife++;
		}
	}
}