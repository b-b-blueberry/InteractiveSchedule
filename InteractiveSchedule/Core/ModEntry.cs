using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewModdingAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InteractiveSchedule
{
	public class ModEntry : Mod
	{
		internal const string DataRoot = "InteractiveContent";
		internal const string ProjectFile = "project.json";
		internal const string SpritesFile = "sprites";
		internal const string FontMonoThinFile = "output";
		internal static string DataPath => Path.Combine(Constants.DataPath, DataRoot);
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
		internal static SpriteFont MonoThinFont;
		internal const int MonoThinFontWidth = 12;

		internal static readonly Rectangle GradientRect = new Rectangle(78, 14, 48, 2);

		public Interface.Desktop Desktop;
		public Dictionary<Data.Project, string> Projects;

		public override void Entry(IModHelper helper)
		{
			Instance = this;
			Config = helper.ReadConfig<Config>();
			this.Init();
		}

		private void Init()
		{
			AssetManager assetManager = new AssetManager();
			Helper.Content.AssetLoaders.Add(assetManager);

			Sprites = Game1.content.Load<Texture2D>(GameContentSpritesPath);
			MonoThinFont = Game1.content.Load<SpriteFont>(GameContentFontMonoThinPath);
			MonoThinFont.LineSpacing = 26;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			HarmonyPatches.Patch(Helper);

			this.LoadProjects(overwrite: true);
		}

		public void LoadProjects(bool overwrite)
		{
			if (this.Projects != null && !overwrite)
			{
				throw new Exception();
			}

			Dictionary<Data.Project, string> projects = new Dictionary<Data.Project, string>();

			// Root directory for projects
			string path = ModEntry.DataPath;
			DirectoryInfo dir = null;
			if (!Directory.Exists(path))
			{
				Log.I($"Creating data directory: {path}");
				dir = Directory.CreateDirectory(path);
			}
			Log.I($"Loading data directory: {path}");
			dir ??= new DirectoryInfo(path);

			// Project directories
			DirectoryInfo[] subdirs = dir.GetDirectories();
			foreach (DirectoryInfo subdir in subdirs)
			{
				// Load project
				Data.Project project;
				FileInfo projectFile = subdir.GetFiles().FirstOrDefault(f => f.Name.Equals(ProjectFile));
				if (projectFile == null)
				{
					Log.I($"Loading blank project {subdir.Name}");
					project = new Data.Project();
				}
				else
				{
					project = Data.Project.Load(file: projectFile);
				}

				// Add project
				if (projects.Keys.Any(p => p.Guid.Equals(project.Guid)))
				{
					Log.E($"Did not load project {subdir.Name}: project with this GUID already exists.");
					continue;
				}
				if (projects.Values.Any(d => d.Equals(subdir.Name)))
				{
					Log.E($"Did not load project {subdir.Name}: project with this directory already exists.");
					continue;
				}
				projects.Add(project, subdir.Name);
			}

			this.Projects = projects;
		}

		public void SetActiveState(bool active)
		{
			Game1.activeClickableMenu = active && Desktop != null ? Desktop : null;
			Game1.isTimePaused = active;
			Game1.displayHUD = !active;
			Game1.displayFarmer = !active;
			Game1.viewportFreeze = active || !string.IsNullOrEmpty(_originalLocation);
			Game1.player.viewingLocation.Value = active ? Game1.currentLocation.Name : null;
		}

		private static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.keyboardDispatcher.Subscriber != null)
				return;

			if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
			{
				Interface.Desktop desktop = ISUtilities.GetDesktopFromOnscreenMenus();

				if (desktop == null || desktop.Taskbar == null || desktop.IsDesktopActive)
					return;

				Vector2 cursor = e.Cursor.GetScaledScreenPixels();
				desktop.receiveLeftClick((int)cursor.X, (int)cursor.Y, playSound: true);
			}

			if (Config.DebugMode)
			{
				if (e.Button == SButton.OemSemicolon)
				{
					Log.D("IS-ENTRY: Destroying Desktop");
					Instance.Desktop.exitThisMenu();
				}
				else if (e.Button == SButton.OemQuotes)
				{
					Log.D("IS-ENTRY: Creating Desktop");
					Instance.Desktop = new Interface.Desktop();
				}
			}
		}

		private static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			Instance.Desktop = new Interface.Desktop();
			Game1.onScreenMenus.Add(Instance.Desktop);
		}
	}
}
