using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SafeApp.MockAuthBindings;
using SafeApp.Utilities;

namespace SafeAuthenticator.Services
{
    public class AuthService :  IDisposable
    {
        private const string AuthReconnectPropKey = nameof(AuthReconnect);
        private readonly SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(1, 1);
        private Authenticator _authenticator;
        internal bool IsLogInitialised { get; set; }

        internal bool AuthReconnect { get; set; }

        string _location;
        string _pwd;

        public AuthService()
        {
            IsLogInitialised = false;
            Authenticator.Disconnected += OnNetworkDisconnected;
            InitLoggingAsync();
        }

        public void Dispose()
        {
            // ReSharper disable once DelegateSubtraction
            Authenticator.Disconnected -= OnNetworkDisconnected;
            FreeState();
            GC.SuppressFinalize(this);
        }

        public async Task CheckAndReconnect()
        {
            if (_authenticator == null)
            {
                return;
            }

            await _reconnectSemaphore.WaitAsync();
            try
            {
                if (_authenticator.IsDisconnected)
                {
                    if (!AuthReconnect)
                    {
                        throw new Exception("Reconnect Disabled");
                    }

                    Debug.WriteLine("Reconnecting to Network");
                    await LoginAsync(_location, _pwd);
                    Debug.WriteLine("Network connection established.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error", $"Unable to Reconnect: {ex.Message}", "OK");
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
            _authenticator = await Authenticator.CreateAccountAsync(location, password, invitation);
            if (AuthReconnect)
            {
                _location = location;
                _pwd = password;
            }
        }

        ~AuthService()
        {
            FreeState();
        }

        public void FreeState()
        {
            _authenticator?.Dispose();
        }

        public async Task<(int, int)> GetAccountInfoAsync()
        {
            var acctInfo = await _authenticator.AuthAccountInfoAsync();
            return (Convert.ToInt32(acctInfo.MutationsDone), Convert.ToInt32(acctInfo.MutationsDone + acctInfo.MutationsAvailable));
        }

        public async Task<List<RegisteredAppModel>> GetRegisteredAppsAsync()
        {
            var appList = await _authenticator.AuthRegisteredAppsAsync();
            return appList.Select(app => new RegisteredAppModel(app.AppInfo, app.Containers)).ToList();
        }

        public async Task<string> HandleUrlActivationAsync(string encodedUri)
        {
            try
            {
                if (_authenticator == null)
                {
                    return null;
                }

                await CheckAndReconnect();
                var encodedReq = UrlFormat.GetRequestData(encodedUri);
                var decodeResult = await _authenticator.DecodeIpcMessageAsync(encodedReq);
                var decodedType = decodeResult.GetType();
                if (decodedType == typeof(AuthIpcReq))
                {
                    var authReq = decodeResult as AuthIpcReq;
                    Debug.WriteLine($"Decoded Req From {authReq?.AuthReq.App.Name}");
                    var isGranted = true;
                      //  await Application.Current.MainPage.DisplayAlert(
                      //"Auth Request",
                      //$"{authReq?.AuthReq.App.Name} is requesting access",
                      //"Allow",
                      //"Deny");
                    var encodedRsp = await _authenticator.EncodeAuthRespAsync(authReq, isGranted);
                    var formattedRsp = UrlFormat.Format(authReq?.AuthReq.App.Id, encodedRsp, false);
                    Debug.WriteLine($"Encoded Rsp to app: {formattedRsp}");
                    //Device.BeginInvokeOnMainThread(() => { Device.OpenUri(new Uri(formattedRsp)); });
                    return formattedRsp;
                }
                else if (decodedType == typeof(IpcReqError))
                {
                    var error = decodeResult as IpcReqError;
                    Debug.WriteLine("Auth Request", $"Error: {error?.Description}", "Ok");
                }
                else
                {
                    Debug.WriteLine("Decoded Req is not Auth Req");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex is ArgumentNullException)
                    errorMsg = "Ignoring Auth Request: Need to be logged in to accept app requests.";

                Debug.WriteLine("Error", errorMsg, "OK");
            }

            return null;
        }

        private async void InitLoggingAsync()
        {
            await Authenticator.AuthInitLoggingAsync(null);

            Debug.WriteLine("Rust Logging Initialised.");
            IsLogInitialised = true;
        }

        public async Task LoginAsync(string location, string password)
        {
            Debug.WriteLine($"LoginAsync {location} - {password}");
            _authenticator = await Authenticator.LoginAsync(location, password);
            if (AuthReconnect)
            {
                _location = location;
                _pwd = password;
            }
        }

        public async Task LogoutAsync()
        {
            await Task.Run(() => { _authenticator.Dispose(); });
        }

        private void OnNetworkDisconnected(object obj, EventArgs args)
        {
            Debug.WriteLine("Network Observer Fired");

            if (obj == null || _authenticator == null || obj as Authenticator != _authenticator)
                return;

            Task.Run(CheckAndReconnect);
        }
    }
}