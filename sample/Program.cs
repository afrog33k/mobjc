// Copyright (C) 2008 Jesse Jones
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// This is based on the AnimatingViews AppKit example from the developer examples.
using MObjc;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal static class Program
{
	internal static void Main(string[] args)
	{
		try
		{
			Registrar.CanInit = true;
			
			// Make our app a foreground app (this is redundant if we were started via the
			// Finder or the open command, but important if we were started by directly
			// executing the launcher script).
			var psn = new ProcessSerialNumber();
			psn.highLongOfPSN = 0;
			psn.lowLongOfPSN = kCurrentProcess;
			
			int err = TransformProcessType(ref psn, kProcessTransformToForegroundApplication);
			if (err != 0)
				throw new InvalidOperationException("TransformProcessType returned " + err + ".");
			
			err = SetFrontProcess(ref psn);
			if (err != 0)
				throw new InvalidOperationException("SetFrontProcess returned " + err + ".");
		
			// Load the nib and run the main event loop.
			NSObject pool = new NSObject(NSObject.AllocNative("NSAutoreleasePool"));
			App app = new App("MainMenu.nib");
			pool.release();
			
			app.Run();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
	
	#region P/Invokes
	private const uint kCurrentProcess = 2;
	private const uint kProcessTransformToForegroundApplication = 1;
	
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	private struct ProcessSerialNumber
	{
		public uint highLongOfPSN;
		public uint lowLongOfPSN;
	}
	
	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	private static extern int TransformProcessType(ref ProcessSerialNumber psn, uint type);
	
	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	private static extern short SetFrontProcess(ref ProcessSerialNumber psn);
	#endregion
}
