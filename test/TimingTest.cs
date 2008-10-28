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
using System.Diagnostics;

#if UNUSED
[TestFixture]
public class TimingTest 	
{
	[TestFixtureSetUp]
	public void Init()
	{
		Registrar.CanInit = true;
		m_pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");
	}
	
	[TestFixtureTearDown]
	public void DeInit()
	{
		if (m_pool != null)
		{
			m_pool.release();
			m_pool = null;
		}
	}
	
	[Test]
	public void Time()	 
	{
		Class klass = new Class("NSString");
		NSObject str1 = (NSObject) klass.Call("stringWithUTF8String:", "hello world");
		NSObject str2 = (NSObject) klass.Call("stringWithUTF8String:", "100");
		NSObject str3 = (NSObject) klass.Call("stringWithUTF8String:", "foo");
		
		Stopwatch timer = Stopwatch.StartNew();
		for (int i = 0; i < 10000; ++i)
		{
			str3.Call("hasPrefix:", str1);
		}
		Console.WriteLine("bool (NSString): {0:0.0} secs", timer.ElapsedMilliseconds/1000.0);
		
		timer = Stopwatch.StartNew();
		for (int i = 0; i < 10000; ++i)
		{
			str2.Call("intValue");
		}
		Console.WriteLine("int (): {0:0.0} secs", timer.ElapsedMilliseconds/1000.0);
		
		timer = Stopwatch.StartNew();
		for (int i = 0; i < 10000; ++i)
		{
			str1.Call("uppercaseString");
		}
		Console.WriteLine("NSString (): {0:0.0} secs", timer.ElapsedMilliseconds/1000.0);
		
		Native native = new Native(str1, new Selector("uppercaseString"));
		timer = Stopwatch.StartNew();
		for (int i = 0; i < 10000; ++i)
		{
			native.Invoke();
		}
		Console.WriteLine("native NSString (): {0:0.0} secs", timer.ElapsedMilliseconds/1000.0);

		string s = "hello world";
		timer = Stopwatch.StartNew();
		for (int i = 0; i < 10000; ++i)
		{
			s.ToUpper();
		}
		Console.WriteLine("ToUpper control: {0:0.0} secs", timer.ElapsedMilliseconds/1000.0);
	}	

	private NSObject m_pool;
}
#endif
