﻿using System.Buffers;
using System.Buffers.Text;

namespace Passwordless.Net;

internal static class Base64Url
{
    /// <summary>
    /// Converts arg data to a Base64Url encoded string.
    /// </summary>
    public static string Encode(ReadOnlySpan<byte> arg)
    {
        int minimumLength = (int)(((long)arg.Length + 2L) / 3 * 4);
        char[] array = ArrayPool<char>.Shared.Rent(minimumLength);

#if NET5_0_OR_GREATER
        Convert.TryToBase64Chars(arg, array, out var charsWritten);
#elif NET462 || NETSTANDARD2_0
        var charsWritten = Convert.ToBase64CharArray(arg.ToArray(), 0, minimumLength, array, 0);
#endif
        Span<char> span = array.AsSpan(0, charsWritten);


        for (int i = 0; i < span.Length; i++)
        {
            ref char reference = ref span[i];
            switch (reference)
            {
                case '+':
                    reference = '-';
                    break;
                case '/':
                    reference = '_';
                    break;
            }
        }
        int num = span.IndexOf('=');
        if (num > -1)
        {
            span = span.Slice(0, num);
        }

#if NET5_0_OR_GREATER
        string result = new string(span);
#elif NET462 || NETSTANDARD2_0
        string result = new string(span.ToArray());
#endif
        ArrayPool<char>.Shared.Return(array, clearArray: true);
        return result;
    }

    /// <summary>
    /// Decodes a Base64Url encoded string to its raw bytes.
    /// </summary>
    public static byte[] Decode(ReadOnlySpan<char> text)
    {
        int num = (text.Length % 4) switch
        {
            2 => 2,
            3 => 1,
            _ => 0,
        };
        int num2 = text.Length + num;
        char[] array = ArrayPool<char>.Shared.Rent(num2);
        text.CopyTo(array);
        for (int i = 0; i < text.Length; i++)
        {
            ref char reference = ref array[i];
            switch (reference)
            {
                case '-':
                    reference = '+';
                    break;
                case '_':
                    reference = '/';
                    break;
            }
        }
        switch (num)
        {
            case 1:
                array[num2 - 1] = '=';
                break;
            case 2:
                array[num2 - 1] = '=';
                array[num2 - 2] = '=';
                break;
        }
        byte[] result = Convert.FromBase64CharArray(array, 0, num2);
        ArrayPool<char>.Shared.Return(array, clearArray: true);
        return result;
    }

    /// <summary>
    /// Decodes a Base64Url encoded string to its raw bytes.
    /// </summary>
    public static byte[] DecodeUtf8(ReadOnlySpan<byte> text)
    {
        int num = (text.Length % 4) switch
        {
            2 => 2,
            3 => 1,
            _ => 0,
        };
        int num2 = text.Length + num;
        byte[] array = ArrayPool<byte>.Shared.Rent(num2);
        text.CopyTo(array);
        for (int i = 0; i < text.Length; i++)
        {
            ref byte reference = ref array[i];
            switch (reference)
            {
                case 45:
                    reference = 43;
                    break;
                case 95:
                    reference = 47;
                    break;
            }
        }
        switch (num)
        {
            case 1:
                array[num2 - 1] = 61;
                break;
            case 2:
                array[num2 - 1] = 61;
                array[num2 - 2] = 61;
                break;
        }
        Base64.DecodeFromUtf8InPlace(array.AsSpan(0, num2), out var bytesWritten);
        byte[] result = array.AsSpan(0, bytesWritten).ToArray();
        ArrayPool<byte>.Shared.Return(array, clearArray: true);
        return result;
    }
}