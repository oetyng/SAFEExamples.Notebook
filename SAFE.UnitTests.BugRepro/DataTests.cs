using Microsoft.VisualStudio.TestTools.UnitTesting;
using SAFE.DotNET.Auth.Services;
using SAFE.EventStore.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;

namespace SAFE.DotNet.UnitTests
{
    [TestClass]
    public class DataTests
    {
        [TestMethod] // Debug this method to be able to see debug output etc.
        public async Task CreateDbs()
        {
            try
            {
                var loginCount = 0;
                var keypass = "asd";  // you need to have created this acc / pwd combo first.
                while (true) // login does not always work at first try with the local network.
                {
                    if (await AutoLogin(keypass, keypass))
                        break;
                    Debug.WriteLine(++loginCount);
                    await Task.Delay(100);
                }

                //var db = new EventStoreImDProtocol();
                var db = new SAFEDataWriter();

                var dbCount = 0;
                while (true)
                {
                    try
                    {
                        var dbId = Mock.RandomString(15);
                        //await db.CreateDbAsync(dbId); // this is the original operation
                        await db.Write_1(dbId); // Write_1 - Write_17 will execute one additional operation per method, from CreateDbAsync. Write_17 will do the same as CreateDbAsync.
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

        async Task<bool> AutoLogin(string user, string pwd)
        {
            try
            {
                var auth = DotNET.Auth.DependencyService.Get<AuthService>();
                var session = new AppSession();

                await auth.LoginAsync(user, pwd);

                var request = await session.GenerateAppRequestAsync();
                request = request.Replace("safe-auth://", ":");
                var response = await auth.HandleUrlActivationAsync(request);
                response = response.Replace("safe-oetyng.apps.safe.eventstore://", ":");
                await session.HandleUrlActivationAsync(response);

                return session.IsAuthenticated;
            }
            catch (Exception ex)
            { return false; }
        }
    }
}
