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
	// Base class for NewMethodAttribute and OverrideMethodAttribute.
	public abstract class MethodAttribute : Attribute
	{		
		// Selector name will have the same name as the method name.
		protected MethodAttribute(bool isOverride)
		{
			m_override = isOverride;
		}
		
		// Selector name will be named name.
		protected MethodAttribute(string name, bool isOverride)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name is null or empty");
				
			m_name = name;
			m_override = isOverride;
		}
				
		// May return null.
		public string Name
		{
			get {return m_name;}
		}
		
		internal bool IsOverride
		{
			get {return m_override;}
		}
		
		private string m_name;
		private bool m_override;
	}
}
