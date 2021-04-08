using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.IO;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewModdingAPI.Utilities;
using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Tiles;
using xTile.Layers;

namespace InteractiveSchedule
{
	public class ModEntry : Mod
	{
		internal const string SpritesFile = "sprites";
		internal const string FontMonoThinFile = "output";
		public string GameContentPathFormat => ModManifest.Author + "." + ModManifest.Name + ".Assets\\";
		public string GameContentSpritesPath => PathUtilities.NormalizePath(GameContentPathFormat + SpritesFile);
		public string GameContentFontMonoThinPath => PathUtilities.NormalizePath(GameContentPathFormat + FontMonoThinFile);

		internal static ModEntry Instance;
		internal static Config Config;
		internal ITranslationHelper i18n => Helper.Translation;
		internal States _state = States.None;
		internal enum States
		{
			None,
			Path,
		}

		internal bool _pauseAllCharacters;
		internal string _pauseCharacter;
		internal SchedulePath _schedulePath;

		/// <summary>
		/// Name of player's current location before casting view using <see cref="ViewLocation"/>.
		/// Null after using <see cref="ReturnFromViewLocation"/>.
		/// </summary>
		internal string _originalLocation;
		/// <summary>
		/// Absolute world position of the player before casting view using <see cref="ViewLocation"/>.
		/// </summary>
		internal Vector2 _originalPosition;

		internal static Texture2D Sprites;
		internal static SpriteFont MonoThin;

		public Interface.Desktop Desktop;

		public override void Entry(IModHelper helper)
		{
			Instance = this;
			Config = helper.ReadConfig<Config>();

			AssetManager assetManager = new AssetManager();
			Helper.Content.AssetLoaders.Add(assetManager);

			Sprites = Game1.content.Load<Texture2D>(GameContentSpritesPath);
			MonoThin = Game1.content.Load<SpriteFont>(GameContentFontMonoThinPath);
			MonoThin.LineSpacing = 26;

			helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
			helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

			HarmonyPatches.Patch(helper);
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (Game1.keyboardDispatcher.Subscriber != null)
				return;

			if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
			{
				Interface.Desktop desktop = Game1.onScreenMenus.FirstOrDefault(menu => menu is Interface.Desktop) as Interface.Desktop;

				if (desktop == null || desktop.Taskbar == null || desktop.IsDesktopActive)
					return;

				Vector2 cursor = e.Cursor.GetScaledScreenPixels();
				desktop.receiveLeftClick((int)cursor.X, (int)cursor.Y, playSound: true);
			}

			if (Config.DebugMode)
			{
				if (e.Button == SButton.OemSemicolon)
				{
					Log.D("IS-ENTRY: Replacing Desktop");
					if (Game1.onScreenMenus.Remove(Game1.onScreenMenus.FirstOrDefault(menu => menu is Interface.Desktop)))
					{
						Game1.activeClickableMenu = null;
						Desktop.exitThisMenu();
						Desktop = new Interface.Desktop();
						Game1.onScreenMenus.Add(Desktop);
					}
				}
				else if (e.Button == SButton.OemQuotes)
				{
					Log.D("IS-ENTRY: Realigning Taskbar");
					Desktop.Taskbar.RealignElements();
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			Desktop = new Interface.Desktop();
			Game1.onScreenMenus.Add(Desktop);
		}
	}
}
