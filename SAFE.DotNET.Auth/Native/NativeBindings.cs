using System;
using System.Runtime.InteropServices;
using Utils;
using System.Collections.Generic;

//[assembly: Dependency(typeof(NativeBindings))]

namespace SAFE.DotNET.Auth.Native
{
    public class NativeBindings : INativeBindings
    {
        #region AuthAccountInfo

        public void AuthAccountInfo(IntPtr authPtr, AuthAccountInfoCb callback)
        {
            AuthAccountInfoNative(authPtr, callback.ToHandlePtr(), OnAuthAccountInfoCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_account_info")]
        public static extern void AuthAccountInfoNative(IntPtr authPtr, IntPtr self, AuthAccountInfoCb callback);

        private static void OnAuthAccountInfoCb(IntPtr self, FfiResult result, IntPtr accountInfoPtr)
        {
            self.HandlePtrToType<AuthAccountInfoCb>()(IntPtr.Zero, result, accountInfoPtr);
        }

        #endregion

        #region AuthDecodeIpcMsg

        public void AuthDecodeIpcMsg(
          IntPtr authPtr,
          string encodedString,
          AppAuthReqCb appAuthCb,
          AppContReqCb appContCb,
          AppUnregAppReqCb appUnregCb,
          AppShareMDataReqCb appShareMDataCb,
          AppReqOnErrorCb appReqOnErrorCb)
        {
            var cbs = new List<object> { appAuthCb, appContCb, appUnregCb, appShareMDataCb, appReqOnErrorCb };
            AuthDecodeIpcMsgNative(
              authPtr,
              encodedString,
              cbs.ToHandlePtr(),
              OnAppAuthReqCb,
              OnAppContReqCb,
              OnAppUnregAppReqCb,
              OnAppShareMDataReqCb,
              OnAppReqOnErrorCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_decode_ipc_msg")]
        public static extern void AuthDecodeIpcMsgNative(
          IntPtr authPtr,
          string encodedString,
          IntPtr self,
          AppAuthReqCb appAuthCb,
          AppContReqCb appContCb,
          AppUnregAppReqCb appUnregCb,
          AppShareMDataReqCb appShareMDataCb,
          AppReqOnErrorCb appReqOnErrorCb);

        private static void OnAppAuthReqCb(IntPtr self, uint reqId, IntPtr authReq)
        {
            var cb = (AppAuthReqCb)self.HandlePtrToType<List<object>>()[0];
            cb(IntPtr.Zero, reqId, authReq);
        }

        private static void OnAppContReqCb(IntPtr self, uint reqId, IntPtr ffiContainersReq)
        {
            var cb = (AppContReqCb)self.HandlePtrToType<List<object>>()[1];
            cb(IntPtr.Zero, reqId, ffiContainersReq);
        }

        private static void OnAppUnregAppReqCb(IntPtr self, uint reqId)
        {
            var cb = (AppUnregAppReqCb)self.HandlePtrToType<List<object>>()[2];
            cb(IntPtr.Zero, reqId);
        }

        private static void OnAppShareMDataReqCb(IntPtr self, uint reqId, IntPtr ffiShareMDataReq, IntPtr ffiUserMetaData)
        {
            var cb = (AppShareMDataReqCb)self.HandlePtrToType<List<object>>()[3];
            cb(IntPtr.Zero, reqId, ffiShareMDataReq, ffiUserMetaData);
        }

        private static void OnAppReqOnErrorCb(IntPtr self, FfiResult result, string encodedString)
        {
            var cb = (AppReqOnErrorCb)self.HandlePtrToType<List<object>>()[4];
            cb(IntPtr.Zero, result, encodedString);
        }

        #endregion

        #region AuthExeFileStem

        public void AuthExeFileStem(AuthExeFileStemCb callback)
        {
            AuthExeFileStemNative(callback.ToHandlePtr(), OnAuthExeFileStemCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_exe_file_stem")]
        public static extern void AuthExeFileStemNative(IntPtr self, AuthExeFileStemCb callback);

        private static void OnAuthExeFileStemCb(IntPtr self, FfiResult result, string exeFileStem)
        {
            self.HandlePtrToType<AuthExeFileStemCb>()(IntPtr.Zero, result, exeFileStem);
        }

        #endregion

        #region AuthInitLogging

        public void AuthInitLogging(string fileName, InitLoggingCb callback)
        {
            AuthInitLoggingNative(fileName, callback.ToHandlePtr(), OnInitLoggingCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_init_logging")]
        public static extern void AuthInitLoggingNative(string fileName, IntPtr userDataPtr, InitLoggingCb callback);

        private static void OnInitLoggingCb(IntPtr self, FfiResult result)
        {
            self.HandlePtrToType<InitLoggingCb>()(IntPtr.Zero, result);
        }

        #endregion

        #region AuthOutputLogPath

        public void AuthOutputLogPath(string fileName, AuthLogPathCb callback)
        {
            AuthOutputLogPathNative(fileName, callback.ToHandlePtr(), OnAuthLogPathCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_output_log_path")]
        public static extern void AuthOutputLogPathNative(string fileName, IntPtr userDataPtr, AuthLogPathCb callback);

        private static void OnAuthLogPathCb(IntPtr self, FfiResult result, string path)
        {
            self.HandlePtrToType<AuthLogPathCb>()(IntPtr.Zero, result, path);
        }

        #endregion

        #region AuthRegisteredApps

        public void AuthRegisteredApps(IntPtr authPtr, AuthRegisteredAppsCb callback)
        {
            AuthRegisteredAppsNative(authPtr, callback.ToHandlePtr(), OnAuthRegisteredAppsCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_registered_apps")]
        public static extern void AuthRegisteredAppsNative(IntPtr authPtr, IntPtr self, AuthRegisteredAppsCb callback);

        private static void OnAuthRegisteredAppsCb(IntPtr self, FfiResult result, IntPtr registeredAppFfiPtr, IntPtr len)
        {
            self.HandlePtrToType<AuthRegisteredAppsCb>()(IntPtr.Zero, result, registeredAppFfiPtr, len);
        }

        #endregion

        #region AuthSetAdditionalSearchPath

        public void AuthSetAdditionalSearchPath(string path, AuthSetAdditionalSearchPathCb callback)
        {
            AuthSetAdditionalSearchPathNative(path, callback.ToHandlePtr(), OAuthSetAdditionalSearchPathCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_set_additional_search_path")]
        public static extern void AuthSetAdditionalSearchPathNative(string path, IntPtr self, AuthSetAdditionalSearchPathCb callback);

        private static void OAuthSetAdditionalSearchPathCb(IntPtr self, FfiResult result)
        {
            self.HandlePtrToType<AuthSetAdditionalSearchPathCb>()(IntPtr.Zero, result);
        }

        #endregion

        #region Create Account

        public void CreateAccount(string location, string password, string invitation, NetObsCb netobs, CreateAccountCb createAcctCb)
        {
            CreateAccountNative(location, password, invitation, netobs.ToHandlePtr(), createAcctCb.ToHandlePtr(), OnNetObsCb, OnCreateAccountCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "create_acc")]
        public static extern void CreateAccountNative(
          string location,
          string password,
          string invitation,
          IntPtr netCbPtr,
          IntPtr userDataPtr,
          NetObsCb netobs,
          CreateAccountCb createAcctCb);

        private static void OnCreateAccountCb(IntPtr self, FfiResult result, IntPtr authPtr)
        {
            self.HandlePtrToType<CreateAccountCb>()(IntPtr.Zero, result, authPtr);
        }

        private static void OnNetObsCb(IntPtr self, int errorCode, int eventType)
        {
            self.HandlePtrToType<NetObsCb>()(IntPtr.Zero, errorCode, eventType);
        }

        #endregion

        #region EncodeAuthRsp

        public void EncodeAuthRsp(IntPtr authPtr, IntPtr authReq, uint reqId, bool isGranted, EncodeAuthRspCb callback)
        {
            EncodeAuthRspCallbackNative(authPtr, authReq, reqId, isGranted, callback.ToHandlePtr(), OnEncodeAuthRspCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "encode_auth_resp")]
        public static extern void EncodeAuthRspCallbackNative(
          IntPtr authPtr,
          IntPtr authReq,
          uint reqId,
          [MarshalAs(UnmanagedType.U1)] bool isGranted,
          IntPtr self,
          EncodeAuthRspCb callback);

        private static void OnEncodeAuthRspCb(IntPtr self, FfiResult result, string encodedRsp)
        {
            self.HandlePtrToType<EncodeAuthRspCb>()(IntPtr.Zero, result, encodedRsp);
        }

        #endregion

        #region FreeAuthNative

        public void FreeAuth(IntPtr authPtr)
        {
            FreeAuthNative(authPtr);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "auth_free")]
        public static extern void FreeAuthNative(IntPtr authPtr);

        #endregion

        #region Login

        public void Login(string location, string password, NetObsCb netobs, LoginCb loginAcctCb)
        {
            LoginNative(location, password, loginAcctCb.ToHandlePtr(), netobs.ToHandlePtr(), OnNetObsCb, OnLoginCb);
        }

        [DllImport("safe_authenticator.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "login")]
        public static extern void LoginNative(
          string location,
          string password,
          IntPtr userDataPtr,
          IntPtr netObsPtr,
          NetObsCb netobs,
          LoginCb loginAcctCb);

        private static void OnLoginCb(IntPtr self, FfiResult result, IntPtr authPtr)
        {
            self.HandlePtrToType<LoginCb>()(IntPtr.Zero, result, authPtr);
        }

        #endregion
    }
}