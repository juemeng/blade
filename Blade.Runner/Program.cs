using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMan.Utils.ProgressBar;
using Blade.MX;

namespace Blade.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var progressBar = new ProgressBar(ProgressBarType.DotBar);
            Task.Run(() => FindMx(progressBar));
            progressBar.Show("Finding MobaXterm");
            Console.ReadLine();

        }

        static void FindMx(ProgressBar progressBar)
        {
            Thread.Sleep(3000);

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var files = dir.GetFiles("MobaXterm*.exe");

            if (files.Length == 0)
            {
                progressBar.Hide();
                Console.WriteLine("Cannot find MobaXterm, please try again later");
                return;
            }

            if (files.Length > 1)
            {
                progressBar.Hide();
                Console.WriteLine("Found Multiple MobaXterm, please check again");
                return;
            }

            var last = files[0].Name.Split("_").Last();
            var arr = last.Split(".");
            int major = -1, minor = -1;
            var autoParseVersion =
                arr.Length == 3 && int.TryParse(arr[0], out major) && int.TryParse(arr[1], out minor);


            Kill(progressBar, autoParseVersion, major, minor);

        }

        static void Kill(ProgressBar progressBar, bool autoParseVersion,int major,int minor)
        {
            string version;
            string name;
            if (autoParseVersion)
            {
                progressBar.Hide();
                Console.Clear();
                Console.Write("MobaXterm was found, please enter a name(e.g: peter): ");
                name = Console.ReadLine();
            }
            else
            {
                Console.Write("Cannot parse MobaXterm version, please enter version(e.g: 12.3): ");
                version = Console.ReadLine();
                var arr = version.Split(".");
                var valid = arr.Length == 2 && int.TryParse(arr[0], out major) && int.TryParse(arr[1], out minor);
                while (!valid)
                {
                    Console.Write("Cannot parse MobaXterm version, please enter a valid version(e.g: 12.3): ");
                    version = Console.ReadLine();
                    arr = version.Split(".");
                    valid = arr.Length == 2 && int.TryParse(arr[0], out major) && int.TryParse(arr[1], out minor);
                }

                Console.Write("please enter a name(e.g: peter): ");
                name = Console.ReadLine();
            }

            Task.Run(async () =>
            {
                Thread.Sleep(3000);
                var mxTools = new MxTools(major, minor, name);
                await mxTools.Tear();
                progressBar.Hide();
                Console.WriteLine("Process Complete");
            });

            progressBar.Show("Starting Processing");
            
        }




    }
}