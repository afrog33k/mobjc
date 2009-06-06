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
using System.Runtime.Serialization;

namespace MObjc
{
	/// <summary>Thrown if a call to unmanaged code cannot be performed.</summary>
	/// <remarks>Usually the reason for this is that the unmanaged code is called with
	/// an argument that cannot be marshaled to unmanaged code or the unmanaged method
	/// could not be found (e.g. it may be misspelled).</remarks>
	[Serializable]
	[ThreadModel(ThreadModel.Concurrent)]
	public sealed class InvalidCallException : Exception
	{
		// Need this for XML serialization.
		internal InvalidCallException()
		{
		}
		
		internal InvalidCallException(string text) : base(text)
		{
		}
		
		internal InvalidCallException(string text, Exception inner) : base(text, inner)
		{
		}
		
		private InvalidCallException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
