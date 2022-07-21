using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimplePM.WebAPI.Library.Models
{
    public class RepositoryEntry : IComparable<RepositoryEntry>
    {
        [JsonProperty]
        public string ID { get; set; }
        [JsonProperty]
        public int Version { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string URL { get; set; }
        [JsonProperty]
        public string Login { get; set; }
        [JsonProperty]
        public string Password { get; set; }

        // Foreign Key
        [JsonProperty]
        public string UserID { get; set; }
        public virtual UserData UserData { get; set; }

        public RepositoryEntry()
        {
        }

        public RepositoryEntry(string userID, Entry entry)
        {
            ID = entry.ID;
            Version = entry.Version;
            Name = entry.Name;
            URL = entry.URL;
            Login = entry.Login;
            Password = entry.Password;
            UserID = userID;
        }

        public int CompareTo(RepositoryEntry other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}