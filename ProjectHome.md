mobjc is a Mono library which allows .NET code to interoperate with Apple frameworks such as foundation and appkit. It makes it possible for managed code to call native Objective-C methods, native methods to call managed code, and new Objective-C classes to be defined in managed code. Exceptions are properly marshaled in both directions.

mobjc may be used by itself, but it's much nicer to use [mcocoa](http://code.google.com/p/mcocoa/) which is built on top of mobjc and provides (mostly) type-safe wrappers for foundation and appkit.

mobjc requires Mac OS 10.5 or later.