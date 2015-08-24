using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    [CmdletProvider("Shell", ProviderCapabilities.None)]
    public class ShellProvider : NavigationCmdletProvider
    {
        protected override bool IsValidPath(string path)
        {
            return true;
        }

        protected override void GetItem(string path)
        {
            IdList pidl = IdList.Parse(path);
            ShellItem obj = new ShellItem(pidl);
            WriteItemObject(obj, path == "" ? "\\" : path, false);
        }

        protected override bool ItemExists(string path)
        {
            try
            {
                IdList.Parse(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }
    }
}
