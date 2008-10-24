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

using NUnit.Framework;
using MObjc;
using System;
using System.Runtime.InteropServices;

[TestFixture]
public class NativeCallTests
{
	[TestFixtureSetUp]
	public void Init()
	{
		Registrar.CanInit = true;
		m_pool = (NSObject) Native.Call("[[NSAutoreleasePool alloc] init]");
	}
	
	[TestFixtureTearDown]
	public void DeInit()
	{
		if (m_pool != null)
		{
			m_pool.Release();
			m_pool = null;
		}
	}
	
	[SetUp]
	public void Setup()
	{
		GC.Collect(); 				
		System.Threading.Thread.Sleep(200);	// give the finalizer enough time to run
	}
	
	[Test]
	public void ExceptionTest() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");
		NSObject str = (NSObject) Native.Call("[[NSString alloc] init]");

		array.Call("addObject:", str);

		Untyped result = array.Call("objectAtIndex:", 0);
		Assert.IsTrue(!result.IsNull);

		try
		{
			array.Call("objectAtIndex:", 1);
		}
		catch (CocoaException e)
		{
			Assert.IsTrue(e.Message.Contains("Array"));
			Assert.IsTrue(e.Message.Contains("objectAtIndex"));
		}
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void TooManyArgs() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");
		NSObject str = (NSObject) Native.Call("[[NSString alloc] init]");

		array.Call("addObject:", str, str);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void TooFewArgs() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");

		array.Call("addObject:");
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void BadSelector1() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");

		array.Call("initWithUTF8String::", "hey");
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void BadSelector2() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");

		array.Call("xxx:", 3);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void BadSelector3() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");

		array.Call("initWithUTF8String::", "hey", 2, 3, 4);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void BadArg() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");

		array.Call("addObject:", 3);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void SlowTooManyArgs() 
	{
		NSObject array = (NSObject) Native.Call("[[NSMutableArray alloc] init]");
		NSObject str = (NSObject) Native.Call("[[NSString alloc] init]");

		array.Call("addObject:", str, str, str, str);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void SlowTooFewArgs() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hello world");
		NSObject padding = (NSObject) klass.Call("stringWithUTF8String:", " ");

		str.Call("stringByPaddingToLength:withString:startingAtIndex:", 10, padding);
	}

	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void SlowBadArg() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hello world");
		NSObject padding = (NSObject) klass.Call("stringWithUTF8String:", " ");

		str.Call("stringByPaddingToLength:withString:startingAtIndex:", 10, padding, padding);
	}

	private NSObject m_pool;
}
