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

#include <Foundation/Foundation.h>
#include <objc/message.h>

typedef void* (*Methodp)(id, SEL);
void* Callp(id instance, SEL selector, id* exception)
{
	void* result;

	@try
	{
		Methodp method = (Methodp) objc_msgSend;
		result = method(instance, selector);
	}
	@catch (id e)
	{
		*exception = e;
	}
	
	return result;
}

typedef int (*Methodi)(id, SEL);
int Calli(id instance, SEL selector, id* exception)
{
	int result;

	@try
	{
		Methodi method = (Methodi) objc_msgSend;
		result = method(instance, selector);
	}
	@catch (id e)
	{
		*exception = e;
	}
	
	return result;
}

typedef int (*Methodii)(id, SEL, int);
int Callii(id instance, SEL selector, int arg0, id* exception)
{
	int result;

	@try
	{
		Methodii method = (Methodii) objc_msgSend;
		result = method(instance, selector, arg0);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef void* (*Methodpi)(id, SEL, int);
void* Callpi(id instance, SEL selector, int arg0, id* exception)
{
	void* result;

	@try
	{
		Methodpi method = (Methodpi) objc_msgSend;
		result = method(instance, selector, arg0);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef int (*Methodip)(id, SEL, void*);
int Callip(id instance, SEL selector, void* arg0, id* exception)
{
	int result;

	@try
	{
		Methodip method = (Methodip) objc_msgSend;
		result = method(instance, selector, arg0);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef void* (*Methodpp)(id, SEL, void*);
void* Callpp(id instance, SEL selector, void* arg0, id* exception)
{
	void* result;

	@try
	{
		Methodpp method = (Methodpp) objc_msgSend;
		result = method(instance, selector, arg0);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

