// Machine generated on 2008-11-15
using System;
using System.Runtime.InteropServices;

namespace MObjc
{
	public static class DirectCalls
	{
		// nullary
		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callp(IntPtr instance, IntPtr selector, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Calli(IntPtr instance, IntPtr selector, ref IntPtr exception);

		// unary
		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callpp(IntPtr instance, IntPtr selector, IntPtr arg0, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callpi(IntPtr instance, IntPtr selector, Int32 arg0, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Callip(IntPtr instance, IntPtr selector, IntPtr arg0, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Callii(IntPtr instance, IntPtr selector, Int32 arg0, ref IntPtr exception);

		// binary
		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callppp(IntPtr instance, IntPtr selector, IntPtr arg0, IntPtr arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callppi(IntPtr instance, IntPtr selector, IntPtr arg0, Int32 arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callpip(IntPtr instance, IntPtr selector, Int32 arg0, IntPtr arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static IntPtr Callpii(IntPtr instance, IntPtr selector, Int32 arg0, Int32 arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Callipp(IntPtr instance, IntPtr selector, IntPtr arg0, IntPtr arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Callipi(IntPtr instance, IntPtr selector, IntPtr arg0, Int32 arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Calliip(IntPtr instance, IntPtr selector, Int32 arg0, IntPtr arg1, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")]
		public extern static Int32 Calliii(IntPtr instance, IntPtr selector, Int32 arg0, Int32 arg1, ref IntPtr exception);

	}
}

