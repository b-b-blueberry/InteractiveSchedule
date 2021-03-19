using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.IO;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Locations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;
using xTile.Tiles;
using xTile.Layers;

namespace InteractiveSchedule
{
	public class ModEntry : Mod
	{
		internal class SchedulePath
		{
			public Character Who;
			public List<Location> Path;
			public Location Destination;
			public int PathIndex;
			public int Speed;
		}

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

		internal static Texture2D Sprites;

		public Interface.Desktop Desktop;

		public override void Entry(IModHelper helper)
		{
			Instance = this;
			Config = helper.ReadConfig<Config>();
			Sprites = Helper.Content.Load<Texture2D>(Path.Combine("assets", "sprites.png"));

			helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
			helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

			HarmonyPatches.Patch(helper);
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
			{
				Vector2 cursor = e.Cursor.ScreenPixels;
				Interface.Desktop desktop = Game1.onScreenMenus.FirstOrDefault(menu => menu is Interface.Desktop) as Interface.Desktop;
				if (!desktop.Taskbar.IsExpanded)
				{
					Microsoft.Xna.Framework.Rectangle area = desktop.Taskbar.ExpandButton.bounds;
					area.Width *= 3;
					area.Height *= 3;
					if (area.Contains((int)cursor.X, (int)cursor.Y))
					{
						desktop.Taskbar.SetActiveState(active: true);
					}
				}
			}

			if (Config.DebugMode)
			{
				if (e.Button == SButton.I)
				{
					Log.D("IS-ENTRY: Replacing Desktop");
					Desktop.exitThisMenu();
					Game1.activeClickableMenu = null;
					Game1.onScreenMenus.Remove(Game1.onScreenMenus.FirstOrDefault(menu => menu is Interface.Desktop));
					Desktop = new Interface.Desktop();
					Game1.onScreenMenus.Add(Desktop);
				}
				else if (e.Button == SButton.O)
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

		internal static void SetNpcToSchedulePoint(ref NPC npc, int time)
		{

		}

		internal static Tile GetTileOnHighestLayer(GameLocation location, int x, int y)
		{
			IEnumerable<Layer> layers = location.Map.Layers.Reverse();
			foreach (Layer layer in layers)
			{
				if (layer.Tiles[x, y] != null)
				{
					return layer.Tiles[x, y];
				}
			}
			return null;
		}
	}
}
