using System;

namespace SimplePM.WebAPI.Library.Models
{
    public class Entry : IComparable<Entry>
    {
        public EntrySyncOperation SyncOperation { get; set; }
        public string ID { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public Entry(string id, int version)
        {
            ID = id;
            Version = version;
        }

        public Entry(RepositoryEntry repositoryEntry)
        {
            ID = repositoryEntry.ID;
            Version = repositoryEntry.Version;
            Name = repositoryEntry.Name;
            URL = repositoryEntry.URL;
            Login = repositoryEntry.Login;
            Password = repositoryEntry.Password;
        }

        public int CompareTo(Entry other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}