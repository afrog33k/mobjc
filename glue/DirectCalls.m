// Machine generated on 2008-11-15
#include <Foundation/Foundation.h>
#include <objc/message.h>

// nullary
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

// unary
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

// binary
typedef void* (*Methodppp)(id, SEL, void*, void*);
void* Callppp(id instance, SEL selector, void* arg0, void* arg1, id* exception)
{
	void* result;

	@try
	{
		Methodppp method = (Methodppp) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef void* (*Methodppi)(id, SEL, void*, int);
void* Callppi(id instance, SEL selector, void* arg0, int arg1, id* exception)
{
	void* result;

	@try
	{
		Methodppi method = (Methodppi) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef void* (*Methodpip)(id, SEL, int, void*);
void* Callpip(id instance, SEL selector, int arg0, void* arg1, id* exception)
{
	void* result;

	@try
	{
		Methodpip method = (Methodpip) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef void* (*Methodpii)(id, SEL, int, int);
void* Callpii(id instance, SEL selector, int arg0, int arg1, id* exception)
{
	void* result;

	@try
	{
		Methodpii method = (Methodpii) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef int (*Methodipp)(id, SEL, void*, void*);
int Callipp(id instance, SEL selector, void* arg0, void* arg1, id* exception)
{
	int result;

	@try
	{
		Methodipp method = (Methodipp) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef int (*Methodipi)(id, SEL, void*, int);
int Callipi(id instance, SEL selector, void* arg0, int arg1, id* exception)
{
	int result;

	@try
	{
		Methodipi method = (Methodipi) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef int (*Methodiip)(id, SEL, int, void*);
int Calliip(id instance, SEL selector, int arg0, void* arg1, id* exception)
{
	int result;

	@try
	{
		Methodiip method = (Methodiip) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

typedef int (*Methodiii)(id, SEL, int, int);
int Calliii(id instance, SEL selector, int arg0, int arg1, id* exception)
{
	int result;

	@try
	{
		Methodiii method = (Methodiii) objc_msgSend;
		result = method(instance, selector, arg0, arg1);
	}
	@catch (id e)
	{
		*exception = e;
	}

	return result;
}

