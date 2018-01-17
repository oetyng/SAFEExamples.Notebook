using System;
using Utils;

namespace SAFE.DotNET.Auth.Native
{
    public struct AccountInfo
    {
        public ulong Used;
        public ulong Available;
    }

    public struct RegisteredAppFfi
    {
        public AppExchangeInfo AppExchangeInfo;
        public IntPtr ContainersArrayPtr;
        public IntPtr ContainersLen;
        public IntPtr ContainersCap;
    }

    public struct DecodeIpcResult
    {
        public AuthReq? AuthReq;
        public uint? UnRegAppReq;
        public IntPtr? ContReq;
        public (IntPtr, IntPtr) ShareMDataReq;
    }
}