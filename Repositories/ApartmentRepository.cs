using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace MinolReportsCreator.Repositories
{
    public class ApartmentRepository
    {
        public static List<Apartment> GetApartments(string gitFolder)
        {
            var apartments = new List<Apartment>();
            var folders = Directory.GetDirectories(gitFolder, "brfskagagard-lgh*");
            var json = new DataContractJsonSerializer(typeof(Apartment));
            foreach (string folder in folders)
            {
                using (
                    var stream =
                        File.OpenRead(folder + Path.DirectorySeparatorChar + "minol-apartment-measurement.json"))
                {
                    stream.Position = 0;
                    var apartment = json.ReadObject(stream) as Apartment;
                    if (apartment == null)
                    {
                        continue;
                    }

                    apartments.Add(apartment);
                }
            }
            return apartments;
        }
    }
}
