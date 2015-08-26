namespace PSShellProvider
{
    public class ShellItemId
    {
        public byte[] Value { get; private set; }
        public ShellItemId(byte[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Utils.ByteArrayToHex(Value);
        }

        public override bool Equals(object obj)
        {
            ShellItemId other = obj as ShellItemId;
            if (other == null)
                return false;
            byte[] v1 = Value;
            byte[] v2 = other.Value;
            if (v1.Length != v2.Length)
                return false;
            for (int i = 0; i < v1.Length; i++)
            {
                if (v1 != v2)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}