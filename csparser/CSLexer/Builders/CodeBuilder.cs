using System;
using System.Text;

namespace DDW.Builders
{
  /// <summary>
  /// Builds code in a specific language. This abstract class is the root. Wraps around the
  /// <c>builder</c> and provides useful features besides. Intended for subclassing.
  /// </summary>
  /// <remarks>
  /// Parts of this class were auto-generated automatically from <c>builder</c>.
  /// </remarks>
  public abstract class CodeBuilder
  {
    protected readonly StringBuilder builder;
    protected int indentLevel;
    protected string indentString = string.Empty;

    protected string IndentString
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        int n = indentLevel;
        while (n --> 0)
          sb.Append(indentString);
        return sb.ToString();
      }
    }

    protected CodeBuilder(string indentString)
    {
      builder = new StringBuilder();
      this.indentString = indentString;
    }

    public CodeBuilder Indent()
    {
      ++indentLevel;
      return this;
    }

    public CodeBuilder Unindent()
    {
      --indentLevel;
      return this;
    }

    public int Capacity
    {
      get
      {
        return builder.Capacity;
      }
      set
      {
        builder.Capacity = value;
      }
    }

    public int Length
    {
      get
      {
        return builder.Length;
      }
      set
      {
        builder.Length = value;
      }
    }

    public int MaxCapacity
    {
      get
      {
        return builder.MaxCapacity;
      }
    }

    public char this[int index]
    {
      get
      {
        return builder[index];
      }
      set
      {
        builder[index] = value;
      }
    }

    public CodeBuilder Append(bool value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(byte value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(char value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(decimal value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(double value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(char[] value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(short value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(int value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(long value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(object value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(sbyte value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(float value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(string value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(ushort value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(uint value)
    {
      return AppendImpl(value);
    }

    public CodeBuilder Append(ulong value)
    {
      return AppendImpl(value);
    }

    protected CodeBuilder AppendIndent()
    {
      int level = indentLevel;
      while (level-- > 0)
        builder.Append(indentString);
      return this;
    }

    protected CodeBuilder AppendImpl(object value)
    {
      AppendIndent();
      builder.Append(value);
      return this;
    }

    public CodeBuilder Append(char value, int repeatCount)
    {
      return Append(value, repeatCount);
    }

    public CodeBuilder Append(string value, int startIndex, int count)
    {
      return Append(value, startIndex, count);
    }

    public CodeBuilder Append(char[] value, int startIndex, int charCount)
    {
      return Append(value, startIndex, charCount);
    }

    public CodeBuilder AppendFormat(string format, object arg0)
    {
      return AppendFormat(format, arg0);
    }

    public CodeBuilder AppendFormat(string format, object[] args)
    {
      return AppendFormat(format, args);
    }

    public CodeBuilder AppendFormat(IFormatProvider provider, string format, object[] args)
    {
      return AppendFormat(provider, format, args);
    }

    public CodeBuilder AppendFormat(string format, object arg0, object arg1)
    {
      return AppendFormat(format, arg0, arg1);
    }

    public CodeBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
    {
      return AppendFormat(format, arg0, arg1, arg2);
    }

    public CodeBuilder AppendLineImpl()
    {
      AppendIndent();
      builder.AppendLine();
      return this;
    }

    public CodeBuilder AppendLine()
    {
      return AppendLineImpl();
    }

    public CodeBuilder AppendLine(string value)
    {
      Append(value);
      return AppendLineImpl();
    }

    public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
      builder.CopyTo(sourceIndex, destination, destinationIndex, count);
    }

    public int EnsureCapacity(int capacity)
    {
      return builder.EnsureCapacity(capacity);
    }

    public bool Equals(CodeBuilder sb)
    {
      return builder.Equals(sb);
    }

    public CodeBuilder Insert(int index, char[] value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, bool value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, byte value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, char value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, decimal value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, double value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, short value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, int value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, long value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, object value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, sbyte value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, float value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, string value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, ushort value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, uint value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, ulong value)
    {
      builder.Insert(index, value);
      return this;
    }

    public CodeBuilder Insert(int index, string value, int count)
    {
      builder.Insert(index, value, count);
      return this;
    }

    public CodeBuilder Insert(int index, char[] value, int startIndex, int charCount)
    {
      builder.Insert(index, value, startIndex, charCount);
      return this;
    }

    public CodeBuilder Remove(int startIndex, int length)
    {
      builder.Remove(startIndex, length);
      return this;
    }

    public CodeBuilder Replace(char oldChar, char newChar)
    {
      builder.Replace(oldChar, newChar);
      return this;
    }

    public CodeBuilder Replace(string oldValue, string newValue)
    {
      builder.Replace(oldValue, newValue);
      return this;
    }

    public CodeBuilder Replace(char oldChar, char newChar, int startIndex, int count)
    {
      builder.Replace(oldChar, newChar, startIndex, count);
      return this;
    }

    public CodeBuilder Replace(string oldValue, string newValue, int startIndex, int count)
    {
      builder.Replace(oldValue, newValue, startIndex, count);
      return this;
    }

    public override string ToString()
    {
      return builder.ToString();
    }

    public string ToString(int startIndex, int length)
    {
      return builder.ToString(startIndex, length);
    }
  }
}