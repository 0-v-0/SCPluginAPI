# API制作方法
Engine（2.2请忽略下列内容，只需删除`Engine.Properties`命名空间下的类即可）
------
- `Engine.Content.ContentStream`构造函数第一个参数类型改为`Stream`，参数名改为`stream`并删掉`m_path`的赋值，再把`m_stream`的赋值改为`stream`，同时将`Engine.Content.ContentCache.AddPackage`第一个参数类型改为`Stream`，参数名改为`stream`
- （Windows版）`Engine.Serialization.TypeCache.LoadedAssemblies`的get方法改为和安卓版的一样。
如果想加载所有子文件夹里的程序集文件及其引用就改为
``` c#
lock (m_typesByName)
{
	if (m_rescanAssemblies)
	{
		HashSet<Assembly> hashSet = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
		hashSet.Add(typeof(int).GetTypeInfo().Assembly);
		hashSet.Add(typeof(Uri).GetTypeInfo().Assembly);
		hashSet.Add(typeof(Enumerable).GetTypeInfo().Assembly);
		var enumerator = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories).GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			string extension = Path.GetExtension(current);
			if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
				try
				{
					hashSet.Add(Assembly.LoadFrom(current, null));
				}
				catch (Exception ex)
				{
					Log.Warning("Failed to load assembly \"{0}\". Reason: {1}", new object[2]
					{
						current,
						ex.Message
					});
				}
		}
		m_loadedAssemblies = new List<Assembly>(hashSet);
		m_rescanAssemblies = false;
	}
	return new ReadOnlyList<Assembly>(m_loadedAssemblies);
}
```（可能会影响性能）
TypeCache添加一个新方法：
``` c#
public static void AssemblyLoad(object obj, AssemblyLoadEventArgs a)
{
	lock (m_typesByName)
	{
		m_rescanAssemblies = true;
	}
}
```

并在静态构造函数末尾添加

``` c#
AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoad;
```

Storage.OpenFile改为

``` c#
	if (openFileMode < 0 || openFileMode > OpenFileMode.CreateOrOpen)
		throw new ArgumentException("openFileMode");
	FileMode mode;
	switch (openFileMode)
	{
	case OpenFileMode.Create:
		mode = FileMode.Create;
		break;
	case OpenFileMode.CreateOrOpen:
		mode = FileMode.OpenOrCreate;
		break;
	default:
		mode = FileMode.Open;
		break;
	}
	return File.Open(ProcessPath(path, openFileMode != OpenFileMode.Read, false), mode, (openFileMode == OpenFileMode.Read) ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read);
```
（Bugs）`Storage.ProcessPath`里的```if (!path.StartsWith("data:"))```下一行改为
``` c#
return path;
```

EntitySystem（如果有的话）
-------------------------
（可选）在 `Database::AddDatabaseObject` 中从后往前找```callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<valuetype [mscorlib]System.Guid,class TemplatesDatabase.DatabaseObject>::Add改为set_Item```；在 `DatabaseObject::set_NestingParent` 中找 ```callvirt instance void TemplatesDatabase.Database::AddDatabaseObject```第二个参数改为`false`（即前面的`ldc.i4.1`改为`ldc.i4.0`）

