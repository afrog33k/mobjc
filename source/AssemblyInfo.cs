using System;
using System.Reflection;
using System.Runtime.CompilerServices; 
using System.Runtime.InteropServices;
using System.Security.Permissions;
 
[assembly: AssemblyTitle("obj3.sharp library")]    
[assembly: AssemblyDescription(".NET <--> Objective C bridge")]
[assembly: AssemblyCopyright("Copyright (C) 2008 Jesse Jones")]
 
[assembly: CLSCompliant(false)]			// It would be nice to be CLS compliant but we can't use unsigned ints then which causes issues with interop            
[assembly: ComVisible(false)]             
[assembly: PermissionSet(SecurityAction.RequestMinimum, Unrestricted = true)]
