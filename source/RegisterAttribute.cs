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

namespace MObjc
{
	// Used to mark structs that need to be marshaled to and from native code or
	// classes which may be used as managed method argument types.
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RegisterAttribute : Attribute
	{		
		public RegisterAttribute()
		{
		}
		
		public RegisterAttribute(string name) 
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name is null or empty");
								
			m_name = name;
		}
		
		// The name of the Objective-C type, e.g. _NSRange or NSString.
		// Maybe be null in which case the .NET type name is used.
		public string Name
		{
			get {return m_name;}
		}
	
		private string m_name;
	}
}
