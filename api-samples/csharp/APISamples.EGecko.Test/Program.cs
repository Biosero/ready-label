using APISamples.Driver;
using APISamples.Driver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.EGecko.Test
{
    // this is a test program 
    // modify as necessary
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ReadyLabelDriver driver = new ReadyLabelDriver();
                driver.InitializeDriver("localhost", 5300);

                LabelTemplate templates = driver.GetTemplates().FirstOrDefault();

                if (templates != null)
                {
                    // initialize egecko instrument
                    Console.WriteLine("Initializing instrument");

                    driver.EGeckoInitializeInstrument();

                    Console.WriteLine("Instrument Initialized");

                    // test rotations
                    Console.WriteLine("Rotating stage to 90");
                    driver.EGeckoRotateStage(90, "absolute");

                    Console.WriteLine("Rotating stage to 180");
                    driver.EGeckoRotateStage(180, "absolute");

                    Console.WriteLine("Rotating stage to 0");
                    driver.EGeckoRotateStage(0, "absolute");

                    Console.WriteLine("Printing label template at" + templates.FilePath);

                    Dictionary<string, bool> printSides = new Dictionary<string, bool>()
                    {
                        { "North", true },
                        { "South", false },
                        { "East", true },
                        { "West", false }
                    };

                    Dictionary<string, string> data = new Dictionary<string, string>()
                    {
                        { "VAR", "TEST DATA" },
                    };

                    // test print and apply
                    driver.EGeckoPrint(templates.FilePath, printSides, data, 0, 0, 0, false);

                    Console.WriteLine("EGecko test completed");
                }
                else
                {
                    Console.WriteLine("No label templates found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
