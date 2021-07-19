using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace InteractiveSchedule.Data
{
	public class Manifest
	{
		public string UniqueID;
		public string Name;
		public string Description;
		public SemanticVersion Version;
		public List<Dependency> Dependencies;
	}

	public class Dependency
	{
		public string UniqueID;
		public bool IsRequired;
		public SemanticVersion MinimumVersion;
	}
}
