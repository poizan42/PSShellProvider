using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

namespace PSShellProvider
{
    internal static class ShellNativeFunctions
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHParseDisplayName"), SuppressUnmanagedCodeSecurity]
        private static extern int _SHParseDisplayName(string name,
            [MarshalAs(UnmanagedType.Interface)]
            IBindCtx pbc,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            out IdList pidl, SFGAO sfgaoIn, out SFGAO psfgaoOut);
        public static IdList SHParseDisplayName(string name, IBindCtx bc, SFGAO sfgaoIn, out SFGAO sfgaoOut)
        {
            IdList pidl;
            int hr = _SHParseDisplayName(name, bc, out pidl, sfgaoIn, out sfgaoOut);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
            return pidl;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHCreateItemFromIDList"), SuppressUnmanagedCodeSecurity]
        private static extern int _SHCreateItemFromIDList(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl,
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)]
            out object v);
        public static T SHCreateItemFromIDList<T>(IdList pidl) where T : class
        {
            object ret;
            int hr = _SHCreateItemFromIDList(pidl, typeof(T).GUID, out ret);
            if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return (T)ret;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetIDListFromObject"), SuppressUnmanagedCodeSecurity]
        private static extern int _SHGetIDListFromObject(
            [MarshalAs(UnmanagedType.IUnknown)]
            object obj,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            out IdList pidl);
        public static IdList SHGetIDListFromObject(object obj)
        {
            IdList ret;
            int hr = _SHGetIDListFromObject(obj, out ret);
            if (hr == ShellConsts.E_NOINTERFACE)
                return null;
            if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return ret;
        }

        [DllImport("Shell32.dll", EntryPoint = "SHGetDesktopFolder"), SuppressUnmanagedCodeSecurity]
        private static extern int _SHGetDesktopFolder(
            [MarshalAs(UnmanagedType.Interface)]
            out IShellFolder desktopFolder);
        public static IShellFolder SHGetDesktopFolder()
        {
            IShellFolder ret;
            int hr = _SHGetDesktopFolder(out ret);
            if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return ret;
        }
        [DllImport("Shell32.dll", EntryPoint = "SHGetNameFromIDList"), SuppressUnmanagedCodeSecurity]
        private static extern int _SHGetNameFromIDList(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl, SIGDN sigdnName,
            [MarshalAs(UnmanagedType.LPWStr)]
            out string name);
        public static string SHGetNameFromIDList(IdList pidl, SIGDN sigdnName)
        {
            string ret;
            int hr = _SHGetNameFromIDList(pidl, sigdnName, out ret);
            if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return ret;
        }
    }
}
