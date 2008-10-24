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
public class FormattedTests 	
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
	public void ClassTarget() 
	{
		NSObject o = (NSObject) Native.Call("[NSString string]");
		
		Assert.AreEqual(0, (int) o.Call("length"));
	}

	[Test]
	public void ArgTarget() 
	{
		Class klass = new Class("NSString");
		NSObject o = (NSObject) Native.Call("[{0} string]", klass);
		
		Assert.AreEqual(0, (int) o.Call("length"));
	}
	
	[Test]
	public void ExprTarget() 
	{
		NSObject o = (NSObject) Native.Call("[[NSString alloc] init]");
		
		Assert.AreEqual(0, (int) o.Call("length"));
	}
	
	[Test]
	public void Operands() 
	{
		NSObject o = (NSObject) Native.Call("[[NSString alloc] initWithCString:{0} encoding:{1}]", "hello", 1);
		
		Assert.AreEqual(5, (int) o.Call("length"));
	}
	
	[Test]
	public void BoolLiteral() 
	{
		NSObject f = (NSObject) Native.Call("[NSNumber numberWithBool:NO]");
		NSObject t = (NSObject) Native.Call("[NSNumber numberWithBool: YES]");
		
		Untyped result = f.Call("boolValue");
		Assert.IsFalse((bool) result);

		result = t.Call("boolValue");
		Assert.IsTrue((bool) result);
	}
	
	[Test]
	public void IntLiteral() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: {0}]", "hello world");

		NSObject str2 = (NSObject) Native.Call("[{0} substringFromIndex: 3]", str1);

		Untyped result = str2.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("lo world", result.Value);
	}

	[Test]
	public void StringLiteral() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello world\"]");

		NSObject str2 = (NSObject) Native.Call("[{0} substringFromIndex: 3]", str1);

		Untyped result = str2.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("lo world", result.Value);
	}
	
	[Test]
	public void ExprArg() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello world\"]");
		NSObject str2 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello \"]");

		NSObject str3 = (NSObject) Native.Call("[{0} substringFromIndex: [{1} length]]", str1, str2);

		Untyped result = str3.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("world", result.Value);
	}
	
	[Test]
	public void RepeatedArg() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello\"]");
		NSObject str2 = (NSObject) Native.Call("[{0} stringByAppendingString: {0}]", str1);

		Untyped result = str2.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("hellohello", result.Value);
	}
		
	[Test]
	public void WhiteSpace() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello world\"]");
		NSObject str2 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello \"]");

		NSObject str3 = (NSObject) Native.Call("  [ {0} substringFromIndex :  [  {1} length  ] ]  ", str1, str2);

		Untyped result = str3.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("world", result.Value);
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MissingIndex() 
	{
		NSObject str1 = (NSObject) Native.Call("[NSString stringWithUTF8String: \"hello world\"]");
		Native.Call("[{0} substringFromIndex: {2}]", str1, 1, 3);
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadArg() 
	{
		Native.Call("[NSNumber numberWithBool:xxx]");
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadSelector2() 
	{
		Native.Call("[[NSString alloc] initWithCString: encoding:{1}]", "hello", 1);
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadSelector3() 
	{
		Native.Call("[[NSString alloc] initWithCString encoding]");
	}
	
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadSelector4() 
	{
		Native.Call("[[NSString alloc] init:]");
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void Extra() 
	{
		Native.Call("[[NSString alloc] init] garbage");
	}
	
	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void TooManyArgs() 
	{
		Class klass = new Class("NSString");
		Native.Call("[{0} string]", klass, 3);
	}
	
	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void TooFewArgs() 
	{
		Native.Call("[{0} string]");
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MissingBracket() 
	{
		Native.Call("[NSString string");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MissingBracket1() 
	{
		Native.Call("NSString string]");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MissingBracket2() 
	{
		Native.Call("[NSString]");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadTarget() 
	{
		Native.Call("[123 init]");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadSelector() 
	{
		Native.Call("NSString xxx]");
	}

	[Test]
	[ExpectedException(typeof(PreconditionException))]
	public void NoExpr() 
	{
		Native.Call("");
	}

	private NSObject m_pool;
}
