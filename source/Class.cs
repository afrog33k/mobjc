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

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MObjc
{
	using id = IntPtr;
	using SEL = IntPtr;

	// Represents a class definition.
	[DisableRuleAttribute("D1041", "CircularReference")]
	public sealed class Class : NSObject
	{
		// Name should be something like "NSString".
		public Class(string name) : base(DoGetDefinition(name))
		{
			m_name = name;
		}
		
		public Class(id klass) : base(klass)
		{
			if ((IntPtr) this != IntPtr.Zero)
			{
				IntPtr ptr = class_getName((IntPtr) this);
				m_name = Marshal.PtrToStringAnsi(ptr);
			}
			else
				m_name = "(null)";
		}
							
		public NSObject Alloc()
		{
			return new NSObject(Call("alloc"));
		}

		public string Name
		{
			get {return m_name;}
		}
				
		// Base class of NSObject is null.
		public Class BaseClass
		{
			get {return new Class(GetBaseClass());}
		}
												
		public override string ToString()
		{
			return m_name;
		}
						
		#region Private Methods -----------------------------------------------
		private static IntPtr DoGetDefinition(string name)
		{
			DBC.Pre(!string.IsNullOrEmpty(name), "name is null or empty");
				
			IntPtr klass = objc_getClass(name);
			
			if (klass == IntPtr.Zero)
				throw new ArgumentException("Couldn't get a class for " + name);
				
			return klass;
		}
		#endregion
		
		#region P/Invokes -----------------------------------------------------
		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr objc_getClass(string name);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getName(id klass);
		#endregion
		
		#region Fields --------------------------------------------------------
		private string m_name;
		#endregion
	}
}
