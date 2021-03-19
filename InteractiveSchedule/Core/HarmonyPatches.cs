using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Characters;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;

namespace InteractiveSchedule
{
	public static class HarmonyPatches
	{
		internal static void Patch(IModHelper helper)
		{
			HarmonyInstance harmony = HarmonyInstance.Create(helper.ModRegistry.ModID);
			harmony.Patch(
				original: AccessTools.Method(type: typeof(NPC), name: nameof(NPC.update), parameters: new Type[] { typeof(GameTime), typeof(GameLocation) }),
				prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(NPC_Update_Prefix)));
		}

		/// <summary>
		/// Disables per-tick updates for specific NPCs.
		/// </summary>
		internal static bool NPC_Update_Prefix(NPC __instance)
		{
			return !ModEntry.Instance._pauseAllCharacters && ModEntry.Instance._pauseCharacter != __instance.Name;
		}
	}
}
