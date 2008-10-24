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
using System.Reflection;
using System.Runtime.InteropServices;

[TestFixture]
public class ArgTests 	
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
	public void IntTest()	 
	{
		Class klass = new Class("NSString");
		NSObject str1 = (NSObject) klass.Call("stringWithUTF8String:", "hello world");

		NSObject str2 = (NSObject) str1.Call("substringFromIndex:", 3);

		Untyped result = str2.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("lo world", result.Value);
	}

	[Test]
	public void UIntTest()	 
	{
		Class klass = new Class("NSString");
		NSObject str1 = (NSObject) klass.Call("stringWithUTF8String:", "hello world");

		NSObject str2 = (NSObject) str1.Call("substringFromIndex:", (uint) 3);

		Untyped result = str2.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("lo world", result.Value);
	}
	
	[Test]
	public void IntanceTest()	 
	{
		Class klass = new Class("NSString");
		NSObject str1 = (NSObject) klass.Call("stringWithUTF8String:", "hello");
		NSObject str2 = (NSObject) klass.Call("stringWithUTF8String:", "goodbye");

		Untyped result = str1.Call("isEqual:", str2);
		Assert.AreEqual(typeof(bool), result.Value.GetType());		
		Assert.IsFalse((bool) result);

		result = str1.Call("isEqual:", str1);
		Assert.AreEqual(typeof(bool), result.Value.GetType());		
		Assert.IsTrue((bool) result);
	}
	
	[Test]
	public void IntPtrTest1()	 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hello");
		
		int len = (int) str.Call("length");

		IntPtr buffer = Marshal.AllocCoTaskMem(2 * len);
		str.Call("getCharacters:", buffer);
		string text = Marshal.PtrToStringUni(buffer, len);

		Assert.AreEqual("hello", text);
		Marshal.FreeCoTaskMem(buffer);
	}
	
	[Test]
	public void IntPtrTest2()	 
	{
		Class klass = new Class("NSData");
		
		byte[] bytes = new byte[]{1, 3, 7};
		NSObject data = (NSObject) klass.Call("dataWithBytes:length:", bytes, bytes.Length);
		
		int len = (int) data.Call("length");
		Assert.AreEqual(3, len);

		byte[] result = new byte[3];
		data.Call("getBytes:", result);

		Assert.AreEqual(1, result[0]);
		Assert.AreEqual(3, result[1]);
		Assert.AreEqual(7, result[2]);
	}
	
	[Test]
	public void StringTest()	 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hello");

		string result = (string) str.Call("UTF8String");
		Assert.AreEqual("hello", result);
	}
	
	[Test]
	public void BoolTest()	 
	{
		Class klass = new Class("NSNumber");
		NSObject f = (NSObject) klass.Call("numberWithBool:", false);
		NSObject t = (NSObject) klass.Call("numberWithBool:", true);
		
		Assert.AreEqual("0", f.Description());
		Assert.AreEqual("1", t.Description());
		
		Untyped result = f.Call("boolValue");
		Assert.IsFalse((bool) result);

		result = t.Call("boolValue");
		Assert.IsTrue((bool) result);
	}

	[Test]
	public void UInt16Test()	 
	{
		Class klass = new Class("NSNumber");
		NSObject f = (NSObject) klass.Call("numberWithUnsignedShort:", (ushort) 300);
		
		Assert.AreEqual("300", f.Description());
	}

	[Test]
	public void ByteTest() 
	{
		Class klass = new Class("NSNumber");
		
		NSObject num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithUnsignedChar:", (byte) 13);
		byte result = (byte) num.Call("unsignedCharValue");
		Assert.AreEqual(13, result);
	}

	[Test]
	public void CharTest() 
	{
		Class klass = new Class("NSNumber");
		
		NSObject num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithUnsignedShort:", (char) 'x');

		UInt16 result1 = (UInt16) num.Call("unsignedShortValue");
		Assert.AreEqual((UInt16) 'x', result1);

		char result = (char) num.Call("unsignedShortValue");
		Assert.AreEqual('x', result);
	}
	
	[Test]
	public void ConstUTF32String() 
	{
		Class nsString = new Class("NSString");
		
		NSObject str = (NSObject) nsString.Call("alloc").Call("initWithCharacters:length:", "hi there", 8);
		
		string result = (string) str.Call("UTF8String");
		Assert.AreEqual("hi there", result);
	}
		
	[Test]
	public void FloatTest() 
	{
		Class klass = new Class("NSNumber");
		
		NSObject num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithFloat:", 3.14f);
		float result = (float) num.Call("floatValue");
		Assert.IsTrue(Math.Abs(3.14 - result) < 0.01);
		
		num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithFloat:", 100);
		result = (float) num.Call("floatValue");
		Assert.IsTrue(Math.Abs(100.0 - result) < 0.01);
	}

	[Test]
	public void StructTest() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "the quick brown fox");
		
		NSRange range = new NSRange();
		range.location = 4;
		range.length = 5;
		
		NSObject iresult = (NSObject) str.Call("substringWithRange:", range);
		string sresult = (string) iresult.Call("UTF8String");
		
		Assert.AreEqual("quick", sresult);
	}

	[Test]
	public void UInt16Test2() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
				
		int result = (int) instance.Call("TakeUInt16", (UInt16) 200);
		Assert.AreEqual(200, result);

		UInt16 result2 = (UInt16) instance.Call("TakeUInt162", (UInt16) 5000);
		Assert.AreEqual(5010, result2);
	}

	[Test]
	public void CharTest2() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
				
		int result = (int) instance.Call("TakeChar", (char) 'x');
		Assert.AreEqual((int) 'x', result);

		result = (int) instance.Call("TakeChar", (UInt16) 'x');
		Assert.AreEqual((int) 'x', result);
	}

	[Test]
	public void StringTest2() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
				
		string result = (string) instance.Call("TakeString", "what");
		Assert.AreEqual("whatwhat", result);
	}

	[Test]
	public void BaseTest() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		instance.Call("initValue");
				
		int result = (int) instance.Call("TakeBase", instance);
		Assert.AreEqual(300, result);
	}

	[Test]
	public void DerivedTest() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		instance.Call("initValue");
				
		int result = (int) instance.Call("TakeDerived", instance);
		Assert.AreEqual(300, result);
	}

	[Test]
	public void DerivedTest2() 
	{
		NSObject super = (NSObject) Native.Call("[[MyBase alloc] init]");
		NSObject derived = (NSObject) Native.Call("[[MyDerived alloc] init]");
				
		NSObject iresult = (NSObject) derived.Call("TakeBase", super);
		string result = (string) iresult.Call("UTF8String");
		Assert.AreEqual("base", result);

		iresult = (NSObject) derived.Call("TakeBase", derived);
		result = (string) iresult.Call("UTF8String");
		Assert.AreEqual("derived", result);

		iresult = (NSObject) derived.Call("TakeDerived", derived);
		result = (string) iresult.Call("UTF8String");
		Assert.AreEqual("derived", result);
	}

	[Test]
	[ExpectedException(typeof(TargetInvocationException))]
	public void DerivedTest3() 
	{
		NSObject super = (NSObject) Native.Call("[[MyBase alloc] init]");
		NSObject derived = (NSObject) Native.Call("[[MyDerived alloc] init]");
				
		derived.Call("TakeDerived", super);
	}

	[Test]
	public void RegisteredClass() 
	{
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		
		NSString s1 = new NSString("hey ");
		NSString s2 = new NSString("there");		
				
		NSString result = new NSString((IntPtr) instance.Call("concat", s1, s2));
		Assert.AreEqual("hey there", result.ToString());
	}

	private NSObject m_pool;
}
