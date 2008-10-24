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

using MObjc;
using System;

// http://developer.apple.com/documentation/Cocoa/Reference/ApplicationKit/Classes/NSApplication_Class/Reference/Reference.html
internal sealed class NSApplication : NSObject 
{
	public NSApplication(string nibName) : base((IntPtr) Native.Call("[NSApplication sharedApplication]"))
	{
		// Load our nib. This will instantiate all of the native objects and wire them together.
		// The C# SimpleLayoutView will be created the first time Objective-C calls one of the
		// methods SimpleLayoutView added or overrode.
		NSObject dict = (NSObject) Native.Call("[[NSMutableDictionary alloc] init]");
		NSObject key = (NSObject) Native.Call("[NSString stringWithUTF8String:\"NSOwner\"]");
		Native.Call("[{0} setObject:{1} forKey:{2}]", dict, this, key);

		NSObject bundle = (NSObject) Native.Call("[NSBundle mainBundle]");

		NSObject nib = (NSObject) Native.Call("[NSString stringWithUTF8String:{0}]", nibName);
		bool loaded = (bool) Native.Call("[{0} loadNibFile:{1} externalNameTable:{2} withZone:nil]", bundle, nib, dict);
		if (!loaded)
			throw new InvalidOperationException("Couldn't load the nib file");
		
		// We need an NSAutoreleasePool to do Native.Call, but we don't want to have one
		// hanging around while we're in the main event loop because that may hide bugs.
		// So, we'll instantiate a Native instance here and call Invoke later which can
		// be done without an NSAutoreleasePool.
		m_run = new Native(this, new Selector("run"));
		
		dict.Release();
	}
		
	public void Run()
	{
		m_run.Invoke();
	}

	private Native m_run;
}
