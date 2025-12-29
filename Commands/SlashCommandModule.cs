using NetCord.Services.ApplicationCommands;
using NetCord;
using NetCord.Rest;
using ComBot_Revamped.Data;
using NetCord.Gateway;

namespace ComBot_Revamped.Commands
{
    public class SlashCommandModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [UserCommand("ID")]
        public static string Id(User user) => user.Id.ToString();

        [SlashCommand("givepoints", "Gives a type of points to a user.")]
        public string GivePoints(User user, string type, ulong points)
        {
            if (PointsDb.pointTypes.ContainsKey(type))
            {
                var pointEntry = PointsDb.pointTypes[type];
                if (pointEntry.ContainsKey(user.Id)) {
                    pointEntry[user.Id] = pointEntry[user.Id] + points;
                } else
                {
                    pointEntry.Add(user.Id, points);
                }
            } else
            {
                var pointEntry = new Dictionary<ulong, ulong>();
                pointEntry.Add(user.Id, points);
                PointsDb.pointTypes.Add(type, pointEntry);
            }

            return "Points Added!";
        }

        [SlashCommand("clearpoints", "Clears the points of a user.")]
        public static string ClearPoints(User user, string? type = null)
        {
            if (type == null)
            {
                bool foundUser = false;
                foreach (var cat in PointsDb.pointTypes.Values) 
                {
                    if(cat.ContainsKey(user.Id))
                    {
                        cat.Remove(user.Id);
                        foundUser = true;
                    }
                }
                
                if(foundUser)
                {
                    return $"{user}\'s points were cleared!";
                } else
                {
                    return $"{user} has no points to clear!";
                }
            }
            else
            {
                if (PointsDb.pointTypes.ContainsKey(type)) {
                    var userDict = PointsDb.pointTypes[type];
                    if (!userDict.ContainsKey(user.Id))
                    {
                        return $"Could not clear {type} points from {user}, as they have none";
                    } else {
                        userDict.Remove(user.Id);
                        return $"{user}\'s {type} points were cleared!";
                    }
                } else
                {
                    return $"Could not clear {type} points from {user}, as {type} does not exist";
                }
            }
        }

        [SlashCommand("showpoints", "Displays the current point tallies.")]
        public string ShowPoints(User? user = null, string? type = null)
        {
            if(user != null)
            {
                
            } else
            {

            }

            // REPLACE THIS.
            return $"fuck you, {user}";
        }
    }
}
