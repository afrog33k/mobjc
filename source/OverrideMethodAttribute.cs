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
	// Use this to associate a .NET method with an overriden Objective-C
	// method. Note that the declaring type should be decorated with
	// ExportClassAttribute. 
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[DisableRuleAttribute("D1032", "NotInstantiated")]
	public sealed class OverrideMethodAttribute : MethodAttribute
	{		
		// Selector name will have the same name as the method name.
		public OverrideMethodAttribute() : base(true)
		{
		}
		
		// Selector name will be named name.
		public OverrideMethodAttribute(string name) : base(name, true)
		{
		}
	}
}