修改主程序
----------
1. 编译要注入的dll并反编译成il
2. 在`LabelWidget`类中新建类型为`Dictionary<string, string>`的字段Strings并将`LabelWidget.Text`的set方法修改为
``` c#
if (Strings.TryGetValue(value, out string text))
	value = text;
if (value != m_text)
{
	m_text = value;
	m_linesSize = null;
}
```
3. 添加`ModInfo`, `PluginLoaderAttribute`, `FileEntry`, `ModsManager`四个类
4. 在`LoadingScreen.Update`中把```ldstr      "Loading error. Reason: "```和```call       void [Engine]Engine.Log::Error(string)```之前的指令修改为
```
ldloc.2
call       string [mscorlib]System.String::Concat(object, object)
```
5. （Bugs&RuthlessConquest）`ModsManager`类中添加个类型为`string`的`Path`字段并将ModsManager那段il```ldsfld     string Game.ContentManager::Path```替换成```ldsfld     string Game.ModsManager::Path```；`Program.Initialize`末尾添加```ModsManager.Initialize()```；在`ModsManager.Initialize()`前添加
```
call       class [Mono.Android]Java.IO.File [Mono.Android]Android.OS.Environment::get_ExternalStorageDirectory()
callvirt   instance string [Mono.Android]Java.IO.File::get_AbsolutePath()
ldstr      "[游戏名]/Mods"
call       string Game.ModsManager::Combine(string, string)
```
（安卓版）
```
ldstr      "Mods"
stsfld     string Game.ContentManager::Path
```
（Windows版）  
6. （Survivalcraft）替换`BlocksManager`, `ContentManager`, `CraftingRecipesManager`, `HelpScreen`, `StringsManager`整个类并替换`ClothingBlock.Initialize`, `DatabaseManager.Initialize`, `WorldsManager.Initialize`整个方法，如果`SettingsGraphicsScreen`和原版的不一样就替换成原版的（保证安卓版与电脑版mod的通用性），删除Game.Properties命名空间下带```[CompilerGenerated]```的类（如果有的话）；
`ComponentGui.DisplayLargeMessage`改为
```
	m_message = new Message
	{
		LargeText = largeText ?? string.Empty,
		SmallText = smallText,
		Duration = duration,
		StartTime = Time.RealTime + (double)delay
	};
```
（安卓版）`AndroidSdCardExternalContentProvider.InitializeFilesystemAccess`改为
``` c#
m_rootDirectory = "/sdcard/Survivalcraft/"
```
7. （Bugs&RuthlessConquest）`GameLogSink`的构造函数改为
``` c#
try
{
	if (m_stream != null)
		throw new InvalidOperationException("GameLogSink already created.");
	Directory.CreateDirectory("/sdcard/Bugs");
	string path = "/sdcard/Bugs/Game.log";
	m_stream = File.Open(path, FileMode.OpenOrCreate);
	if (m_stream.Length > 10485760)
	{
		m_stream.Dispose();
		m_stream = File.Open(path, FileMode.Create);
	}
	m_stream.Position = m_stream.Length;
	m_writer = new StreamWriter(m_stream);
}
catch (Exception ex)
{
	Engine.Log.Error("Error creating GameLogSink. Reason: {0}", ex.Message);
}
```
8. （Bugs）GameData的构造函数偏移82位置后插入
```
ldstr ".pat"
call class [mscorlib]System.Collections.Generic.List`1<class Game.FileEntry> Game.ModsManager::GetEntries(string)
ldstr "Name"
ldstr "Type"
ldnull
call class [System.Xml.Linq]System.Xml.Linq.XElement Game.ContentManager::CombineXml(class [System.Xml.Linq]System.Xml.Linq.XElement, class [mscorlib]System.Collections.Generic.IEnumerable`1<class Game.FileEntry>, string, string, string)
```
将以上exe/dll文件拖入Injector
----

替换图标
--------
- 做几个不同分辨率的png格式的图标（分辨率跟原图标相同）
- （可选）打开limitPNG最新版，压缩等级选择极限，无损压缩png图标
- （Windows版）将png制作成图标组，并替换exe里的资源（推荐用 [PNGtoICO](https://download.csdn.net/download/zytf16888/3435434) 制作图标组）
- （安卓版）替换所有drawable开头的文件夹里的androidicon.png；并压缩所有png（可选）

（可选）优化
------------
- 将其余无强名称的dll文件拖入Injector
- 将Content.pak拖入PAKOptimizer，安卓版在当前文件夹新建个assets文件夹并把输出的pak移到该文件夹下，Windows版直接用输出的pak替换掉
- （安卓版）解压lib文件夹到当前文件夹，打开命令行输入：
```
7z a -mx=9 -tzip "SCPluginAPI.apk" "lib\*" -r -mfb=258
7z a -mx=9 -tzip "SCPluginAPI.apk" "assets\Content.pak" -mfb=258
```

打包 & 发布
-----------
- （安卓版）用制作好的dll替换assemblies文件夹里的dll文件，压缩等级选存储，然后签名apk
- （Windows版）推荐用7zip压缩，右键文件夹点添加到压缩包，格式选7z，算法选LZMA2，压缩等级选极限，单词大小选最大