using System;
using System.Management.Automation;

namespace PSShellProvider
{
    internal class ShellPSDriveInfo : PSDriveInfo, IDisposable
    {
        public ShellFolder RootFolder { get; private set; }

        public ShellPSDriveInfo(ShellFolder rootFolder, PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            RootFolder = rootFolder;
        }

        public void Dispose()
        {
            RootFolder.Dispose();
        }
    }
}