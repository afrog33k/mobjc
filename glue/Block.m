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

void* CreateBlock(void* callback)
{
	id block = NULL;
	
#if NS_BLOCKS_AVAILABLE
	// Make a dummy block, copy it onto the heap, and autorelease it.
	block = [[^ {return 0;} copy] autorelease];
	
	// Swap out the invoke function pointer.
	// TODO: This is pretty evil, but according to Apple's ABI docs it
	// should work on IA-32 but probably not on ppc...
	assert(sizeof(int) == sizeof(void*));
	int* p = (int*) block;		// 0 = isa, 1 = flags, 2 = reserved, 3 = invoke, for details see http://clang.llvm.org/docs/BlockLanguageSpec.txt
	p[3] = (int) callback;
#endif
	
	return block;
}

BOOL BlocksAreAvailable()
{
	BOOL has = NO;
	
#if NS_BLOCKS_AVAILABLE
	// True iff we were compiled against Snow Leopard or later and we are
	// running on Snow Leopard or later.
	has = NSFoundationVersionNumber > NSFoundationVersionNumber10_5_6;
#endif

	return has;
}
