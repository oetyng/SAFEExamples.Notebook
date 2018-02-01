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
        [TestMethod]
        public async Task CreateDbs()
        {
            try
            {
                var loginCount = 0;
                var keypass = "asd";
                while (true)
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
                        //await db.CreateDbAsync(dbId);
                        await db.Write_x(dbId);
                        Debug.WriteLine(++dbCount);
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
