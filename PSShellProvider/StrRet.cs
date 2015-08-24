using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    class StrRet
    {
        private string value;
        private int offset;

        public StrRet(string value)
        {
            this.value = value;
        }

        public StrRet(int offset)
        {
            this.offset = offset;
        }

        public string GetValue(IdList list)
        {
            if (value != null)
                return value;
            int curOffset = 0;
            int idx = 0;
            while (curOffset < offset)
            {
                byte[] curId = list.Parts[idx];
                if (curOffset + 2 + curId.Length > offset)
                {
                    return GetValueFrom(list, idx, offset-curOffset);
                }
                curOffset += curId.Length + 2;
                idx++;
            }
            throw new ArgumentException(String.Format("The offset {0} is not within the IdList", offset), "list");
        }

        private string GetValueFrom(IdList list, int startIdx, int startOffset)
        {
            StringBuilder sb = new StringBuilder();
            int innerOffset = startOffset - 2;
            int idIdx = startIdx;
            List<byte> ansiStr = new List<byte>();
            while (true)
            {
                byte[] curId = list.Parts[idIdx];
                byte[] cb = BitConverter.GetBytes((ushort)(curId.Length + sizeof(ushort)));
                for (; innerOffset < curId.Length; innerOffset++)
                {
                    byte b;
                    if (innerOffset < 0)
                        b = cb[innerOffset + 2];
                    else
                        b = curId[innerOffset];
                    if (b == 0)
                    {
                        return Encoding.Default.GetString(ansiStr.ToArray());
                    }
                    else
                    {
                        ansiStr.Add(b);
                    }
                }
                innerOffset = -2;
                idIdx++;
            }
        }
    }

    class StrRetMarshaler : ICustomMarshaler
    {
        private enum StrRetType {
            WStr	= 0,
            Offset	= 0x1,
            CStr	= 0x2
        }

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new StrRetMarshaler();
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }

        public int GetNativeDataSize()
        {
            throw new NotImplementedException();
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            throw new NotImplementedException();
        }

        public unsafe object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            var type = *((StrRetType*)pNativeData);
            IntPtr dataStart = pNativeData + sizeof(StrRetType);
            switch (type)
            {
                case StrRetType.CStr:
                    return new StrRet(Marshal.PtrToStringAnsi(dataStart, 260)); //MAX_PATH
                case StrRetType.WStr:
                    IntPtr strAddr = *(IntPtr*)dataStart;
                    try
                    {
                        return new StrRet(Marshal.PtrToStringUni(strAddr));
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(strAddr);
                    }
                case StrRetType.Offset:
                    return new StrRet(*(int*)dataStart);
                default:
                    throw new ArgumentException(String.Format("Invalid type of StrRet: {0}", (int)type), "pNativeData");
            }
        }
    }
}
