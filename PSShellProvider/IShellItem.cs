using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    public enum SIGDN : uint {
        NormalDisplay                = 0x00000000,
        ParentRelativeParsing        = 0x80018001,
        DesktopAbsoluteParsing       = 0x80028000,
        ParentRelativeEditing        = 0x80031001,
        DesktopAbsoluteEditing       = 0x8004c000,
        FileSysPath                  = 0x80058000,
        Url                          = 0x80068000,
        ParentRelativeForAddressBar  = 0x8007c001,
        ParentRelative               = 0x80080001,
        ParentRelativeForUi          = 0x80094001
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    internal interface IShellItem
    {
        void BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    };
}
