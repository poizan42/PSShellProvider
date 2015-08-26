using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    internal interface IShellItem
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object BindToHandler(IBindCtx pbc,
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid);
        [PreserveSig]
        int _GetParent(
            [MarshalAs(UnmanagedType.Interface)]
            out IShellItem parent);

        [return: MarshalAs(UnmanagedType.LPWStr)] 
        string GetDisplayName(SIGDN sigdnName);

        void GetAttributes(SFGAO sfgaoMask, out SFGAO psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    };

    internal static class IShellItemExtensions
    {
        public static IShellItem GetParent(this IShellItem shellItem)
        {
            IShellItem parent;
            int hr = shellItem._GetParent(out parent);
            if (hr == ShellConsts.MK_E_NOOBJECT)
                return null;
            else if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return parent;
        }
    }
}
