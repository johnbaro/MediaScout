using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImdbProvider
{
	public class FileCache
	{
		public string CacheDirectory
		{
			get;
			set;
		}

		public int MaxDays
		{
			get;
			set;
		}

		public FileCache()
		{
			this.CacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
			this.MaxDays = 7;
			this.createDirectory();
		}

		private void createDirectory()
		{
			if (!Directory.Exists(this.CacheDirectory))
			{
				Directory.CreateDirectory(this.CacheDirectory);
			}
		}

		public FileCache(string cacheDirectory, int maxDays)
		{
			this.CacheDirectory = cacheDirectory;
			this.MaxDays = maxDays;
			this.createDirectory();
		}

		public object Read(string name)
		{
			name = Path.Combine(this.CacheDirectory, name + ".cache");
			if (!File.Exists(name))
			{
				return null;
			}
			CacheObject cacheObject = this.Unserialize(name);
			if (!(cacheObject.Created.AddDays((double)this.MaxDays) > DateTime.Now))
			{
				return null;
			}
			return cacheObject.Object;
		}

		public bool Save(string name, object value)
		{
			return this.Serialize(new CacheObject
			{
				Created = DateTime.Now,
				Object = value
			}, Path.Combine(this.CacheDirectory, name + ".cache"));
		}

		private CacheObject Unserialize(string file)
		{
			CacheObject result;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				using (FileStream fileStream = new FileStream(file, FileMode.Open))
				{
					CacheObject cacheObject = (CacheObject)binaryFormatter.Deserialize(fileStream);
					result = cacheObject;
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		private bool Serialize(object obj, string file)
		{
			bool result;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple
				};
				using (FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate))
				{
					binaryFormatter.Serialize(fileStream, obj);
				}
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
	}
}
