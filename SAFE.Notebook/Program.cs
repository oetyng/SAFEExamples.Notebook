using SAFE.CQRS;
using SAFE.DotNET.Auth.Services;
using SAFE.EventSourcing.Stream;
using SAFE.EventStore.Services;
using SAFE.SystemUtils;
using SAFE.TestCQRSApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SAFEExamples.Notebook
{
    class Program
    {
        static AuthService _auth;
        static IAppSession _session;
        static readonly long _sessionId = new Random(new Random().Next()).Next();
        static NoteBookCmdHandler _cmdHandler;

        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Console.WriteLine();
                Console.WriteLine(" -------- SAFEExamples.Notebook -------- ");

                if (!CreateAccount().GetAwaiter().GetResult())
                    Login().GetAwaiter().GetResult();
                
                SetupCmdHandler();

                Console.WriteLine();
                Console.WriteLine("Write any note and press enter to save.");

                CollectNotes();

                //ReadLoadTest();
                //LoadTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message + ex.StackTrace}");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }
        
        static void SetupCmdHandler()
        {
            Console.WriteLine("Enter database id (creates if not exists): ");
            var dbid = Console.ReadLine();
            _cmdHandler = new NoteBookCmdHandler(new Repository(new EventStreamHandler(new EventStoreImDProtocol(), dbid)));
        }

        static void CollectNotes()
        {
            int expectedVersion = StreamVersion.NoStream
            while (true)
            {
                Console.WriteLine("Write a note..");
                var note = Console.ReadLine();
                if (note == null)
                    continue;
                expectedVersion = SaveNote(note, expectedVersion);
            }
        }

        static int SaveNote(string note, int expectedVersion)
        {
            try
            {
                var targetId = _sessionId;
                var result = _cmdHandler.Handle(new AddNote(targetId, expectedVersion, note))
                    .GetAwaiter()
                    .GetResult();

                if (result.OK && result.Value)
                {
                    if (expectedVersion == -1)
                        ++expectedVersion;
                    ++expectedVersion;
                    Console.WriteLine($"Note saved: v.{expectedVersion}.");
                }
                else if (result.OK)
                    Console.WriteLine("Note already existed.");
                else
                    Console.WriteLine(result.ErrorMsg);

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message + ex.StackTrace}");
            }

            return expectedVersion;
        }

        #region Session
       
        static async Task<bool> CreateAccount()
        {
            Console.WriteLine("");
            Console.WriteLine("---- Create account on SAFE Network ----");

            Console.WriteLine("Create account Y / N ?");
            while (true)
            {
                var line = Console.ReadKey();
                if (line.Key == ConsoleKey.N)
                    return false;
                else if (line.Key == ConsoleKey.Y)
                    break;
            }

            _auth = SAFE.DotNET.Auth.DependencyService.Get<AuthService>();

            Console.Write("Username: ");
            var user = Console.ReadLine();
            Console.Write("Password: ");
            var pwd = Console.ReadLine();

            await _auth.CreateAccountAsync(user, pwd, "any string");

            await AutoLogin(user, pwd);

            return true;
        }

        static async Task Login()
        {
            Console.WriteLine("");
            Console.WriteLine("---- Login to SAFE Network ----");
            Console.Write("Enter username: ");
            var user = Console.ReadLine();
            Console.Write("Enter password: ");
            var pwd = Console.ReadLine();

            await AutoLogin(user, pwd);
        }

        static async Task AutoLogin(string user, string pwd)
        {
            _auth = SAFE.DotNET.Auth.DependencyService.Get<AuthService>();
            _session = new AppSession();

            await _auth.LoginAsync(user, pwd);

            var request = await _session.GenerateAppRequestAsync();
            request = request.Replace("safe-auth://", ":");
            var response = await _auth.HandleUrlActivationAsync(request);
            response = response.Replace("safe-oetyng.apps.safe.eventstore://", ":");
            await _session.HandleUrlActivationAsync(response);

            if (!_session.IsAuthenticated)
            {
                Console.WriteLine("Could not log in.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }
        #endregion Session


        #region Helpers
        static readonly Random Random = new Random();

        static string RandomString(int maxLength, bool fixedLength = false)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
            var length = fixedLength ? maxLength : Random.Next(10, Math.Max(10, maxLength));
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }
        #endregion Helpers


        #region Load tests
        
        // This method tries the read performance
        static void ReadLoadTest()
        {
            int expectedVersion = -1;

            var swlist = new List<long>();
            var sw = new Stopwatch();

            int count = 0;
            while (++count < 1000)
            {
                sw.Start();
                var note = "this is a note";
                if (note == null)
                    continue;
                expectedVersion = SaveNote(note, expectedVersion);
                sw.Stop();
                swlist.Add(sw.ElapsedMilliseconds);
                sw.Reset();
                Console.WriteLine(count);
            }
            Console.WriteLine($"Entered: {swlist.Count} notes in {swlist.Sum() / 1000m} s. I.e. {swlist.Count / (swlist.Sum() / 1000m)}/s ");
            var json = swlist.Json();
            Console.ReadLine();
            Console.WriteLine(json);
        }

        // This methods tries the write performance
        static void LoadTest()
        {
            int expectedVersion = -1;

            var swlist = new List<long>();
            var sw = new Stopwatch();

            while (true) // 10 > expectedVersion
            {
                Task.Delay(50000).Wait();
                sw.Start();
                var note = RandomString(10, true);
                if (note == null)
                    continue;
                expectedVersion = SaveNote(note, expectedVersion);
                sw.Stop();
                swlist.Add(sw.ElapsedMilliseconds);
                sw.Reset();

                if (expectedVersion % 10 == 0)
                    Console.WriteLine($"Entered: {swlist.Count} notes in {swlist.Sum() / 1000m} s. I.e. {swlist.Count / (swlist.Sum() / 1000m)}/s ");
            }

            var json = swlist.Json();
            Console.ReadLine();
            Console.WriteLine(json);
        }
        #endregion Load tests
    }
}
