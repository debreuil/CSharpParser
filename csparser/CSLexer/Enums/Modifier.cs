using System;

namespace DDW
{
	[Flags]
	public enum Modifier : uint 
	{
		Empty		= 0x00000000,
		New			= 0x00000001,
		Public		= 0x00000002,
		Protected	= 0x00000004,
		Internal	= 0x00000008,
		Private		= 0x00000010,
		Abstract	= 0x00000020,
		Sealed		= 0x00000040,

		Static		= 0x00000100,
		Virtual		= 0x00000200,
		Override	= 0x00000400,
		Extern		= 0x00000800,
		Readonly	= 0x00001000,
		Volatile	= 0x00002000,

		Ref			= 0x00008000,
		Out			= 0x00010000,
		Params		= 0x00020000,

		Assembly	= 0x00040000,
		Field		= 0x00080000,
		Event		= 0x00100000,
		Method		= 0x00200000,
		Param		= 0x00400000,
		Property	= 0x00800000,
		Return		= 0x01000000,
		Type		= 0x02000000,
		Module		= 0x04000000,
        Partial     = 0x08000000,
        Unsafe      = 0x10000000,
        Fixed       = 0x20000000,

        ClassMods = New | Public | Protected | Internal | Private | Abstract | Sealed | Static | Partial | Unsafe,
		ConstantMods	= New | Public | Protected | Internal | Private,
        FieldMods = New | Public | Protected | Internal | Private | Static | Readonly | Volatile | Unsafe | Fixed,
        FxedBufferdMods = New | Public | Protected | Internal | Private | Unsafe | Fixed,
        MethodMods = New | Public | Protected | Internal | Private | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe,
		ParamMods		= Ref | Out,
        PropertyMods = New | Public | Protected | Internal | Private | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe,
        EventMods = New | Public | Protected | Internal | Private | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe,
        IndexerMods = New | Public | Protected | Internal | Private | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe,
        OperatorMods = Public | Static | Extern | Unsafe,
        ConstructorMods = Public | Protected | Internal | Private | Extern | Unsafe,
        DestructorMods = Extern | Unsafe,
        StructMods = New | Public | Protected | Internal | Private | Partial | Unsafe,
        InterfaceMods = New | Public | Protected | Internal | Private | Partial | Unsafe,
		EnumMods		= New | Public | Protected | Internal | Private,
        DelegateMods = New | Public | Protected | Internal | Private | Unsafe,
		AttributeMods	= Field | Event | Method | Param | Property | Return | Type | Module,
        Accessibility      = Public | Protected | Internal | Private,
		GlobalAttributeMods	= Assembly,
	}

}
