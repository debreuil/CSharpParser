namespace DDW
{
	public enum PreprocessorID : byte
	{
		Empty	= 0x00,
		Define	= 0x01,
		Undef	= 0x02,
		If		= 0x03,
		Elif	= 0x04,
		Else	= 0x05,
		Endif	= 0x06,
		Line	= 0x07,
		Error	= 0x08,
		Warning	= 0x09,
		Region	= 0x0A,
		Endregion = 0x0B,
		Pragma	= 0x0C,
	}
}
