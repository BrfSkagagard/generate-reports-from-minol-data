using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace MinolReportsCreator.Repositories
{
    public class LoginRepository
    {
        public static MinoWebLogin GetLoginInfo(string gitFolder, int apartmentNumber)
        {
            var folder = gitFolder + "brfskagagard-lgh" + apartmentNumber + Path.DirectorySeparatorChar;

            using (
                var stream =
                    File.OpenRead(folder + Path.DirectorySeparatorChar + "minol-login.json"))
            {
                stream.Position = 0;
                var json = new DataContractJsonSerializer(typeof(MinoWebLogin));
                var login = json.ReadObject(stream) as MinoWebLogin;

                return login;
            }
        }

        public static List<MinoWebLogin> GetLogins(string gitFolder)
        {
            List<MinoWebLogin> logins = new List<MinoWebLogin>();
            DirectoryInfo root = new DirectoryInfo(gitFolder);
            var folders = root.GetDirectories("brfskagagard-lgh*");
            foreach (var folder in folders)
            {
                int apartmentNumber;
                var name = folder.Name.Replace("brfskagagard-lgh", "");
                if (int.TryParse(name, out apartmentNumber))
                {
                    var login = GetLoginInfo(gitFolder, apartmentNumber);
                    if (login != null)
                    {
                        login.Number = apartmentNumber;
                        logins.Add(login);
                    }
                }
            }
            return logins;
        }
    }
}
