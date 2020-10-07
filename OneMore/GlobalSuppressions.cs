// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0076 // Invalid global 'SuppressMessageAttribute'

[assembly: SuppressMessage(
	"Major Code Smell", "S125:Sections of code should not be commented out",
	Justification = "Because it's stupid",
	Scope = "namespaceanddescendants", Target = "River.OneMore")]

[assembly: SuppressMessage(
	"Info Code Smell", "S1135:Track uses of \"TODO\" tags", 
	Justification = "Because it's stupid",
	Scope = "namespaceanddescendants", Target = "River.OneMore")]
