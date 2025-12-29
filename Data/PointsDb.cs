using System.Text.Json;

namespace ComBot_Revamped.Data
{
    public static class PointsDb
    {
        public static string databaseLocation = @"PointsDatabase.json";
        public static Dictionary<string, Dictionary<ulong, ulong>> pointTypes = new();

        
        private static object saveLocker = new();

        public static void Init()
        {
            if (File.Exists(databaseLocation)) {
                var dbStr = File.ReadAllBytes(databaseLocation);
                
                // I know this is illegible
                // It goes type -> userID -> amount of points
                pointTypes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<ulong, ulong>>>(dbStr);
            }
            var autosaveTimer = new System.Timers.Timer(300000);
            autosaveTimer.AutoReset = true;
            autosaveTimer.Elapsed += (a, b) => { Save(); };

            autosaveTimer.Start();
        }

        public static void Save()
        {


            // We'll still throw from an external file usage, but not our own concurrency
            lock (saveLocker)
            {
                var dbStr = JsonSerializer.Serialize(pointTypes);

                File.WriteAllText(databaseLocation, dbStr);
            }
        }
    }
}
