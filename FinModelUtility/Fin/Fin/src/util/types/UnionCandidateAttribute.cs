using System;

namespace fin.util.types;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Property)]
public sealed class UnionCandidateAttribute : Attribute;