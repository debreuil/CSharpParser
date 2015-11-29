using System.Diagnostics;

namespace DDW
{
    [DebuggerDisplay("ID = {ID}, Line = {Line}, Column = {Col}")]
	public struct Token
	{
      public int Col;
      public int Data; // index into data table

      /// <summary>
		/// While the lexer parses tokens, it checks whether a '&lt;' character
		/// may be a start of a generic type parameter or argument list.
        /// It optimizes the generic parsing.
        /// 
        /// If this field is false, this is not a start of a generic type parameter or argument list.
        /// </summary>
        public bool GenericStart;

      public TokenID ID;

      public bool LastCharWasGreater;
      public int Line;

      /// <summary>
        /// While the lexer parses tokens, it check whether a '?' character
		/// may be a nullable type declaration.
        /// It optimizes the conditionals parsing.
        /// 
        /// If this field is false, this is not a '?' of a nullable type.
        /// </summary>
        public bool NullableDeclaration;

		public Token(TokenID id)
		{
			ID = id;
			Data = -1;
            Line = 0;
            Col = 0;
            GenericStart = false;
            NullableDeclaration = true;
			LastCharWasGreater = false;
		}

		public Token(TokenID id, int data)
		{
			ID = id;
			Data = data;
            Line = 0;
            Col = 0;
            GenericStart = false;
            NullableDeclaration = true;
			LastCharWasGreater = false;
		}

		public Token(TokenID id, int data, int line, int col)
		{
			ID = id;
			Data = data;
            Line = line;
            Col = col;
			GenericStart = false;
            NullableDeclaration = true;
			LastCharWasGreater = false;
		}

        public Token(TokenID id, int line, int col)
        {
            ID = id;
            Data = -1;
            Line = line;
            Col = col;
			GenericStart = false;
            NullableDeclaration = true;
			LastCharWasGreater = false;
		}

		public override string ToString()
		{
			return ID.ToString();
		}

		public string ToLongString()
		{
			return "Token " + ID + " at line: " + Line + ", column: " + Col;
		}
	}
}
