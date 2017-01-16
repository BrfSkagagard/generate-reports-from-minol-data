using MinolReportsCreator.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using static MinolReportsCreator.Repositories.StyrelseReportRepository;

namespace MinolReportsCreator
{
    public class Program
    {
        public const double FlatRate = 90;

        static void Main(string[] args)
        {
            string gitFolder = @"C:\Users\Mattias\Documents\GitHub\";
            try
            {
                if (args != null && args.Length > 0)
                {
                    gitFolder = args[0];
                }

                var folderBoard = gitFolder + "brfskagagard-styrelsen" + Path.DirectorySeparatorChar;
                var folderBoardExists = Directory.Exists(folderBoard);

                var apartments = ApartmentRepository.GetApartments(gitFolder);
                var logins = LoginRepository.GetLogins(gitFolder);
                var apartmentReports = ApartmentReportRepository.CreateApartmentReports(apartments, logins);

                Console.WriteLine("Number of apartments: " + apartments.Count);

                foreach (ApartmentReport report in apartmentReports)
                {
                    var folder = gitFolder + "brfskagagard-lgh" + report.Number + Path.DirectorySeparatorChar;

                    var jsonReporter = new DataContractJsonSerializer(typeof(ApartmentReport));

                    Console.WriteLine("\t# " + report.Number);

                    using (
                        var stream =
                            File.Create(folder +
                                           Path.DirectorySeparatorChar + "minol-apartment-report-last-month.json"))
                    {
                        jsonReporter.WriteObject(stream, report);
                        stream.Flush();
                    }

                    // We only want to update repositories that we know about (read: that we have created)
                    if (folderBoardExists)
                    {
                        using (
                            var stream =
                                File.Create(folderBoard + "minol-apartment-report-" + report.Number + "-last-month.json"))
                        {
                            jsonReporter.WriteObject(stream, report);
                            stream.Flush();
                        }
                    }
                }

                if (folderBoardExists)
                {
                    var jsonWarning = new DataContractJsonSerializer(typeof(StyrelseWarning[]));
                    var styrelseWarnings = StyrelseReportRepository.CreateWarnings(apartments);
                    var groups = styrelseWarnings.GroupBy(w => w.Type);
                    foreach (var group in groups)
                    {
                        var orderedWarnings = group.OrderBy(w => w.Order);
                        // We only want to update repositories that we know about (read: that we have created)
                        using (
                            var stream =
                                File.Create(folderBoard + "minol-styrelse-" + group.Key + "-last-month.json"))
                        {
                            jsonWarning.WriteObject(stream, orderedWarnings.ToArray());
                            stream.Flush();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                using (var stream = File.CreateText(gitFolder + "generate-reports-last-error.txt"))
                {
                    stream.Write(ex.ToString());
                    stream.Flush();
                }

                throw;
            }
        }
    }
}
