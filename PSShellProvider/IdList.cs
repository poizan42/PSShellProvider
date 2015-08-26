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
            int len = pidl.Parts.Sum(id => id.Value.Length + 2) + 2;
            IntPtr nativePidl = Marshal.AllocCoTaskMem(len);
            IntPtr pos = nativePidl;
            foreach (var id in pidl.Parts)
            {
                *(ushort*)pos = checked((ushort)(id.Value.Length + sizeof(ushort)));
                Marshal.Copy(id.Value, 0, pos + sizeof(ushort), id.Value.Length);
                pos += id.Value.Length + sizeof(ushort);
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
        public IList<ShellItemId> Parts { get; private set; }

        public IdList (IEnumerable<ShellItemId> parts)
        {
            Parts = parts.ToList().AsReadOnly();
        }

        public IdList (ShellItemId relativeId)
            : this(new ShellItemId[] { relativeId })
        {
        }

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
            var parts = new List<ShellItemId>();
            IntPtr cur = pidl;
            while (true)
            {
                int cb = *(ushort*)cur;
                if (cb == 0)
                    break;
                byte[] item = new byte[cb];
                Marshal.Copy(cur + sizeof(ushort), item, 0, cb - sizeof(ushort));
                cur += cb;
                parts.Add(new ShellItemId(item));
            }
            Parts = parts.AsReadOnly();
        }
       
        public override string ToString()
        {
            return String.Join("\\", Parts);
        }

        public static IdList Parse(string displayName, SFGAO queryForAttributes, out SFGAO attributes)
        {
            return ShellNativeFunctions.SHParseDisplayName(displayName, null, queryForAttributes, out attributes);
        }

        public static IdList Parse(string displayName)
        {
            SFGAO dummy;
            return Parse(displayName, SFGAO.None, out dummy);
        }

        public bool AbsolutePidlEquals(IdList other)
        {
            if (Parts.Count == other.Parts.Count)
            {
                Parts.Zip(other.Parts, (a,b) => a.Equals(b));
            }
            int hr = ShellFolder.DesktopFolder.NativeFolder.CompareIDs(0, this, other);
            bool isError = ((uint)(hr)) >> 31 == 1;
            if (isError)
                Marshal.ThrowExceptionForHR(hr);
            int code = unchecked((int)(((uint)hr) & 0xFFFF));
            return code == 0;
        }
    }
}
