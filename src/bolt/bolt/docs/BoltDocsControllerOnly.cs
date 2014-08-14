using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class BoltDocsControllerOnly : Attribute { }