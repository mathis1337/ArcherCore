﻿using ArcherCore.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArcherCore.Extensions;

namespace ArcherCore.Passwords
{
    [Flags]
    public enum PasswordReq
    {
        // Password policy for types of passwords.
        // like Microsoft.AspNetCore.Identity.PasswordOptions
        None = 0,
        Lowercase = 1,  // must have lowercase.
        Uppercase = 2,
        Digit = 4,      // Numbers. RequireDigit
        NonAlphanumeric = 8,    // must have special char.  RequireNonAlphanumeric
        UniqueChars = 16,       // needs unique chars . cant just repeat ??
        Def = Lowercase | Uppercase | Digit,
    }

    public class PasswordUtilities
    {
        public static readonly PasswordUtilities Def = new PasswordUtilities(8, 16, PasswordReq.Def);

        public const int kMinLength = 8;

        public readonly int MinLength = kMinLength;
        public readonly int MaxLength = 128;     // These are hashed, there should be no max.
        public readonly PasswordReq ReqFlags = PasswordReq.Def;

        public static readonly char[] _numberChars = "1234567890".ToCharArray();
        public static readonly char[] _alnumChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        private static readonly char[] _SpecialChars = "!@#%".ToCharArray();    // Allowed or required for passwords.
        private static readonly char[] _InvalidChars = "/.,;'\\][\"}{:\"?><+_)(*&^$~`".ToCharArray();   // Not allowed.

        // For auto generated passwords.
        // Removed "iloILO01" from use since they can get confused by users? (26+26+10)-8 = 54
        public static readonly char[] _BasicAlnumChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789".ToCharArray();

        public PasswordUtilities()
        {
        }

        public PasswordUtilities(int minLen, int maxLen, PasswordReq flags)
        {
            MinLength = minLen;
            MaxLength = maxLen;
            ReqFlags = flags;
        }
        public static SecureString CreatePassword(int maxSize, char[] chars, bool lowerCase = false)
        {
            // Generate a random password using chars supplied.
            // chars = restrict to this set of chars.
            // lowerCase = force to lower case.
            bool passwordGood = false;
            SecureString password = null;
            while(!passwordGood)
            {
                var crypto = RandomNumberGenerator.Create();

                byte[] data1 = new byte[1];     // junk first byte.
                crypto.GetNonZeroBytes(data1);

                byte[] data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);

                var result = new StringBuilder();
                foreach (byte b in data)
                {
                    char ch = chars[b % (chars.Length)];
                    if (lowerCase)
                    {
                        // Force to lower.
                        ch = char.ToLower(ch);
                    }
                    result.Append(ch);
                }
                var passCheck = GetPasswordError(result.ToString());
                if(passCheck)
                {
                    password = result.ToString().ToSecureString();
                    passwordGood = true;
                }
            }

            return password; //this should never be null
            
        }

        public static SecureString CreatePassCode(int maxSize = 4)
        {
            // Generate a maxSize digit random numeric passcode.
            return CreatePassword(maxSize, _numberChars);
        }

        public static SecureString CreateUniqueKey(int maxSize = 10)
        {
            return CreatePassword(maxSize, _BasicAlnumChars);
        }

        public static bool IsRandomish(string s)
        {

            if (s == null)
                return false;
            if (s.Length < kMinLength)
                return false;

            // TODO

            return true;
        }

        public static bool GetPasswordError(string password, string? excludeToken = null)
        {
            // Match a password against password policy.
            // Second entry of the password to see if it matches the first does not need to come here.
            if (password == null)
                password = "";

            if (password.Length < Def.MinLength)
            {
                if (password.Length <= 0)
                    return false;

            }
            if (password.Length > Def.MaxLength)
            {
                return false;
            }

            if ((Def.ReqFlags & PasswordReq.Lowercase) != 0 && !StringUtilities.HasLowerCase(password))
            {
                return false;
            }
            if ((Def.ReqFlags & PasswordReq.Uppercase) != 0 && !StringUtilities.HasUpperCase(password))
            {
                return false;
            }
            if ((Def.ReqFlags & PasswordReq.Digit) != 0 && !StringUtilities.HasNumber(password))
            {
                return false;
            }
            if ((Def.ReqFlags & PasswordReq.NonAlphanumeric) != 0 && !StringUtilities.HasNumber(password))
            {
                return false;
            }

            if (HasInvalidChars(password))
            {
                return false;
            }

            // Optional checks.
            // e.g. dont put your own username in password 

            if (excludeToken != null && password.Contains(excludeToken))
            {
                return false;
            }

            return true;
        }

        public static bool HasValidSpecial(string password)
        {
            int indexOf = password.IndexOfAny(_SpecialChars);
            if (indexOf == -1)
                return false;
            else
                return true;
        }

        public static bool HasInvalidChars(string password)
        {
            // Does NOT have _InvalidChars
            int indexOf = password.IndexOfAny(_InvalidChars);
            if (indexOf == -1)
                return false;
            else
                return true;
        }

        public static bool IsMatch(string providedpwd, string currentpwd)
        {
            // Match ?
            if (string.Equals(providedpwd, currentpwd))
                return true;
            return false;
        }

        public List<string> GetPasswordErrorMessages(string password, string? excludeToken = null)
        {
            // Match a password against password policy.
            // Second entry of the password to see if it matches the first does not need to come here.

            var errors = new List<string>();

            if (password == null)
                password = "";

            if (password.Length < MinLength)
            {
                errors.Add("Must be at least " + MinLength + " characters long");
                if (password.Length <= 0)
                    return errors;

            }
            if (password.Length > MaxLength)
            {
                errors.Add("Must be less than " + MaxLength + " characters");
            }

            if ((this.ReqFlags & PasswordReq.Lowercase) != 0 && !StringUtilities.HasLowerCase(password))
            {
                errors.Add("Must have lower case characters");
            }
            if ((this.ReqFlags & PasswordReq.Uppercase) != 0 && !StringUtilities.HasUpperCase(password))
            {
                errors.Add("Must have upper case characters");
            }
            if ((this.ReqFlags & PasswordReq.Digit) != 0 && !StringUtilities.HasNumber(password))
            {
                errors.Add("Must have numbers");
            }
            if ((this.ReqFlags & PasswordReq.NonAlphanumeric) != 0 && !StringUtilities.HasNumber(password))
            {
                errors.Add("Must have special characters '" + _SpecialChars + "'");
            }

            if (HasInvalidChars(password))
            {
                errors.Add("Has invalid characters");
            }

            // Optional checks.
            // e.g. dont put your own username in password 

            if (excludeToken != null && password.Contains(excludeToken))
            {
                errors.Add("Contains text that is not allowed");
            }

            return errors;
        }
    }
}
