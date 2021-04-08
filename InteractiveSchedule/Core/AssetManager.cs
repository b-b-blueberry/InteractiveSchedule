using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace InteractiveSchedule
{
	public class AssetManager : IAssetLoader
	{
		IModHelper Helper => ModEntry.Instance.Helper;

		public bool CanLoad<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals(ModEntry.Instance.GameContentSpritesPath)
				|| asset.AssetNameEquals(ModEntry.Instance.GameContentFontMonoThinPath);
		}

		public T Load<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals(ModEntry.Instance.GameContentSpritesPath))
			{
				return (T)(object)Helper.Content.Load<Texture2D>(Path.Combine("assets", ModEntry.SpritesFile + ".png"));
			}
			if (asset.AssetNameEquals(ModEntry.Instance.GameContentFontMonoThinPath))
			{
				return (T)(object)Helper.Content.Load<SpriteFont>(Path.Combine("assets", ModEntry.FontMonoThinFile + ".xnb"));
			}

			return (T)(object)null;
		}
	}
}
