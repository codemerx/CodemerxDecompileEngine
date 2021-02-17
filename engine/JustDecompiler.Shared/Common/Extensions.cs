using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.AssemblyResolver;

namespace Telerik.JustDecompiler.Common
{
    public static class Extensions
    {
        public static bool IsValidIdentifier(this string self)
        {
            /// The pattern is taken from the ECMA-334 standart, 9.4.2 Identifiers (page 92).
            /// Although the regex covers the C# identifiers, the rules for VB are the same.
            /// No care is taken for escape sequences in our case.
            if (self == null || self == string.Empty)
            {
                return false;
            }

			string normalizedString;
			try
			{
				normalizedString = self.Normalize();
			}
			catch (ArgumentException)
			{
				// String contains invalid code points created by obfuscators
				return false;
			}

            if (!normalizedString[0].IsValidIdentifierFirstCharacter())
            {
                return false;
            }

            for (int i = 1; i < normalizedString.Length; i++)
            {
                if (!normalizedString[i].IsValidIdentifierCharacter())
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidIdentifierFirstCharacter(this char firstCharacter)
        {
            /// The pattern is taken from the ECMA-334 standart, 9.4.2 Identifiers (page 92).
            /// Although the check covers the C# identifiers, the rules for VB are the same.
            if (firstCharacter == '_')
            {
                return true;
            }

            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(firstCharacter);

            if (unicodeCategory == UnicodeCategory.LowercaseLetter || // class Ll
                unicodeCategory == UnicodeCategory.UppercaseLetter || // class Lu
                unicodeCategory == UnicodeCategory.TitlecaseLetter || // class Lt
                unicodeCategory == UnicodeCategory.ModifierLetter || // class Lm
                unicodeCategory == UnicodeCategory.OtherLetter || // class Lo
                unicodeCategory == UnicodeCategory.LetterNumber)	  // class Nl
            {
                return true;
            }

            return false;
        }

        public static bool IsValidIdentifierCharacter(this char currentChar)
        {
            /// The pattern is taken from the ECMA-334 standart, 9.4.2 Identifiers (page 92).
            /// Although the check covers the C# identifiers, the rules for VB are the same.
            if (currentChar.IsValidIdentifierFirstCharacter())
            {
                return true;
            }

            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(currentChar);

            if (unicodeCategory == UnicodeCategory.NonSpacingMark || // class Mn
                unicodeCategory == UnicodeCategory.SpacingCombiningMark || // class Mc
                unicodeCategory == UnicodeCategory.DecimalDigitNumber || // class Nd
                unicodeCategory == UnicodeCategory.ConnectorPunctuation || // class Pc
                unicodeCategory == UnicodeCategory.Format)                  // class Cf
            {
                return true;
            }
            return false;
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> source)
        {
            foreach (KeyValuePair<TKey, TValue> pair in source)
            {
                self.Add(pair);
            }
        }

        /// <summary>
        /// Resolves the given TypeReference to the first overloaded equality (inequality) operator or to Object if there is no such.
        /// </summary>
        /// <param name="type">The TypeReference to be resolved.</param>
        /// <param name="lastResolvedType">Out parameter which is filled with the last resolved type.</param>
        /// <returns>True if there is overloaded operator. False if there is no overloaded operator in the whole inheritance chain. Null if there is not resolved reference.</returns>
        public static bool? ResolveToOverloadedEqualityOperator(TypeReference type, out TypeReference lastResolvedType)
        {
            if (type.IsValueType || type.IsFunctionPointer || type.IsPrimitive || type.IsGenericParameter)
            {
                throw new NotSupportedException();
            }

            lastResolvedType = type;
            TypeDefinition currentTypeDefinition = type.Resolve();
            if (currentTypeDefinition == null)
            {
                return null;
            }

            if (currentTypeDefinition.IsInterface)
            {
                return false;
            }

            while (currentTypeDefinition.Name != "Object")
            {
                if (currentTypeDefinition.Methods.Any(m => m.Name == "op_Equality" || m.Name == "op_Inequality"))
                {
                    return true;
                }

                lastResolvedType = currentTypeDefinition.BaseType;
                TypeDefinition baseTypeDefinition = currentTypeDefinition.BaseType.Resolve();
                if (baseTypeDefinition == null)
                {
                    return null;
                }

                currentTypeDefinition = baseTypeDefinition;
            }

            return false;
        }

        public static string ToString(this FrameworkVersion self, bool includeVersionSign)
        {
            string result = string.Empty;
            switch (self)
            {
                case FrameworkVersion.v1_0:
                    result = "1.0";
                    break;
                case FrameworkVersion.v2_0:
                    result = "2.0";
                    break;
                case FrameworkVersion.v3_0:
                    result = "3.0";
                    break;
                case FrameworkVersion.v3_5:
                    result = "3.5";
                    break;
                case FrameworkVersion.v4_0:
                    result = "4.0";
                    break;
                case FrameworkVersion.v4_5:
                    result = "4.5";
                    break;
                case FrameworkVersion.v4_5_1:
                    result = "4.5.1";
                    break;
                case FrameworkVersion.v4_5_2:
                    result = "4.5.2";
                    break;
                case FrameworkVersion.v4_6:
                    result = "4.6";
                    break;
                case FrameworkVersion.v4_6_1:
                    result = "4.6.1";
                    break;
                case FrameworkVersion.v4_6_2:
                    result = "4.6.2";
                    break;
				case FrameworkVersion.v4_7:
					result = "4.7";
					break;
				case FrameworkVersion.v4_7_1:
					result = "4.7.1";
					break;
                /* AGPL */
                case FrameworkVersion.v4_7_2:
                    result = "4.7.2";
                    break;
                case FrameworkVersion.v4_8:
                    result = "4.8";
                    break;
                /* End AGPL */
                case FrameworkVersion.NetPortableV4_0:
                    result = ".NETPortable v4.0";
                    break;
                case FrameworkVersion.NetPortableV4_5:
					result = ".NETPortable v4.5";
					break;
				case FrameworkVersion.NetPortableV4_6:
                    result = ".NETPortable v4.6";
                    break;
                case FrameworkVersion.NetPortableV5_0:
                    result = ".NETPortable v5.0";
                    break;
                case FrameworkVersion.WinRT_4_5:
                    result = "WinRT - 4.5";
                    break;
                case FrameworkVersion.WinRT_4_5_1:
                    result = "WinRT - 4.5.1";
                    break;
                case FrameworkVersion.UWP:
                    result = "UWP";
                    break;
                /* AGPL */
                case FrameworkVersion.NetCoreV5_0:
                    result = "net5.0";
                    break;
                case FrameworkVersion.NetCoreV3_1:
                    result = "netcoreapp3.1";
                    break;
                case FrameworkVersion.NetCoreV3_0:
                    result = "netcoreapp3.0";
                    break;
                case FrameworkVersion.NetCoreV2_2:
                    result = "netcoreapp2.2";
                    break;
                /* End AGPL */
                case FrameworkVersion.NetCoreV2_1:
					result = "netcoreapp2.1";
					break;
				case FrameworkVersion.NetCoreV2_0:
					result = "netcoreapp2.0";
					break;
				case FrameworkVersion.NetCoreV1_1:
					result = "netcoreapp1.1";
					break;
				case FrameworkVersion.NetCoreV1_0:
					result = "netcoreapp1.0";
					break;
                /* AGPL */
                case FrameworkVersion.NetStandardV2_1:
                    result = "netstandard2.1";
                    break;
                case FrameworkVersion.NetStandardV2_0:
                    result = "netstandard2.0";
                    break;
                case FrameworkVersion.NetStandardV1_6:
                    result = "netstandard1.6";
                    break;
                case FrameworkVersion.NetStandardV1_5:
                    result = "netstandard1.5";
                    break;
                case FrameworkVersion.NetStandardV1_4:
                    result = "netstandard1.4";
                    break;
                case FrameworkVersion.NetStandardV1_3:
                    result = "netstandard1.3";
                    break;
                case FrameworkVersion.NetStandardV1_2:
                    result = "netstandard1.2";
                    break;
                case FrameworkVersion.NetStandardV1_1:
                    result = "netstandard1.1";
                    break;
                case FrameworkVersion.NetStandardV1_0:
                    result = "netstandard1.0";
                    break;
                /* End AGPL */
                case FrameworkVersion.Silverlight:
                case FrameworkVersion.WindowsCE:
                case FrameworkVersion.WindowsPhone:
                    return self.ToString();
                default:
                    return string.Empty;
            }

            if (includeVersionSign)
            {
                return "v" + result;
            }
            else
            {
                return result;
            }
        }
    }
}
