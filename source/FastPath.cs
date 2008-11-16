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
	internal static class FastPath
	{
		public static bool Try(IntPtr instance, Selector selector, MethodSignature sig, object[] args, out object result)	// thread safe
		{		
			if (args.Length == 0 && DoTryVoidFastPath(instance, selector, sig, out result))
				return true;
				
			if (args.Length == 1)
			{
				if (args[0] is int && DoTryIntFastPath(instance, selector, (int) args[0], sig, out result))
					return true;
				
				if (args[0] is uint && DoTryUIntFastPath(instance, selector, (uint) args[0], sig, out result))
					return true;

				if (args[0] is IntPtr && DoTryPtrFastPath(instance, selector, (IntPtr) args[0], sig, out result))
					return true;

				if (args[0] is NSObject && DoTryPtrFastPath(instance, selector, (IntPtr) (NSObject) args[0], sig, out result))
					return true;

				if (args[0] is Selector && DoTryPtrFastPath(instance, selector, (IntPtr) (Selector) args[0], sig, out result))
					return true;
			}
						
			return false;
		}
		
		private static bool DoTryVoidFastPath(IntPtr instance, IntPtr selector, MethodSignature sig, out object result)
		{
			bool done = true;
			
			IntPtr exception = IntPtr.Zero, ip;
			switch (sig.ToString())
			{
				case "v@:":
					ip = DirectCalls.Callp(instance, selector, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = null;
					break;

				case "i@:":
				case "l@:":
					result = DirectCalls.Calli(instance, selector, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
					
				case "I@:":
				case "L@:":
					result = unchecked((uint) DirectCalls.Calli(instance, selector, ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
															
				case "@@:":
					ip = DirectCalls.Callp(instance, selector, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = NSObject.Lookup(ip);
					break;

				case "#@:":
					ip = DirectCalls.Callp(instance, selector, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Class(ip);
					break;

				case ":@:":
					ip = DirectCalls.Callp(instance, selector, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Selector(ip);
					break;
					
				default:
					done = false;
					result = null;
					break;
			}		
			
			return done;
		}
		
		private static bool DoTryIntFastPath(IntPtr instance, IntPtr selector, int arg, MethodSignature sig, out object result)
		{
			bool done = true;
			
			IntPtr exception = IntPtr.Zero, ip;
			switch (sig.ToString())
			{
				case "v@:i":
				case "v@:l":
					ip = DirectCalls.Callpi(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = null;
					break;

				case "i@:i":
				case "i@:l":
				case "l@:i":
				case "l@:l":
					result = DirectCalls.Callii(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
					
				case "I@:i":
				case "I@:l":
				case "L@:i":
				case "L@:l":
					result = unchecked((uint) DirectCalls.Callii(instance, selector, arg, ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
															
				case "@@:i":
				case "@@:l":
					ip = DirectCalls.Callpi(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = NSObject.Lookup(ip);
					break;

				case "#@:i":
				case "#@:l":
					ip = DirectCalls.Callpi(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Class(ip);
					break;

				case ":@:i":
				case ":@:l":
					ip = DirectCalls.Callpi(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Selector(ip);
					break;
					
				default:
					done = false;
					result = null;
					break;
			}		
			
			return done;
		}

		private static bool DoTryUIntFastPath(IntPtr instance, IntPtr selector, uint arg, MethodSignature sig, out object result)
		{
			bool done = true;
			
			IntPtr exception = IntPtr.Zero, ip;
			switch (sig.ToString())
			{
				case "v@:I":
				case "v@:L":
					ip = DirectCalls.Callpi(instance, selector, unchecked((int) arg), ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = null;
					break;

				case "i@:I":
				case "i@:L":
				case "l@:I":
				case "l@:L":
					result = DirectCalls.Callii(instance, selector, unchecked((int) arg), ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
					
				case "I@:I":
				case "I@:L":
				case "L@:I":
				case "L@:L":
					result = unchecked((uint) DirectCalls.Callii(instance, selector, unchecked((int) arg), ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
															
				case "@@:I":
				case "@@:L":
					ip = DirectCalls.Callpi(instance, selector, unchecked((int) arg), ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = NSObject.Lookup(ip);
					break;

				case "#@:I":
				case "#@:L":
					ip = DirectCalls.Callpi(instance, selector, unchecked((int) arg), ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Class(ip);
					break;

				case ":@:I":
				case ":@:L":
					ip = DirectCalls.Callpi(instance, selector, unchecked((int) arg), ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Selector(ip);
					break;
					
				default:
					done = false;
					result = null;
					break;
			}		
			
			return done;
		}

		private static bool DoTryPtrFastPath(IntPtr instance, IntPtr selector, IntPtr arg, MethodSignature sig, out object result)
		{
			bool done = true;
			
			IntPtr exception = IntPtr.Zero, ip;
			switch (sig.ToString())
			{
				case "v@:@":
				case "v@:#":
				case "v@::":
				case "v@:*":
				case "v@:r*":
				case "v@:r^S":
					ip = DirectCalls.Callpp(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = null;
					break;

				case "i@:@":
				case "i@:#":
				case "i@::":
				case "i@:*":
				case "i@:r*":
				case "i@:r^S":
					result = DirectCalls.Callip(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
					
				case "I@:@":
				case "I@:#":
				case "I@::":
				case "I@:*":
				case "I@:r*":
				case "I@:r^S":
					result = unchecked((uint) DirectCalls.Callip(instance, selector, arg, ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
					break;
															
				case "@@:@":
				case "@@:#":
				case "@@::":
				case "@@:*":
				case "@@:r*":
				case "@@:r^S":
					ip = DirectCalls.Callpp(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = NSObject.Lookup(ip);
					break;

				case "#@:@":
				case "#@:#":
				case "#@::":
				case "#@:*":
				case "#@:r*":
				case "#@:r^S":
					ip = DirectCalls.Callpp(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Class(ip);
					break;

				case ":@:@":
				case ":@:#":
				case ":@::":
				case ":@:*":
				case ":@:r*":
				case ":@:r^S":
					ip = DirectCalls.Callpp(instance, selector, arg, ref exception);
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
	
					result = new Selector(ip);
					break;
					
				default:
					done = false;
					result = null;
					break;
			}		
			
			return done;
		}
	}
}
