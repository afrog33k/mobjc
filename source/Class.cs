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

using MObjc.Helpers;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MObjc
{
	/// <summary>Wrapper for the Objective-C <c>Class</c> type.</summary>
	/// <remarks>See the <a href = "http://developer.apple.com/documentation/Cocoa/Reference/ObjCRuntimeRef/Reference/reference.html">Objective-C 2.0 Runtime Reference</a> for more information.</remarks>
	[ThreadModel(ThreadModel.Concurrent)]
	public sealed class Class : NSObject
	{
		/// <param name = "name">The name of the class, e.g. "NSString".</param>
		public Class(string name) : base(DoGetDefinition(name))
		{
			m_name = name;
		}
		
		/// <param name = "klass">An unmanaged <c>Class</c> pointer.</param>
		public Class(IntPtr klass) : base(klass)
		{
			if ((IntPtr) this != IntPtr.Zero)
			{
				IntPtr ptr = class_getName((IntPtr) this);
				m_name = Marshal.PtrToStringAnsi(ptr);
			}
			else
				m_name = "(null)";
		}
		
		/// <summary>Used to allocate an uninitialized Objective-C instance.</summary>
		/// <remarks>The new instance will have a reference count of one. Note that
		/// this is preferred over <c>alloc</c> because mcocoa generates <c>Alloc></c>
		/// methods for each type which return the type instead of NSObject.</remarks>
		public NSObject Alloc()
		{
			IntPtr exception = IntPtr.Zero;
			IntPtr instance = DirectCalls.Callp(this, Selector.Alloc, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
			
			return NSObject.Lookup(instance);
		}
		
		/// <summary>Returns the name of the class, e.g. "NSString".</summary>
		public string Name
		{
			get {return m_name;}
		}
		
		/// <remarks>The base class of the NSObject class is null.</remarks>
		public Class BaseClass
		{
			get {return new Class(GetBaseClass());}
		}
		
		/// <summary>Returns the name of the class, e.g. "NSString".</summary>
		/// <remarks>This ignores the normal <see cref = "NSObject">NSObject</see> format specifiers like ":D".</remarks>
		public override string ToString()
		{
			return m_name;
		}
		
		#region Private Methods
		private static IntPtr DoGetDefinition(string name)
		{
			Contract.Requires(!string.IsNullOrEmpty(name), "name is null or empty");
			
			IntPtr klass = objc_getClass(name);
			
			if (klass == IntPtr.Zero)
				throw new ArgumentException("Couldn't get a class for " + name);
				
			return klass;
		}
		#endregion
		
		#region P/Invokes
		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr objc_getClass(string name);
		
		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getName(IntPtr klass);
		#endregion
		
		#region Fields
		private readonly string m_name;
		#endregion
	}
}
