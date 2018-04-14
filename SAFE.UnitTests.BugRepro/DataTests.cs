using Microsoft.VisualStudio.TestTools.UnitTesting;
using SAFE.DotNET.Auth.Services;
using SAFE.EventStore.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;
using AuthSession = SAFE.DotNET.Auth.Native.Session;
using AuthBindings = SAFE.DotNET.Auth.Native.NativeBindings;
using AuthFileOps = SAFE.DotNET.Auth.FileOpsFactory;
using Session = SAFE.DotNET.Native.Session;
using NativeBindings = SAFE.DotNET.Native.NativeBindings;
using FileOps = SAFE.DotNET.FileOpsFactory;
using System.Collections.Generic;
using System.Linq;
using SAFE.EventSourcing.Models;
using SAFE.SystemUtils;
using SAFE.SystemUtils.Events;

namespace SAFE.DotNet.UnitTests
{
    [TestClass]
    public class DataTests
    {
        AppSession _app;
        AuthService _auth;

        [TestMethod] // Debug this method to be able to see debug output etc.
        public async Task CreateDbs()
        {
            try
            {
                InitAuth();

                var session = new Session(new NativeBindings(), FileOps.Create());
                _app = new AppSession(session);

                var loginCount = 0;
                var keypass = "asd";  // you need to have created this acc / pwd combo first.
                while (true) // login does not always work at first try with the local network.
                {
                    if (await AutoLogin(keypass, keypass))
                        break;
                    Debug.WriteLine(++loginCount);
                    await Task.Delay(100);
                }

                var db = new EventStoreImDProtocol(_app.AppId, session);
                //var db = new SAFEDataWriter(_app.AppId, session);

                var dbCount = 0;
                while (true)
                {
                    try
                    {
                        var dbId = Mock.RandomString(15);
                        await db.CreateDbAsync(dbId); // this is the original operation
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
                InitAuth();

                var session = new Session(new NativeBindings(), FileOps.Create());
                _app = new AppSession(session);

                var loginCount = 0;
                var keypass = "qwert";  // you need to have created this acc / pwd combo first.
                while (true) // login does not always work at first try with the local network.
                {
                    if (await AutoLogin(keypass, keypass))
                        break;
                    Debug.WriteLine(++loginCount);
                    await Task.Delay(100);
                }

                var db = new EventStoreImDProtocol(_app.AppId, session); // this is the actual protocol code
                                                                         //var db = new SAFEDataWriter(_app.AppId, session); // this is just a duplication of code in a mock class, for testing.

                var version = -1;

                var dbId = Mock.RandomString(15);
                await db.CreateDbAsync(dbId); // here we create a random db

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

                        await db.StoreBatchAsync(dbId, batch); // store the data to the db

                        Debug.WriteLine(version); // so we expect to reach ~22-30 entries before sudden crash. Sometimes more, sometimes less.
                        await Task.Delay(1000);
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

        async Task<bool> AutoLogin(string user, string pwd)
        {
            try
            {
                await _auth.LoginAsync(user, pwd);

                var request = await _app.GenerateAppRequestAsync();
                request = request.Replace("safe-auth://", ":");
                var response = await _auth.HandleUrlActivationAsync(request);
                response = response.Replace("safe-oetyng.apps.safe.eventstore://", ":");
                await _app.HandleUrlActivationAsync(response);

                return _app.IsAuthenticated;
            }
            catch (Exception ex)
            { return false; }
        }

        void InitAuth()
        {
            if (_auth == null)
            {
                var authSession = new AuthSession(new AuthBindings(), AuthFileOps.Create());
                _auth = new AuthService(authSession);
            }
        }
    }
}
