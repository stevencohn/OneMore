//************************************************************************************************
// Copyright © 2032 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Numerics;
	using System.Security.Cryptography;
	using System.Text;


	internal static class HashAlgorithmExtensions
	{
		public static string GetHashString(this HashAlgorithm algorithm, string s)
		{
			var data = Encoding.UTF8.GetBytes(s);
			var hash = algorithm.ComputeHash(data);
			return new BigInteger(hash).ToString();
		}
	}
}
