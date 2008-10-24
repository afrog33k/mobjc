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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace MObjc
{
	// Thrown by DBC.Assert and FastAssert.
	[Serializable]
	public class AssertException : Exception
	{
		internal AssertException() 
		{
		}

		public AssertException(string text) : base(text) 
		{
		}

		protected AssertException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	// Thrown by DBC.Pre and FastPre.
	[Serializable]
	public class PreconditionException : AssertException
	{
		internal PreconditionException() 
		{
		}

		public PreconditionException(string text) : base(text) 
		{
		}

		protected PreconditionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	// Thrown by DBC.Post and FastPost.
	[Serializable]
	public class PostconditionException : AssertException
	{
		internal PostconditionException() 
		{
		}

		public PostconditionException(string text) : base(text) 
		{
		}

		protected PostconditionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	// Asserts used to verify the contract between the caller of
	// a method and the method.
	//
	// In the Design By Contract methodology methods act as a contract between
	// the caller of the method and the method itself. The client guarantees
	// to call the method correctly and the method guarantees to some work
	// if called correctly (or throw an exception if it is unable to do so).
	//
	// The client side of the contract is checked by the DBC.Pre methods. The
	// method side of the contract is checked with DBC.Post methods. The DBC.
	// Assert methods can be used to verify that the state of the world is as
	// expected within a method. There are also Fast versions of these methods
	// which are compiled out in release. These should only be used for code
	// that is known to be time critical.
	//
	// DBC also makes use of invariants, but these are outside the scope of the
	// DBC class. See http://archive.eiffel.com/doc/manuals/technology/contract/
	// for more details.
	public static class DBC
	{		
		public static void Pre(bool f, string s)									{if (!f) throw new PreconditionException(s);}
		public static void Pre<A1>(bool f, string format, A1 arg1)					{if (!f) throw new PreconditionException(string.Format(format, arg1));}
		public static void Pre<A1, A2>(bool f, string format, A1 arg1, A2 arg2)		{if (!f) throw new PreconditionException(string.Format(format, arg1, arg2));}
		public static void Pre(bool f, string format, params object[] args)			{if (!f) throw new PreconditionException(string.Format(format, args));}

		public static void Post(bool f, string s)									{if (!f) throw new PostconditionException(s);}
		public static void Post<A1>(bool f, string format, A1 arg1)					{if (!f) throw new PostconditionException(string.Format(format, arg1));}
		public static void Post<A1, A2>(bool f, string format, A1 arg1, A2 arg2)	{if (!f) throw new PostconditionException(string.Format(format, arg1, arg2));}
		public static void Post(bool f, string format, params object[] args)		{if (!f) throw new PostconditionException(string.Format(format, args));}

		public static void Assert(bool f, string s)									{if (!f) throw new AssertException(s);}
		public static void Assert<A1>(bool f, string format, A1 arg1)				{if (!f) throw new AssertException(string.Format(format, arg1));}
		public static void Assert<A1, A2>(bool f, string format, A1 arg1, A2 arg2)	{if (!f) throw new AssertException(string.Format(format, arg1, arg2));}
		public static void Assert(bool f, string format, params object[] args)		{if (!f) throw new AssertException(string.Format(format, args));}

		public static void Fail(string s)											{throw new AssertException(s);}
		public static void Fail<A1>(string format, A1 arg1)							{throw new AssertException(string.Format(format, arg1));}
		public static void Fail<A1, A2>(string format, A1 arg1, A2 arg2)			{throw new AssertException(string.Format(format, arg1, arg2));}
		public static void Fail(string format, params object[] args)				{throw new AssertException(string.Format(format, args));}

		[Conditional("DEBUG")]
		public static void FastPre(bool f, string s)									{if (!f) throw new PreconditionException(s);}
#if !DEBUG		// Conditional doesn't work with generic methods in mono 1.2.5			
		[Conditional("DEBUG")]
		public static void FastPre<A1>(bool f, string format, A1 arg1)					{if (!f) throw new PreconditionException(string.Format(format, arg1));}
		[Conditional("DEBUG")]
		public static void FastPre<A1, A2>(bool f, string format, A1 arg1, A2 arg2)		{if (!f) throw new PreconditionException(string.Format(format, arg1, arg2));}
#endif
		[Conditional("DEBUG")]
		public static void FastPre(bool f, string format, params object[] args)			{if (!f) throw new PreconditionException(string.Format(format, args));}

		[Conditional("DEBUG")]
		public static void FastPost(bool f, string s)									{if (!f) throw new PostconditionException(s);}
#if !DEBUG					
		[Conditional("DEBUG")]
		public static void FastPost<A1>(bool f, string format, A1 arg1)					{if (!f) throw new PostconditionException(string.Format(format, arg1));}
		[Conditional("DEBUG")]
		public static void FastPost<A1, A2>(bool f, string format, A1 arg1, A2 arg2)	{if (!f) throw new PostconditionException(string.Format(format, arg1, arg2));}
#endif
		[Conditional("DEBUG")]
		public static void FastPost(bool f, string format, params object[] args)		{if (!f) throw new PostconditionException(string.Format(format, args));}

		[Conditional("DEBUG")]
		public static void FastAssert(bool f, string s)									{if (!f) throw new AssertException(s);}
#if !DEBUG					
		[Conditional("DEBUG")]
		public static void FastAssert<A1>(bool f, string format, A1 arg1)				{if (!f) throw new AssertException(string.Format(format, arg1));}
		[Conditional("DEBUG")]
		public static void FastAssert<A1, A2>(bool f, string format, A1 arg1, A2 arg2)	{if (!f) throw new AssertException(string.Format(format, arg1, arg2));}
#endif
		[Conditional("DEBUG")]
		public static void FastAssert(bool f, string format, params object[] args)		{if (!f) throw new AssertException(string.Format(format, args));}

		[Conditional("DEBUG")]
		public static void FastFail(string s)											{throw new AssertException(s);}
#if !DEBUG					
		[Conditional("DEBUG")]
		public static void FastFail<A1>(string format, A1 arg1)							{throw new AssertException(string.Format(format, arg1));}
		[Conditional("DEBUG")]
		public static void FastFail<A1, A2>(string format, A1 arg1, A2 arg2)			{throw new AssertException(string.Format(format, arg1, arg2));}
#endif
		[Conditional("DEBUG")]
		public static void FastFail(string format, params object[] args)				{throw new AssertException(string.Format(format, args));}
	}
}