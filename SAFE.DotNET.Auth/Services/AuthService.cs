using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;
using SAFE.DotNET.Auth.Models;
using SAFE.DotNET.Auth.Native;
using System.Threading;

namespace SAFE.DotNET.Auth.Services
{
    public class AuthService : IDisposable
    {
        private const string AuthReconnectPropKey = nameof(AuthReconnect);
        private readonly SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(1, 1);
        private bool _isLogInitialised;

        public bool IsLogInitialised { get => _isLogInitialised; set => _isLogInitialised = value; }

        private CredentialCacheService CredentialCache { get; }

        public bool AuthReconnect { get; set; }

        public AuthService()
        {
            _isLogInitialised = false;
            CredentialCache = new CredentialCacheService();
            InitLoggingAsync();
        }

        public void Dispose()
        {
            FreeState();
            GC.SuppressFinalize(this);
        }

        public async Task CheckAndReconnect()
        {
            await _reconnectSemaphore.WaitAsync();
            try
            {
                if (Session.IsDisconnected)
                {
                    if (!AuthReconnect)
                    {
                        throw new Exception("Reconnect Disabled");
                    }
                //show Loading ("Reconnecting to Network"))
                    var (location, password) = CredentialCache.Retrieve();
                    await LoginAsync(location, password);
                    try
                    {
                        var cts = new CancellationTokenSource(2000);
                    }
                    catch (OperationCanceledException) { }
                }
            }
            catch (Exception ex)
            {
                FreeState();
            }
            finally
            {
                _reconnectSemaphore.Release(1);
            }
        }

        public async Task CreateAccountAsync(string location, string password, string invitation)
        {
            Debug.WriteLine($"CreateAccountAsync {location} - {password} - {invitation.Substring(0, 5)}");
            await Session.CreateAccountAsync(location, password, invitation);
            if (AuthReconnect)
            {
                CredentialCache.Store(location, password);
            }
        }

        ~AuthService()
        {
            FreeState();
        }

        public void FreeState()
        {
            Session.FreeAuth();
        }

        public async Task<(int, int)> GetAccountInfoAsync()
        {
            var acctInfo = await Session.AuthAccountInfoAsync();
            return (Convert.ToInt32(acctInfo.Used), Convert.ToInt32(acctInfo.Used + acctInfo.Available));
        }

        public Task<List<RegisteredApp>> GetRegisteredAppsAsync()
        {
            return Session.AuthRegisteredAppsAsync();
        }

        public async Task<string> HandleUrlActivationAsync(string encodedUrl)
        {
            try
            {
                await CheckAndReconnect();
                var formattedUrl = UrlFormat.Convert(encodedUrl, true);
                var decodeResult = await Session.AuthDecodeIpcMsgAsync(formattedUrl);
                if (decodeResult.AuthReq.HasValue)
                {
                    var authReq = decodeResult.AuthReq.Value;
                    Debug.WriteLine($"Decoded Req From {authReq.AppExchangeInfo.Name}");
                    //var isGranted = await Application.Current.MainPage.DisplayAlert(
                    //  "Auth Request",
                    //  $"{authReq.AppExchangeInfo.Name} is requesting access",
                    //  "Allow",
                    //  "Deny");
                    var encodedRsp = await Session.EncodeAuthRspAsync(authReq, true);
                    var formattedRsp = UrlFormat.Convert(encodedRsp, false);
                    Debug.WriteLine($"Encoded Rsp to app: {formattedRsp}");
                    //Device.BeginInvokeOnMainThread(() => { Device.OpenUri(new Uri(formattedRsp)); });
                    return formattedRsp;
                }
                else
                {
                    var msg = "Decoded Req is not Auth Req";
                    Debug.WriteLine(msg);
                    throw new InvalidOperationException(msg);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex is ArgumentNullException)
                    errorMsg = "Ignoring Auth Request: Need to be logged in to accept app requests.";
                // logging
                throw;
            }
        }

        private async void InitLoggingAsync()
        {
            var started = await Session.InitLoggingAsync();
            if (!started)
            {
                Debug.WriteLine("Unable to Initialise Logging.");
                return;
            }

            Debug.WriteLine("Rust Logging Initialised.");
            IsLogInitialised = true;
        }

        public async Task LoginAsync(string location, string password)
        {
            Debug.WriteLine($"LoginAsync {location} - {password}");
            await Session.LoginAsync(location, password);
            if (AuthReconnect)
                CredentialCache.Store(location, password);
        }

        public async Task LogoutAsync()
        {
            await Task.Run(() => { Session.FreeAuth(); });
        }
    }
}