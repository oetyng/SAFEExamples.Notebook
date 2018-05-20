using Microsoft.VisualStudio.TestTools.UnitTesting;
using SAFE.EventStore.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SAFE.EventSourcing.Models;
using SAFE.SystemUtils;
using SAFE.SystemUtils.Events;
using SAFE.EventSourcing;
using SafeAuthenticator.Services;

namespace SAFE.DotNet.UnitTests
{
    [TestClass]
    public class DataTests
    {
        AppSession _app;
        IEventStore _db;
        AuthService _auth;

        [TestMethod] // Debug this method to be able to see debug output etc.
        public async Task CreateDbs()
        {
            try
            {
                await InitApp();

                //var db = new SAFEDataWriter(_app.AppId, session);

                var dbCount = 0;
                while (true)
                {
                    try
                    {
                        var dbId = GetRandomString(15);
                        await _db.CreateDbAsync(dbId); // this is the original operation
                        //await db.Write_17(dbId); // Write_1 - Write_17 will execute one additional operation per method, from CreateDbAsync. Write_17 will do the same as CreateDbAsync.
                        Debug.WriteLine(++dbCount); // so we expect to reach ~1285 iterations on Write_1 and about 50 iterations on Write_17
                        await Task.Delay(1);
                    }
                    catch (Exception ex)
                    { }
                }
            }
            catch (Exception ex)
            { }
        }

        [TestMethod] // Debug this method to be able to see debug output etc.
        public async Task WriteData()
        {
            try
            {
                await InitApp();

                var version = -1;

                var dbId = GetRandomString(15);
                await _db.CreateDbAsync(dbId); // here we create a random db

                var streamKey = $"{dbId}@{0}";

                while (true)
                {
                    try
                    {
                        var evt = new RaisedEvent(new NoteAdded(0, "someNote")) // create some data, in form of an event
                        {
                            SequenceNumber = ++version // protocol way of managing concurrent write to the stream
                        };
                        
                        var events = new List<RaisedEvent> { evt };
                        var data = events.Select(e => new EventData(
                            e.Payload,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            e.EventClrType,
                            e.Id,
                            e.Name,
                            e.SequenceNumber,
                            e.TimeStamp))
                        .ToList(); // protocol way of how to serialize and package the event data

                        var batch = new EventBatch(streamKey, Guid.NewGuid(), data); // another protocol way of packaging the data

                        var res = await _db.StoreBatchAsync(dbId, batch); // store the data to the db
                        if (res.Error)
                        { }

                        Debug.WriteLine(version); // so we expect to reach ~22-30 entries before sudden crash. Sometimes more, sometimes less.
                        //await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    { } // you can put breakpoint here, however, the big problem (and mystery) is that these do not catch anything, program just dies OR a NullReferenceException is reported in logs; "occurred in Unknown Module".
                }
            }
            catch (Exception ex)
            { } // you can put breakpoint here, however, the big problem (and mystery) is that these do not catch anything, program just dies OR a NullReferenceException is reported in logs; "occurred in Unknown Module".
        }

        public class NoteAdded : Event
        {
            public NoteAdded(long notebookId, string note)
            {
                NotebookId = notebookId;
                Note = note;
            }

            public long NotebookId { get; private set; }
            public string Note { get; private set; }
        }

        #region Session

        async Task InitApp()
        {
            _app = new AppSession();
            _auth = new AuthService();
            await ConnectAsync();
        }

        async Task ConnectAsync()
        {
            var pwdloc = GetRandomString(8);

            if (!await CreateAccountAsync(pwdloc, pwdloc))
                await LoginAsync(pwdloc, pwdloc);

            await AuthenticateAppAsync();
        }

        async Task<bool> CreateAccountAsync(string location, string pwd)
        {
            try
            {
                await _auth.CreateAccountAsync(location, pwd, "any string");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        async Task LoginAsync(string location, string pwd)
        {
            await _auth.LoginAsync(location, pwd);
        }

        async Task AuthenticateAppAsync()
        {
            var request = await _app.GenerateAppRequestAsync();
            request = request.Replace("safe-auth://", ":");

            var response = await _auth.HandleUrlActivationAsync(request);
            response = response.Replace("safe-oetyng.apps.safe.eventstore://", ":");
            _db = await _app.HandleUrlActivationAsync(response);

            if (!_app.IsAuthenticated)
            {
                Console.WriteLine("Could not log in.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        #endregion Session

        Random _rand = new Random();
        public string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_rand.Next(s.Length)]).ToArray());
        }
    }
}
