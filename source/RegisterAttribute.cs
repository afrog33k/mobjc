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

namespace MObjc
{
	// This is most commonly used to decorate types which are associated with
	// existing cocoa types (e.g. NSApplication or NSRange). It may also be used
	// on methods to explicitly register a method with cocoa or to provide a custom
	// name for the cocoa form of the method.
	//
	// Note that methods in exported classes are automatically registered if they
	// are lower case. The cocoa name is the same as the managed name except 
	// that underscores are replaced with colons and a colon is appended if the
	// method is not nullary.
	[Serializable]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RegisterAttribute : Attribute
	{
		public RegisterAttribute()
		{
		}
		
		public RegisterAttribute(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name is null or empty");
			
			Name = name;
		}
		
		// Optional name to be used by the native code. If it is null the
		// native code will use the managed method/struct/class name.
		[ThreadModel(ThreadModel.Concurrent)]
		public string Name {get; private set;}
	}
}
