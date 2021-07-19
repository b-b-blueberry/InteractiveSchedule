using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI;

namespace InteractiveSchedule.Data
{
	public class Project
	{
		public Guid Guid = Guid.Empty;
		public string Name = null;
		public Manifest Manifest = null;
		public bool IsActive = false;

		public Project()
		{
			this.Guid = Guid.NewGuid();
		}

		public Project(Guid guid, string name, Manifest manifest, bool isActive)
		{
			this.Guid = guid;
			this.Name = name;
			this.Manifest = manifest;
			this.IsActive = isActive;
		}

		public static Project Make(string path = null)
		{
			Log.I($"Making new project");
			path ??= ModEntry.DataPath;
			DirectoryInfo dir = new DirectoryInfo(path);
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException();
			}
			Project project = new Project();
			FileInfo file = project.Save(dir: dir, overwrite: false);
			ModEntry.Instance.Projects.Add(project, file.Name);
			Log.I($"Made new project: {file.Name} - {dir.Name}");
			return project;
		}

		public static Project Load(FileInfo file)
		{
			Log.I($"Loading project: {file.Name}");
			if (file == null)
			{
				throw new FileNotFoundException();
			}
			if (!file.Exists)
			{
				throw new FileNotFoundException(message: null, fileName: file.FullName);
			}
			string json = file.OpenText().ReadToEnd();
			if (string.IsNullOrWhiteSpace(json))
			{
				throw new FileLoadException(message: null, fileName: file.FullName);
			}
			Project project = JsonConvert.DeserializeObject<Project>(json);
			Log.I($"Loaded project: {project.Name} - {project.Guid}");
			return project;
		}

		public FileInfo Save(DirectoryInfo dir, bool overwrite)
		{
			Log.I($"Saving project: {this.Name} - {this.Guid}");
			if (dir == null || !dir.Exists)
			{
				throw new DirectoryNotFoundException();
			}
			dir = dir.CreateSubdirectory(this.Guid.ToString());
			string path = Path.Combine(dir.FullName, ModEntry.ProjectFile);
			FileInfo file = new FileInfo(path);
			if (file.Exists && file.Length > 8 && !overwrite)
			{
				throw new FileLoadException(message: null, fileName: file.FullName);
			}
			string json = JsonConvert.SerializeObject(value: this, formatting: Formatting.Indented);
			File.WriteAllText(file.FullName, json);
			Log.I($"Saved project: {file.Name}");
			return file;
		}
	}
}
