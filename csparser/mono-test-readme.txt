on Mono with --mono option:

gtest-230.cs Line 29 - Expecting ';' (known issue (see known-issues-gmcs))
gtest-284 Test failed! Exit code = 10006 (use Mono-1.2.6 to resolve this)
support-* compile according test-* files

test-416-mod crashes compiler with a BadImageFormatException (see https://bugzilla.novell.com/show_bug.cgi?id=353536)
test-418-2-mod crashes compiler with a BadImageFormatException (see https://bugzilla.novell.com/show_bug.cgi?id=353536)
test-418-3-mod crashes compiler with a BadImageFormatException (see https://bugzilla.novell.com/show_bug.cgi?id=353536)

test-540 Does not compile because it's empty (no Main function defined)

verify-4 is meant to crash
verify-7 is meant to hang


on .NET:

gtest-278 fails on .NET, but works on Mono (like original)

support-* compile according test-* files

test-311.cs fails if compiled with .NET, but not with Mono.

test-473 Does not compile with .NET because of usage of obsolete enums members

test-540 Does not compile because it's empty (no Main function defined)

verify-4 is meant to crash
verify-7 is meant to hang

	
TODO:
 - Local declaration "statements" are always surrounded by an expression statement??
 - Check preprocessor else and elif
 - Check newlines or preprocessor directives after cast...
 - Check all usages of nextTokNode as this may point to a NewLine token!!!
   Probably this should be removed, as Token.Line already has the line information.
   It can also be Whitespace, comment, and hash token...
   Move preprocessor stuff completely into lexer? But what about comments?
 - Check UTF32 identifiers
