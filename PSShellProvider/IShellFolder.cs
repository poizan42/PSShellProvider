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
    [Guid("000214E6-0000-0000-C000-000000000046")]
    internal interface IShellFolder
    {
        /// <summary>
        /// Translates a file object's or folder's display name into an item identifier list.
        /// </summary>
        /// <param name="hwnd">A window handle. The client should provide a window handle if it displays a dialog or message box. Otherwise set hwnd to NULL.</param>
        /// <param name="pbc">Optional. A pointer to a bind context used to pass parameters as inputs and outputs to the parsing function.
        /// These passed parameters are often specific to the data source and are documented by the data source owners. For example, the file system data source accepts the name being parsed (as a WIN32_FIND_DATA structure), using the STR_FILE_SYS_BIND_DATA bind context parameter. STR_PARSE_PREFER_FOLDER_BROWSING can be passed to indicate that URLs are parsed using the file system data source when possible. Construct a bind context object using CreateBindCtx and populate the values using IBindCtx::RegisterObjectParam. See Bind Context String Keys for a complete list of these.
        /// <para />If no data is being passed to or received from the parsing function, this value can be NULL.</param>
        /// <param name="pszDisplayName">A null-terminated Unicode string with the display name.
        /// Because each Shell folder defines its own parsing syntax, the form this string can take may vary.
        /// The desktop folder, for instance, accepts paths such as "C:\My Docs\My File.txt".
        /// It also will accept references to items in the namespace that have a GUID associated with them using the "::{GUID}" syntax.
        /// For example, to retrieve a fully qualified identifier list for the control panel from the desktop folder, you can use the following:
        /// <code>::{CLSID for Control Panel}\::{CLSID for printers folder}</code></param>
        /// <param name="pchEaten">A pointer to a ULONG value that receives the number of characters of the display name that was parsed.
        /// If your application does not need this information, set pchEaten to NULL, and no value will be returned.</param>
        /// <param name="ppidl">When this method returns, contains a pointer to the PIDL for the object. The returned item identifier list specifies the item relative to the parsing folder. If the object associated with pszDisplayName is within the parsing folder, the returned item identifier list will contain only one SHITEMID structure. If the object is in a subfolder of the parsing folder, the returned item identifier list will contain multiple SHITEMID structures. If an error occurs, NULL is returned in this address.</param>
        /// <param name="pdwAttributes">The value used to query for file attributes. If not used, it should be set to NULL. To query for one or more attributes, initialize this parameter with the SFGAO flags that represent the attributes of interest. On return, those attributes that are true and were requested will be set.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [PreserveSig]
        Int32 ParseDisplayName(
            IntPtr hwnd,
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPWStr)]
            String pszDisplayName,
            ref UInt32 pchEaten,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            out IdList ppidl,
            ref SFGAO pdwAttributes);
                                       

        // Allows a client to determine the contents of a folder by creating an item
        // identifier enumeration object and returning its IEnumIDList interface.
        // Return value: error code, if any
        [PreserveSig]
        Int32 EnumObjects(
            IntPtr hwnd,            // If user input is required to perform the
                                    // enumeration, this window handle 
                                    // should be used by the enumeration object as
                                    // the parent window to take 
                                    // user input.
            SHCONTF grfFlags,             // Flags indicating which items to include in the
                                        // enumeration. For a list 
                                        // of possible values, see the SHCONTF enum.
            [MarshalAs(UnmanagedType.Interface)]
            out IEnumIDList ppenumIDList); // Address that receives a pointer to the
                                      // IEnumIDList interface of the 
                                      // enumeration object created by this method. 

        // Retrieves an IShellFolder object for a subfolder.
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object BindToObject(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl,            // Address of an ITEMIDLIST structure (PIDL)
                                    // that identifies the subfolder.
            IBindCtx pbc,                // Optional address of an IBindCtx interface on
                                       // a bind context object to be 
                                       // used during this operation.
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid);

        // Requests a pointer to an object's storage interface. 
        // Return value: error code, if any
        [PreserveSig]
        Int32 BindToStorage(
            IntPtr pidl,            // Address of an ITEMIDLIST structure that
                                    // identifies the subfolder relative 
                                    // to its parent folder. 
            IntPtr pbc,                // Optional address of an IBindCtx interface on a
                                       // bind context object to be 
                                       // used during this operation.
            Guid riid,                  // Interface identifier (IID) of the requested
                                        // storage interface.
            out IntPtr ppv);        // Address that receives the interface pointer specified by riid.

        // Determines the relative order of two file objects or folders, given their
        // item identifier lists. Return value: If this method is successful, the
        // CODE field of the HRESULT contains one of the following values (the code
        // can be retrived using the helper function GetHResultCode): Negative A
        // negative return value indicates that the first item should precede
        // the second (pidl1 < pidl2). 

        // Positive A positive return value indicates that the first item should
        // follow the second (pidl1 > pidl2).  Zero A return value of zero
        // indicates that the two items are the same (pidl1 = pidl2). 
        [PreserveSig]
        Int32 CompareIDs(
            Int32 lParam,               // Value that specifies how the comparison
                                        // should be performed. The lower 
                                        // Sixteen bits of lParam define the sorting rule.
                                        // The upper sixteen bits of 
                                        // lParam are used for flags that modify the
                                        // sorting rule. values can be from 
                                        // the SHCIDS enum
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl1,               // Pointer to the first item's ITEMIDLIST structure.
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl2);              // Pointer to the second item's ITEMIDLIST structure.

        // Requests an object that can be used to obtain information from or interact
        // with a folder object.
        // Return value: error code, if any
        [PreserveSig]
        Int32 CreateViewObject(
            IntPtr hwndOwner,           // Handle to the owner window.
            Guid riid,                  // Identifier of the requested interface. 
            out IntPtr ppv);        // Address of a pointer to the requested interface. 

        // Retrieves the attributes of one or more file objects or subfolders. 
        // Return value: error code, if any
        [PreserveSig]
        Int32 GetAttributesOf(
            UInt32 cidl,            // Number of file objects from which to retrieve
                                    // attributes. 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]
        IntPtr[] apidl,            // Address of an array of pointers to ITEMIDLIST
                                   // structures, each of which 
                                   // uniquely identifies a file object relative to
                                   // the parent folder.
            ref UInt32 rgfInOut);    // Address of a single ULONG value that, on entry,
                                     // contains the attributes that 
                                     // the caller is requesting. On exit, this value
                                     // contains the requested 
                                     // attributes that are common to all of the
                                     // specified objects. this value can
                                     // be from the SFGAO enum

        // Retrieves an OLE interface that can be used to carry out actions on the
        // specified file objects or folders.
        // Return value: error code, if any
        [PreserveSig]
        Int32 GetUIObjectOf(
            IntPtr hwndOwner,        // Handle to the owner window that the client
                                     // should specify if it displays 
                                     // a dialog box or message box.
            UInt32 cidl,            // Number of file objects or subfolders specified
                                    // in the apidl parameter. 
            IntPtr[] apidl,            // Address of an array of pointers to ITEMIDLIST
                                       // structures, each of which 
                                       // uniquely identifies a file object or subfolder
                                       // relative to the parent folder.
            Guid riid,                // Identifier of the COM interface object to return.
            ref UInt32 rgfReserved,    // Reserved. 
            out IntPtr ppv);        // Pointer to the requested interface.

        // Retrieves the display name for the specified file object or subfolder. 
        // Return value: error code, if any
        [PreserveSig]
        int _GetDisplayNameOf(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl,            // Address of an ITEMIDLIST structure (PIDL)
                                    // that uniquely identifies the file 
                                    // object or subfolder relative to the parent folder. 
            SHGDNF uFlags,              // Flags used to request the type of display name
                                        // to return. For a list of 
                                        // possible values, see the SHGNO enum. 
            out StrRetNative value
            );

        // Sets the display name of a file object or subfolder, changing the item
        // identifier in the process.
        // Return value: error code, if any
        [PreserveSig]
        Int32 SetNameOf(
            IntPtr hwnd,            // Handle to the owner window of any dialog or
                                    // message boxes that the client 
                                    // displays.
            IntPtr pidl,            // Pointer to an ITEMIDLIST structure that uniquely
                                    // identifies the file object
                                    // or subfolder relative to the parent folder. 
            [MarshalAs(UnmanagedType.LPWStr)]
        String pszName,            // Pointer to a null-terminated string that
                                   // specifies the new display name. 
            UInt32 uFlags,            // Flags indicating the type of name specified by
                                      // the lpszName parameter. For a list of possible
                                      // values, see the description of the SHGNO enum. 
            out IntPtr ppidlOut);   // Address of a pointer to an ITEMIDLIST structure
                                    // which receives the new ITEMIDLIST. 
    }

    internal static class IShellFolderExtensions
    {
        public static string GetDisplayNameOf(this IShellFolder folder, IdList pidl, SHGDNF uFlags)
        {
            //StrRet value;
            StrRetNative value;
            int hr = folder._GetDisplayNameOf(pidl, uFlags, out value);
            if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            return new StrRet(value).GetValue(pidl);
        }
    }
}
