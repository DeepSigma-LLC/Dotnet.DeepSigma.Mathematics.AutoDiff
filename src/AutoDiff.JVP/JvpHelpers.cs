namespace DeepSigma.Mathematics.AutoDiff.JVP;

internal static class JvpHelpers
{
    internal static void ValidateLengths(string param1, int len1, string param2, int len2)
    {
        if (len1 != len2)
            throw new ArgumentException($"'{param1}' (length {len1}) and '{param2}' (length {len2}) must have the same length.");
    }
}
