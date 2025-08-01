// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace System.Net
{
    /// <summary>
    /// Represents an IP network with an <see cref="IPAddress"/> containing the network prefix and an <see cref="int"/> defining the prefix length.
    /// </summary>
    /// <remarks>
    /// This type disallows arbitrary IP-address/prefix-length CIDR pairs. <see cref="BaseAddress"/> must be defined so that all bits after the network prefix are set to zero.
    /// In other words, <see cref="BaseAddress"/> is always the first usable address of the network.
    /// The constructor and the parsing methods will throw in case there are non-zero bits after the prefix.
    /// </remarks>
    public readonly struct IPNetwork : IEquatable<IPNetwork>, ISpanFormattable, ISpanParsable<IPNetwork>, IUtf8SpanFormattable, IUtf8SpanParsable<IPNetwork>
    {
        private readonly IPAddress? _baseAddress;

        /// <summary>
        /// Gets the <see cref="IPAddress"/> that represents the prefix of the network.
        /// </summary>
        public IPAddress BaseAddress => _baseAddress ?? IPAddress.Any;

        /// <summary>
        /// Gets the length of the network prefix in bits.
        /// </summary>
        public int PrefixLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPNetwork"/> class with the specified <see cref="IPAddress"/> and prefix length.
        /// </summary>
        /// <param name="baseAddress">The <see cref="IPAddress"/> that represents the prefix of the network.</param>
        /// <param name="prefixLength">The length of the prefix in bits.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="baseAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="prefixLength"/> is smaller than `0` or longer than maximum length of <paramref name="prefixLength"/>'s <see cref="AddressFamily"/>.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="baseAddress"/> has non-zero bits after the network prefix.</exception>
        public IPNetwork(IPAddress baseAddress, int prefixLength)
        {
            ArgumentNullException.ThrowIfNull(baseAddress);

            if (prefixLength < 0 || prefixLength > GetMaxPrefixLength(baseAddress))
            {
                ThrowArgumentOutOfRangeException();
            }

            _baseAddress = ClearNonZeroBitsAfterNetworkPrefix(baseAddress, prefixLength);
            PrefixLength = prefixLength;

            [DoesNotReturn]
            static void ThrowArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException(nameof(prefixLength));
        }

        /// <summary>
        /// Determines whether a given <see cref="IPAddress"/> is part of the network.
        /// </summary>
        /// <param name="address">The <see cref="IPAddress"/> to check.</param>
        /// <returns><see langword="true"/> if the <see cref="IPAddress"/> is part of the network; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="address"/> is <see langword="null"/>.</exception>
        public bool Contains(IPAddress address)
        {
            ArgumentNullException.ThrowIfNull(address);

            if (address.AddressFamily != BaseAddress.AddressFamily && (BaseAddress.AddressFamily != AddressFamily.InterNetwork || !address.IsIPv4MappedToIPv6))
            {
                return false;
            }

            // This prevents the 'uint.MaxValue << 32' and the 'UInt128.MaxValue << 128' special cases in the code below.
            if (PrefixLength == 0)
            {
                return true;
            }

            if (address.AddressFamily == AddressFamily.InterNetwork || address.IsIPv4MappedToIPv6)
            {
                uint mask = uint.MaxValue << (32 - PrefixLength);
                if (BitConverter.IsLittleEndian)
                {
                    mask = BinaryPrimitives.ReverseEndianness(mask);
                }

                return BaseAddress.PrivateIPv4Address == (address.PrivateIPv4Address & mask);
            }
            else
            {
                UInt128 baseAddressValue = default;
                UInt128 otherAddressValue = default;

                BaseAddress.TryWriteBytes(MemoryMarshal.AsBytes(new Span<UInt128>(ref baseAddressValue)), out int bytesWritten);
                Debug.Assert(bytesWritten == IPAddressParserStatics.IPv6AddressBytes);
                address.TryWriteBytes(MemoryMarshal.AsBytes(new Span<UInt128>(ref otherAddressValue)), out bytesWritten);
                Debug.Assert(bytesWritten == IPAddressParserStatics.IPv6AddressBytes);

                UInt128 mask = UInt128.MaxValue << (128 - PrefixLength);
                if (BitConverter.IsLittleEndian)
                {
                    mask = BinaryPrimitives.ReverseEndianness(mask);
                }

                return baseAddressValue == (otherAddressValue & mask);
            }
        }

        /// <summary>
        /// Converts a CIDR <see cref="string"/> to an <see cref="IPNetwork"/> instance.
        /// </summary>
        /// <param name="s">A <see cref="string"/> that defines an IP network in CIDR notation.</param>
        /// <returns>An <see cref="IPNetwork"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified string is <see langword="null"/>.</exception>
        /// <exception cref="FormatException"><paramref name="s"/> is not a valid CIDR network string, or the address contains non-zero bits after the network prefix.</exception>
        public static IPNetwork Parse(string s)
        {
            ArgumentNullException.ThrowIfNull(s);
            return Parse(s.AsSpan());
        }

        /// <summary>
        /// Converts a CIDR character span to an <see cref="IPNetwork"/> instance.
        /// </summary>
        /// <param name="s">A character span that defines an IP network in CIDR notation.</param>
        /// <returns>An <see cref="IPNetwork"/> instance.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> is not a valid CIDR network string, or the address contains non-zero bits after the network prefix.</exception>
        public static IPNetwork Parse(ReadOnlySpan<char> s)
        {
            if (!TryParse(s, out IPNetwork result))
            {
                throw new FormatException(SR.net_bad_ip_network);
            }

            return result;
        }

        /// <summary>
        /// Converts a UTF-8 CIDR character span to an <see cref="IPNetwork"/> instance.
        /// </summary>
        /// <param name="utf8Text">A UTF-8 character span that defines an IP network in CIDR notation.</param>
        /// <returns>An <see cref="IPNetwork"/> instance.</returns>
        /// <exception cref="FormatException"><paramref name="utf8Text"/> is not a valid UTF-8 CIDR network string, or the address contains non-zero bits after the network prefix.</exception>
        public static IPNetwork Parse(ReadOnlySpan<byte> utf8Text)
        {
            if (!TryParse(utf8Text, out IPNetwork result))
            {
                throw new FormatException(SR.net_bad_ip_network);
            }

            return result;
        }

        /// <summary>
        /// Converts the specified CIDR string to an <see cref="IPNetwork"/> instance and returns a value indicating whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A <see cref="string"/> that defines an IP network in CIDR notation.</param>
        /// <param name="result">When the method returns, contains an <see cref="IPNetwork"/> instance if the conversion succeeds.</param>
        /// <returns><see langword="true"/> if the conversion was succesful; otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(string? s, out IPNetwork result)
        {
            if (s == null)
            {
                result = default;
                return false;
            }

            return TryParse(s.AsSpan(), out result);
        }

        /// <summary>
        /// Converts the specified CIDR character span to an <see cref="IPNetwork"/> instance and returns a value indicating whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A <see cref="string"/> that defines an IP network in CIDR notation.</param>
        /// <param name="result">When the method returns, contains an <see cref="IPNetwork"/> instance if the conversion succeeds.</param>
        /// <returns><see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(ReadOnlySpan<char> s, out IPNetwork result)
        {
            int separatorIndex = s.LastIndexOf('/');
            if (separatorIndex >= 0)
            {
                ReadOnlySpan<char> ipAddressSpan = s.Slice(0, separatorIndex);
                ReadOnlySpan<char> prefixLengthSpan = s.Slice(separatorIndex + 1);

                if (IPAddress.TryParse(ipAddressSpan, out IPAddress? address) &&
                    int.TryParse(prefixLengthSpan, NumberStyles.None, CultureInfo.InvariantCulture, out int prefixLength) &&
                    prefixLength <= GetMaxPrefixLength(address))
                {
                    Debug.Assert(prefixLength >= 0); // Parsing with NumberStyles.None should ensure that prefixLength is always non-negative.
                    result = new IPNetwork(address, prefixLength);
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Converts the specified UTF-8 CIDR character span to an <see cref="IPNetwork"/> instance and returns a value indicating whether the conversion succeeded.
        /// </summary>
        /// <param name="utf8Text">A UTF-8 character span that defines an IP network in CIDR notation.</param>
        /// <param name="result">When the method returns, contains an <see cref="IPNetwork"/> instance if the conversion succeeds.</param>
        /// <returns><see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(ReadOnlySpan<byte> utf8Text, out IPNetwork result)
        {
            int separatorIndex = utf8Text.LastIndexOf((byte)'/');
            if (separatorIndex >= 0)
            {
                ReadOnlySpan<byte> ipAddressSpan = utf8Text.Slice(0, separatorIndex);
                ReadOnlySpan<byte> prefixLengthSpan = utf8Text.Slice(separatorIndex + 1);

                if (IPAddress.TryParse(ipAddressSpan, out IPAddress? address) &&
                    int.TryParse(prefixLengthSpan, NumberStyles.None, CultureInfo.InvariantCulture, out int prefixLength) &&
                    prefixLength <= GetMaxPrefixLength(address))
                {
                    Debug.Assert(prefixLength >= 0); // Parsing with NumberStyles.None should ensure that prefixLength is always non-negative.
                    result = new IPNetwork(address, prefixLength);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static int GetMaxPrefixLength(IPAddress baseAddress) => baseAddress.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

        private static IPAddress ClearNonZeroBitsAfterNetworkPrefix(IPAddress baseAddress, int prefixLength)
        {
            if (baseAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                // Bitwise shift works only for lower 5-bits count operands.
                if (prefixLength == 0)
                {
                    // Corresponds to 0.0.0.0
                    return IPAddress.Any;
                }

                uint mask = uint.MaxValue << (32 - prefixLength);
                if (BitConverter.IsLittleEndian)
                {
                    mask = BinaryPrimitives.ReverseEndianness(mask);
                }

                uint newAddress = baseAddress.PrivateAddress & mask;
                return newAddress == baseAddress.PrivateAddress
                    ? baseAddress
                    : new IPAddress(newAddress);
            }
            else
            {
                // Bitwise shift works only for lower 7-bits count operands.
                if (prefixLength == 0)
                {
                    // Corresponds to [::]
                    return IPAddress.IPv6Any;
                }

                UInt128 value = default;
                baseAddress.TryWriteBytes(MemoryMarshal.AsBytes(new Span<UInt128>(ref value)), out int bytesWritten);
                Debug.Assert(bytesWritten == IPAddressParserStatics.IPv6AddressBytes);

                UInt128 mask = UInt128.MaxValue << (128 - prefixLength);
                if (BitConverter.IsLittleEndian)
                {
                    mask = BinaryPrimitives.ReverseEndianness(mask);
                }

                UInt128 newAddress = value & mask;
                return newAddress == value
                    ? baseAddress
                    : new IPAddress(MemoryMarshal.AsBytes(new Span<UInt128>(ref newAddress)));
            }
        }

        /// <summary>
        /// Converts the instance to a string containing the <see cref="IPNetwork"/>'s CIDR notation.
        /// </summary>
        /// <returns>The <see cref="string"/> containing the <see cref="IPNetwork"/>'s CIDR notation.</returns>
        public override string ToString() =>
            string.Create(CultureInfo.InvariantCulture, stackalloc char[128], $"{BaseAddress}/{(uint)PrefixLength}");

        /// <summary>
        /// Attempts to write the <see cref="IPNetwork"/>'s CIDR notation to the given <paramref name="destination"/> span and returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <param name="destination">The destination span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written to <paramref name="destination"/>.</param>
        /// <returns><see langword="true"/> if the formatting was succesful; otherwise <see langword="false"/>.</returns>
        public bool TryFormat(Span<char> destination, out int charsWritten) =>
            destination.TryWrite(CultureInfo.InvariantCulture, $"{BaseAddress}/{(uint)PrefixLength}", out charsWritten);

        /// <summary>
        /// Attempts to write the <see cref="IPNetwork"/>'s CIDR notation to the given <paramref name="utf8Destination"/> UTF-8 span and returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <param name="utf8Destination">The destination span of UTF-8 bytes.</param>
        /// <param name="bytesWritten">When this method returns, contains the number of bytes that were written to <paramref name="utf8Destination"/>.</param>
        /// <returns><see langword="true"/> if the formatting was succesful; otherwise <see langword="false"/>.</returns>
        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten) =>
            Utf8.TryWrite(utf8Destination, CultureInfo.InvariantCulture, $"{BaseAddress}/{(uint)PrefixLength}", out bytesWritten);

        /// <summary>
        /// Determines whether two <see cref="IPNetwork"/> instances are equal.
        /// </summary>
        /// <param name="other">The <see cref="IPNetwork"/> instance to compare to this instance.</param>
        /// <returns><see langword="true"/> if the networks are equal; otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Uninitialized <see cref="IPNetwork"/> instance.</exception>
        public bool Equals(IPNetwork other) =>
            PrefixLength == other.PrefixLength &&
            BaseAddress.Equals(other.BaseAddress);

        /// <summary>
        /// Determines whether two <see cref="IPNetwork"/> instances are equal.
        /// </summary>
        /// <param name="obj">The <see cref="IPNetwork"/> instance to compare to this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="IPNetwork"/> instance and the networks are equal; otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Uninitialized <see cref="IPNetwork"/> instance.</exception>
        public override bool Equals([NotNullWhen(true)] object? obj) =>
            obj is IPNetwork other &&
            Equals(other);

        /// <summary>
        /// Determines whether the specified instances of <see cref="IPNetwork"/> are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns><see langword="true"/> if the networks are equal; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(IPNetwork left, IPNetwork right) => left.Equals(right);

        /// <summary>
        /// Determines whether the specified instances of <see cref="IPNetwork"/> are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns><see langword="true"/> if the networks are not equal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(IPNetwork left, IPNetwork right) => !(left == right);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An integer hash value.</returns>
        public override int GetHashCode() => HashCode.Combine(BaseAddress, PrefixLength);

        /// <inheritdoc />
        string IFormattable.ToString(string? format, IFormatProvider? provider) => ToString();

        /// <inheritdoc />
        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
            // format and provider are ignored
            TryFormat(destination, out charsWritten);

        /// <inheritdoc />
        bool IUtf8SpanFormattable.TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
            // format and provider are ignored
            TryFormat(utf8Destination, out bytesWritten);

        /// <inheritdoc />
        static IPNetwork IParsable<IPNetwork>.Parse([NotNull] string s, IFormatProvider? provider) => Parse(s);

        /// <inheritdoc />
        static bool IParsable<IPNetwork>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out IPNetwork result) => TryParse(s, out result);

        /// <inheritdoc />
        static IPNetwork ISpanParsable<IPNetwork>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s);

        /// <inheritdoc />
        static IPNetwork IUtf8SpanParsable<IPNetwork>.Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider) => Parse(utf8Text);

        /// <inheritdoc />
        static bool ISpanParsable<IPNetwork>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out IPNetwork result) => TryParse(s, out result);

        /// <inheritdoc />
        static bool IUtf8SpanParsable<IPNetwork>.TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out IPNetwork result) => TryParse(utf8Text, out result);
    }
}
