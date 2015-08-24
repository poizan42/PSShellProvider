using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    public class ShellItem
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromIDList(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl,
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.Interface)]
            out IShellItem v);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetIDListFromObject(
            [MarshalAs(UnmanagedType.IUnknown)]
            object obj,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            out IdList pidl);

        private IShellItem nativeItem;

        public IdList Pidl
        {
            get
            {
                IdList pidl;
                int hr = SHGetIDListFromObject(nativeItem, out pidl);
                if (hr == ShellConsts.E_NOINTERFACE)
                    return null;
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
                return pidl;
            }
        }

        internal ShellItem(IShellItem nativeItem)
        {
            this.nativeItem = nativeItem;
        }

        public ShellItem(IdList pidl)
        {
            int hr = SHCreateItemFromIDList(pidl, typeof(IShellItem).GUID, out nativeItem);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
        }
    }
}
