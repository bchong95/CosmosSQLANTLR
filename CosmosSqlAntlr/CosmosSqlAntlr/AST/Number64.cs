//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace CosmosSqlAntlr.Ast
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Struct that represents either a double or 64 bit int
    /// </summary>
    public readonly struct Number64 : IComparable<Number64>, IEquatable<Number64>
    {
        /// <summary>
        /// Size of the Number64 when compared as a DoubleEx
        /// </summary>
        public const int SizeOf = sizeof(double) + sizeof(ushort);

        /// <summary>
        /// Maximum Number64.
        /// </summary>
        public static readonly Number64 MaxValue = new Number64(double.MaxValue);

        /// <summary>
        /// Maximum Number64.
        /// </summary>
        public static readonly Number64 MinValue = new Number64(double.MinValue);

        /// <summary>
        /// The double if the value is a double.
        /// </summary>
        private readonly double? doubleValue;

        /// <summary>
        /// The long if the value is a long.
        /// </summary>
        private readonly long? longValue;

        private Number64(double value)
        {
            this.doubleValue = value;
            this.longValue = null;
        }

        private Number64(long value)
        {
            this.longValue = value;
            this.doubleValue = null;
        }

        public bool IsInteger
        {
            get
            {
                return this.longValue.HasValue;
            }
        }

        public bool IsDouble
        {
            get
            {
                return this.doubleValue.HasValue;
            }
        }

        public bool IsInfinity
        {
            get
            {
                return !this.IsInteger && double.IsInfinity(this.doubleValue.Value);
            }
        }

        public bool IsNaN
        {
            get
            {
                return !this.IsInteger && double.IsNaN(this.doubleValue.Value);
            }
        }

        public override string ToString()
        {
            return this.ToString(format: null, formatProvider: CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return this.ToString(format: format, formatProvider: CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return this.ToString(format: null, formatProvider: formatProvider);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            string toString;
            if (this.IsDouble)
            {
                toString = Number64.ToDouble(this).ToString(format, formatProvider);
            }
            else
            {
                toString = Number64.ToLong(this).ToString(format, formatProvider);
            }

            return toString;
        }

        public void CopyTo(Span<byte> buffer)
        {
            if (buffer.Length < Number64.SizeOf)
            {
                throw new ArgumentException($"{nameof(buffer)} is too short.");
            }

            DoubleEx doubleEx = Number64.ToDoubleEx(this);
            Span<double> doubleBuffer = MemoryMarshal.Cast<byte, double>(buffer);
            doubleBuffer[0] = doubleEx.DoubleValue;
            Span<ushort> ushortBuffer = MemoryMarshal.Cast<byte, ushort>(buffer.Slice(sizeof(double)));
            ushortBuffer[0] = doubleEx.ExtraBits;
        }

        #region Static Operators
        /// <summary>
        /// Returns if one Number64 is less than another Number64.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether left is less than right.</returns>
        public static bool operator <(Number64 left, Number64 right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Returns if one Number64 is greater than another Number64.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether left is greater than right.</returns>
        public static bool operator >(Number64 left, Number64 right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Returns if one Number64 is less than or equal to another Number64.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether left is less than or equal to the right.</returns>
        public static bool operator <=(Number64 left, Number64 right)
        {
            return !(right < left);
        }

        /// <summary>
        /// Returns if one Number64 is greater than or equal to another Number64.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether left is greater than or equal to the right.</returns>
        public static bool operator >=(Number64 left, Number64 right)
        {
            return !(left < right);
        }

        /// <summary>
        /// Returns if two Number64 are equal.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether the left is equal to the right.</returns>
        public static bool operator ==(Number64 left, Number64 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns if two Number64 are not equal.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>Whether the left is not equal to the right.</returns>
        public static bool operator !=(Number64 left, Number64 right)
        {
            return !(left == right);
        }
        #endregion

        #region Static Implicit Operators
        /// <summary>
        /// Implicitly converts a long to Number64.
        /// </summary>
        /// <param name="value">The long to convert.</param>
        public static implicit operator Number64(long value)
        {
            return new Number64(value);
        }

        /// <summary>
        /// Implicitly converts a double to Number64.
        /// </summary>
        /// <param name="value">The double to convert.</param>
        public static implicit operator Number64(double value)
        {
            return new Number64(value);
        }
        #endregion

        public static long ToLong(Number64 number64)
        {
            long value;
            if (number64.IsInteger)
            {
                value = number64.longValue.Value;
            }
            else
            {
                value = (long)number64.doubleValue.Value;
            }

            return value;
        }

        public static double ToDouble(Number64 number64)
        {
            double value;
            if (number64.IsDouble)
            {
                value = number64.doubleValue.Value;
            }
            else
            {
                value = (double)number64.longValue.Value;
            }

            return value;
        }

        private static DoubleEx ToDoubleEx(Number64 number64)
        {
            DoubleEx doubleEx;
            if (number64.IsDouble)
            {
                doubleEx = number64.doubleValue.Value;
            }
            else
            {
                doubleEx = number64.longValue.Value;
            }

            return doubleEx;
        }

        /// <summary>
        /// Compares this value to an object.
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The comparison.</returns>
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }

            if (value is Number64)
            {
                return this.CompareTo((Number64)value);
            }

            throw new ArgumentException("Value must be a Number64.");
        }

        /// <summary>
        /// Compares this Number64 to another instance of the Number64 type.
        /// </summary>
        /// <param name="other">The other instance to compare to.</param>
        /// <returns>
        /// A negative number if this instance is less than the other instance.
        /// Zero if they are the same.
        /// A positive number if this instance is greater than the other instance.
        /// </returns>
        public int CompareTo(Number64 other)
        {
            int comparison;
            if (this.IsInteger && other.IsInteger)
            {
                comparison = this.longValue.Value.CompareTo(other.longValue.Value);
            }
            else if (this.IsDouble && other.IsDouble)
            {
                comparison = this.doubleValue.Value.CompareTo(other.doubleValue.Value);
            }
            else
            {
                // Convert both to doubleEx and compare
                DoubleEx first = this.IsDouble ? this.doubleValue.Value : this.longValue.Value;
                DoubleEx second = other.IsDouble ? other.doubleValue.Value : other.longValue.Value;
                comparison = first.CompareTo(second);
            }

            return comparison;
        }

        /// <summary>
        /// Returns whether this instance equals another object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>Whether this instance equals another object.</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is Number64)
            {
                return this.Equals((Number64)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns whether this Number64 equals another Number64.
        /// </summary>
        /// <param name="other">The Number64 to compare to.</param>
        /// <returns>Whether this Number64 equals another Number64.</returns>
        public bool Equals(Number64 other)
        {
            return this.CompareTo(other) == 0;
        }

        /// <summary>
        /// Gets a hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            DoubleEx doubleEx = Number64.ToDoubleEx(this);
            return doubleEx.GetHashCode();
        }

        #region DoubleEx
        /// <summary>
        /// Represents an extended double number with 62-bit mantissa which is capable of representing a 64-bit integer with no precision loss
        /// </summary>
        private readonly struct DoubleEx : IEquatable<DoubleEx>, IComparable<DoubleEx>
        {
            private DoubleEx(double doubleValue, ushort extraBits)
            {
                this.DoubleValue = doubleValue;
                this.ExtraBits = extraBits;
            }

            /// <summary>
            /// The double if the value is a double.
            /// </summary>
            public double DoubleValue
            {
                get;
            }

            /// <summary>
            /// The long if the value is a long.
            /// </summary>
            public ushort ExtraBits
            {
                get;
            }

            #region Static Operators
            /// <summary>
            /// Returns if two DoubleEx are equal.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether the left is equal to the right.</returns>
            public static bool operator ==(DoubleEx left, DoubleEx right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Returns if two DoubleEx are not equal.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether the left is not equal to the right.</returns>
            public static bool operator !=(DoubleEx left, DoubleEx right)
            {
                return !(left == right);
            }

            /// <summary>
            /// Returns if one DoubleEx is less than another DoubleEx.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether left is less than right.</returns>
            public static bool operator <(DoubleEx left, DoubleEx right)
            {
                return left.CompareTo(right) < 0;
            }

            /// <summary>
            /// Returns if one DoubleEx is greater than another DoubleEx.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether left is greater than right.</returns>
            public static bool operator >(DoubleEx left, DoubleEx right)
            {
                return left.CompareTo(right) > 0;
            }

            /// <summary>
            /// Returns if one DoubleEx is less than or equal to another DoubleEx.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether left is less than or equal to the right.</returns>
            public static bool operator <=(DoubleEx left, DoubleEx right)
            {
                return !(right < left);
            }

            /// <summary>
            /// Returns if one Number64 is greater than or equal to another Number64.
            /// </summary>
            /// <param name="left">The left hand side of the operator.</param>
            /// <param name="right">The right hand side of the operator.</param>
            /// <returns>Whether left is greater than or equal to the right.</returns>
            public static bool operator >=(DoubleEx left, DoubleEx right)
            {
                return !(left < right);
            }
            #endregion

            #region Static Implicit Operators
            /// <summary>
            /// Implicitly converts a long to DoubleEx.
            /// </summary>
            /// <param name="value">The int to convert.</param>
            public static implicit operator DoubleEx(long value)
            {
                if (value == long.MinValue)
                {
                    // Special casing this since you can't take Abs(long.MinValue) due to two's complement
                    return new DoubleEx(value, 0);
                }

                double doubleValue;
                ushort extraBits;

                long absValue = Math.Abs(value);
                int msbIndex = BitUtils.GetMostSignificantBitIndex((ulong)absValue);

                // Check if the integer value spans more than 52 bits (meaning it won't fit in a double's mantissa at full precision)
                if ((msbIndex > 52) && ((msbIndex - BitUtils.GetLeastSignificantBitIndex((long)absValue)) > 52))
                {
                    // Retrieve the most significant bit index which is the double exponent value
                    int exponentValue = msbIndex;

                    long exponentBits = ((long)exponentValue + 1023) << 52;

                    // Set the mantissa as a 62 bit value (i.e. represents 63-bit number)
                    long mantissa = (absValue << (62 - exponentValue)) & 0x3FFFFFFFFFFFFFFF;

                    // Retrieve the least significant 10 bits
                    extraBits = (ushort)((mantissa & 0x3FF) << 6);

                    // Adjust the mantissa to 52 bits
                    mantissa = mantissa >> 10;

                    long valueBits = exponentBits | mantissa;
                    if (value != absValue)
                    {
                        valueBits = (long)((ulong)valueBits | 0x8000000000000000);
                    }

                    doubleValue = BitConverter.Int64BitsToDouble(valueBits);
                }
                else
                {
                    doubleValue = value;
                    extraBits = 0;
                }

                return new DoubleEx(doubleValue, extraBits);
            }

            /// <summary>
            /// Implicitly converts a DoubleEx to long.
            /// </summary>
            /// <param name="value">The int to convert.</param>
            public static implicit operator long(DoubleEx value)
            {
                long integerValue;

                if (value.ExtraBits != 0)
                {
                    integerValue = BitConverter.DoubleToInt64Bits(value.DoubleValue);

                    // Retrieve and clear the sign bit
                    bool isNegative = BitUtils.BitTestAndReset64(integerValue, 63, out integerValue);

                    // Retrieve the exponent value
                    int exponentValue = (int)((integerValue >> 52) - 1023L);

                    // Extend the value to 62 bits
                    integerValue = integerValue << 10;

                    // Set MSB (i.e. bit 62) and clear the sign bit (left over from the exponent)
                    integerValue = (integerValue | 0x4000000000000000) & 0x7FFFFFFFFFFFFFFF;

                    // Set the extra bits
                    integerValue = integerValue | (((long)value.ExtraBits) >> 6);

                    // Adjust for the exponent
                    integerValue = integerValue >> (62 - exponentValue);
                    if (isNegative)
                    {
                        integerValue = -integerValue;
                    }
                }
                else
                {
                    integerValue = (long)value.DoubleValue;
                }

                return integerValue;
            }

            /// <summary>
            /// Implicitly converts a double to DoubleEx.
            /// </summary>
            /// <param name="value">The int to convert.</param>
            public static implicit operator DoubleEx(double value)
            {
                return new DoubleEx(value, 0);
            }
            #endregion

            /// <summary>
            /// Returns whether this instance equals another object.
            /// </summary>
            /// <param name="obj">The object to compare to.</param>
            /// <returns>Whether this instance equals another object.</returns>
            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj is DoubleEx doubleEx)
                {
                    return this.Equals(doubleEx);
                }

                return false;
            }

            /// <summary>
            /// Returns whether this DoubleEx equals another DoubleEx.
            /// </summary>
            /// <param name="other">The DoubleEx to compare to.</param>
            /// <returns>Whether this DoubleEx equals another DoubleEx.</returns>
            public bool Equals(DoubleEx other)
            {
                return (this.DoubleValue == other.DoubleValue) && (this.ExtraBits == other.ExtraBits);
            }

            /// <summary>
            /// Gets a hash code for this instance.
            /// </summary>
            /// <returns>The hash code for this instance.</returns>
            public override int GetHashCode()
            {
                int hashCode = 0;
                // Need to use bit converter here, since there was a bug in get hash code for NaN
                // https://github.com/dotnet/coreclr/issues/6237
                hashCode ^= BitConverter.DoubleToInt64Bits(this.DoubleValue).GetHashCode();
                hashCode ^= this.ExtraBits.GetHashCode();
                return hashCode;
            }

            public int CompareTo(DoubleEx other)
            {
                int compare = this.DoubleValue.CompareTo(other.DoubleValue);
                if (compare == 0)
                {
                    compare = this.ExtraBits.CompareTo(other.ExtraBits) * Math.Sign(this.DoubleValue);
                }

                return compare;
            }
        }
        #endregion

        private static class BitUtils
        {
            /// <summary>
            /// https://stackoverflow.com/questions/11376288/fast-computing-of-log2-for-64-bit-integers
            /// </summary>
            private static readonly int[] tab64 = new int[]
            {
            63,  0, 58,  1, 59, 47, 53,  2,
            60, 39, 48, 27, 54, 33, 42,  3,
            61, 51, 37, 40, 49, 18, 28, 20,
            55, 30, 34, 11, 43, 14, 22,  4,
            62, 57, 46, 52, 38, 26, 32, 41,
            50, 36, 17, 19, 29, 10, 13, 21,
            56, 45, 25, 31, 35, 16,  9, 12,
            44, 24, 15,  8, 23,  7,  6,  5
            };

            public static long GetMostSignificantBit(long x)
            {
                // http://aggregate.org/MAGIC/#Most%20Significant%201%20Bit
                // Given a binary integer value x, the most significant 1 bit (highest numbered element of a bit set)
                // can be computed using a SWAR algorithm that recursively "folds" the upper bits into the lower bits.
                // This process yields a bit vector with the same most significant 1 as x, but all 1's below it.
                // Bitwise AND of the original value with the complement of the "folded" value shifted down by one yields the most significant bit.
                // For a 64-bit value:

                x |= (x >> 1);
                x |= (x >> 2);
                x |= (x >> 4);
                x |= (x >> 8);
                x |= (x >> 16);
                x |= (x >> 32);
                return (x & ~(x >> 1));
            }

            public static int FloorLog2(ulong value)
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                value |= value >> 32;
                return tab64[((ulong)((value - (value >> 1)) * 0x07EDD5E59A4E28C2)) >> 58];
            }

            public static bool IsPowerOf2(ulong x)
            {
                return (x & (x - 1)) == 0;
            }

            public static int GetMostSignificantBitIndex(ulong x)
            {
                return BitUtils.FloorLog2(x);
            }

            public static long GetLeastSignificantBit(long x)
            {
                // http://aggregate.org/MAGIC/#Least%20Significant%201%20Bit
                // Given a 2's complement binary integer value x, (x&-x) is the least significant 1 bit.
                // (This was pointed-out by Tom May.)
                // The reason this works is that it is equivalent to (x & ((~x) + 1));
                // any trailing zero bits in x become ones in ~x, 
                // adding 1 to that carries into the following bit,
                // and AND with x yields only the flipped bit... the original position of the least significant 1 bit.

                return (int)(x & -x);
            }

            public static int GetLeastSignificantBitIndex(long x)
            {
                return BitUtils.FloorLog2((ulong)BitUtils.GetLeastSignificantBit(x));
            }

            //examines bit b of the address a, returns its current value, and resets the bit to 0.
            public static bool BitTestAndReset64(long input, int index, out long output)
            {
                // https://stackoverflow.com/questions/2431732/checking-if-a-bit-is-set-or-not
                bool set = (input & (1L << index)) != 0;

                // https://www.dotnetperls.com/set-bit-zero
                output = (input &= ~(1L << index));

                return set;
            }
        }
    }
}