using System;
//using System.IO;

namespace Game
{
	public class SubsystemJs : SubsystemBlockBehavior
	{
		public static Jint.Engine JsEngine;
		public static ComponentPlayer ComponentPlayer;
		protected static string lastcode = "";

		public override int[] HandledBlocks => new int[0];

		/*public override void Load(ValuesDictionary valuesDictionary)
		{
			JsEngine.Invoke("invokeInterface", "onLoad", valuesDictionary);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			JsEngine.Invoke("invokeInterface", "onSave", valuesDictionary);
		}

		public override void Dispose()
		{
			JsEngine.Invoke("invokeInterface", "onDispose");
		}

		public override void OnEntityAdded(Entity entity)
		{
			JsEngine.Invoke("invokeInterface", "onEntityAdded", entity);
		}

		public override void OnEntityRemoved(Entity entity)
		{
			JsEngine.Invoke("invokeInterface", "onEntityRemoved", entity);
		}

		public static void Init(Jint.Options cfg)
		{
			cfg.AllowClr(typeof(Engine.Storage).Assembly, typeof(SubsystemJs).Assembly, typeof(Block).Assembly);
		}*/

		public SubsystemJs()
		{
			JsEngine = new Jint.Engine();
			/*using (var streamReader = new StreamReader(File.OpenRead(ModsManager.Combine(ContentManager.Path, "js/init.js"))))
			{
				JsEngine = new Jint.Engine(Init).Execute(streamReader.ReadToEnd());
			}*/
		}
		public static void Execute(string code)
		{
			if (string.IsNullOrWhiteSpace(code)) return;
			try
			{
				Jint.Engine engine = JsEngine.Execute(code);
				if (ComponentPlayer != null)
					ComponentPlayer.ComponentGui.DisplaySmallMessage(engine.GetCompletionValue().ToString(), false, false);
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("JS", e);
			}
			lastcode = code;
		}
		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			ComponentPlayer = componentPlayer;
			DialogsManager.ShowDialog(componentPlayer.View.GameWidget, new TextBoxDialog("JS", lastcode, int.MaxValue, Execute));
			return true;
		}
	}
}
/*namespace Launcher
{
	public static class JsInterface
	{
		public static string readAllTextCustom(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (path.StartsWith("ex:"))
			{
				if (Path.DirectorySeparatorChar != '/')
				{
					path = path.Replace('/', Path.DirectorySeparatorChar);
				}
				if (Path.DirectorySeparatorChar != '\\')
				{
					path = path.Replace('\\', Path.DirectorySeparatorChar);
				}
				using (var streamReader = new StreamReader(Path.Combine(ContentManager.Path, path.Substring(3).TrimStart(Path.DirectorySeparatorChar))))
				{
					return streamReader.ReadToEnd();
				}
			}
			return Storage.ReadAllText(path);
		}
		public static void setDatabaseValue(DatabaseObject databaseObject, object value)
		{
			Type type = databaseObject.Value.GetType();
			if (type == value.GetType())
			{
				databaseObject.Value = value;
			}
			else
			{
				databaseObject.Value = Convert.ChangeType(value, type);
			}
		}
	}
}*/