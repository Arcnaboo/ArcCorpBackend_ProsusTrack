using ArcCorpBackend.Core.Users;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcCorpBackend.Core.Users
{
    [MessagePackObject]
    public partial class Knowledge
    {
        [Key(0)]
        public User User;

        [Key(1)]
        private HashSet<string> Preferences;

        [Key(2)]
        private HashSet<string> Avoids;

        // Public constructor for normal use
        public Knowledge(User user)
        {
            User = user;
            Preferences = new HashSet<string>();
            Avoids = new HashSet<string>();
        }

        // Private constructor for MessagePack deserialization
        private Knowledge()
        {
       
        }

        public void AddPreference(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Preferences.Add(value.Trim());
            }
        }

        public void AddAvoid(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Avoids.Add(value.Trim());
            }
        }

        public List<string> GetPreferencesAsList()
        {
            return Preferences.ToList();
        }

        public string GetPreferencesAsString()
        {
            if (Preferences.Count == 0)
                return "No preferences recorded.";

            return string.Join("; ", Preferences);
        }

        public List<string> GetAvoidsAsList()
        {
            return Avoids.ToList();
        }

        public string GetAvoidsAsString()
        {
            if (Avoids.Count == 0)
                return "No avoids recorded.";

            return string.Join("; ", Avoids);
        }
    }
}
