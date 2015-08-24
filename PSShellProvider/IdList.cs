using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    internal class IdListMarshaler : ICustomMarshaler
    {
        private bool freeData;

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new IdListMarshaler(cookie);
        }

        public IdListMarshaler(string cookie)
        {
            freeData = cookie != "ownsdata=false";
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (freeData && pNativeData != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pNativeData);
        }

        public int GetNativeDataSize()
        {
            throw new NotImplementedException();
        }

        public unsafe IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null)
                return IntPtr.Zero;
            IdList pidl = (IdList)ManagedObj;
            int len = pidl.Parts.Sum(id => id.Length + 2) + 2;
            IntPtr nativePidl = Marshal.AllocCoTaskMem(len);
            IntPtr pos = nativePidl;
            foreach (byte[] id in pidl.Parts)
            {
                *(ushort*)pos = checked((ushort)(id.Length + sizeof(ushort)));
                Marshal.Copy(id, 0, pos + sizeof(ushort), id.Length);
                pos += id.Length + sizeof(ushort);
            }
            *(ushort*)pos = 0;
            return nativePidl;
        }

        public unsafe object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            return new IdList(pNativeData);
        }
    }

    public class IdList
    {
        public IList<byte[]> Parts { get; private set; }

        public unsafe IdList(byte[] pidl)
        {
            fixed (byte* pidl0 = pidl)
                Load((IntPtr)pidl0);
        }

        public unsafe IdList(IntPtr pidl)
        {
            Load(pidl);
        }

        private unsafe void Load(IntPtr pidl)
        {
            var parts = new List<byte[]>();
            IntPtr cur = pidl;
            while (true)
            {
                int cb = *(ushort*)cur;
                if (cb == 0)
                    break;
                byte[] item = new byte[cb];
                Marshal.Copy(cur + sizeof(ushort), item, 0, cb - sizeof(ushort));
                cur += cb;
                parts.Add(item);
            }
            Parts = parts.AsReadOnly();
        }
       
        public override string ToString()
        {
            return String.Join("\\", Parts.Select(id => Utils.ByteArrayToHex(id)));
        }

        [DllImport("Shell32", CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        private static extern int SHParseDisplayName(string name, IntPtr pbc, out IntPtr ppidl, SFGAO sfgaoIn, out SFGAO psfgaoOut);

        public static IdList Parse(string displayName, SFGAO queryForAttributes, out SFGAO attributes)
        {
            IntPtr pidl = IntPtr.Zero;
            try
            {
                int hr = SHParseDisplayName(displayName, IntPtr.Zero, out pidl, queryForAttributes, out attributes);
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
                return new IdList(pidl);
            }
            finally
            {
                if (pidl != IntPtr.Zero)
                    Marshal.FreeCoTaskMem((IntPtr)pidl);
            }
        }

        public static IdList Parse(string displayName)
        {
            SFGAO dummy;
            return Parse(displayName, SFGAO.None, out dummy);
        }
    }
}
