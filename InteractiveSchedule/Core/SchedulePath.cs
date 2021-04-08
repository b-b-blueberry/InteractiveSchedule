using StardewValley;
using System.Collections.Generic;
using xTile.Dimensions;

namespace InteractiveSchedule
{
	public class SchedulePath
	{
		public Character Who;
		public List<Location> Path;
		public Location Destination;
		public int PathIndex;
		public int Speed;
	}
}
