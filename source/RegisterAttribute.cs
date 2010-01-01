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
	/// <summary>Used to create an association between a managed type/method and
	/// unmanaged code.</summary>
	/// <remarks>When used with a class or struct then mobjc will create an instance of that
	/// type when it needs to create a managed object to wrap an unmanaged object. When
	/// used on a method of an exported type the Objective-C method name can be customized.
	///
	/// <para/>Note that it's rarely necessary for users to use this attribute: mcocoa already
	/// provides class and struct wrappers and mobjc will automatically register lower-case
	/// methods in exported classes (the Objective-C name is the same as the managed name except 
	/// that underscores are replaced with colons and a colon is appended if the
	/// method is not nullary).</remarks>
	/// <seealso cref = "MObjc.ExportClassAttribute"/>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RegisterAttribute : Attribute
	{
		public RegisterAttribute()
		{
		}
		
		/// <param name = "name">The name of a type ("MyClass") or a method ("initWithFrame:color:").</param>
		public RegisterAttribute(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name is null or empty");
			
			Name = name;
		}
		
		[ThreadModel(ThreadModel.Concurrent)]
		public string Name {get; private set;}
	}
}
