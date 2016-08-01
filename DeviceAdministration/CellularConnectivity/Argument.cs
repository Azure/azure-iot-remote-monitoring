using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceManagement.Infrustructure.Connectivity
{
    /// <summary>
    ///     Represents a validator that simplifies checking method and constructor arguments and throws the
    ///     appropriate exceptions.
    /// </summary>
    internal static class Argument
    {
        /// <summary>
        ///     Determine whether an argument is null and if it is, throw an <see cref="ArgumentNullException" />.
        /// </summary>
        /// <param name="argument">The argument to be checked.</param>
        /// <param name="argumentName">The name of the argument to be checked.</param>
        /// <param name="argumentDescription">The description of the argument to be checked.</param>
        public static void CheckIfNull([ValidatedNotNull] object argument, string argumentName,
            string argumentDescription)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);
            VerifyArgumentDescriptionIsNotNullOrEmpty(argumentDescription);

            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, argumentDescription + " cannot be null.");
            }
        }

        private static void VerifyArgumentDescriptionIsNotNullOrEmpty(string argumentDescription)
        {
            if (argumentDescription == null)
            {
                throw new ArgumentNullException("argumentDescription", "argumentDescription cannot be null.");
            }
            if (string.IsNullOrEmpty(argumentDescription))
            {
                throw new ArgumentException("argumentDescription cannot be empty.", "argumentDescription");
            }
        }

        private static void VerifyArgumentNameIsNotNullOrEmpty(string argumentName)
        {
            if (argumentName == null)
            {
                throw new ArgumentNullException("argumentName", "argumentName cannot be null.");
            }
            if (string.IsNullOrEmpty(argumentName))
            {
                throw new ArgumentException("argumentName cannot be empty.", "argumentName");
            }
        }

        /// <summary>
        ///     Determine whether an argument is null and if it is, throw an <see cref="ArgumentNullException" />.
        /// </summary>
        /// <param name="argument">The argument to be checked.</param>
        /// <param name="argumentName">The name of the argument to be checked.</param>
        public static void CheckIfNull([ValidatedNotNull] object argument, string argumentName)
        {
            CheckIfNull(argument, argumentName, argumentName);
        }

        /// <summary>
        ///     Determine whether a string argument is null or empty, throws an <see cref="ArgumentNullException" />
        ///     if it is null or an <see cref="ArgumentException" /> if it is empty.
        /// </summary>
        /// <param name="argument">The string argument to be checked.</param>
        /// <param name="argumentName">The name of the string argument to be checked.</param>
        public static void CheckIfNullOrEmpty([ValidatedNotNull] Guid argument, string argumentName)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);
            VerifyArgumentDescriptionIsNotNullOrEmpty(argumentName);

            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, argumentName + " cannot be null.");
            }
            if (argument == Guid.Empty)
            {
                throw new ArgumentException(argumentName + " cannot be empty.", argumentName);
            }
        }

        /// <summary>
        ///     Determine whether a string argument is null or empty, throws an <see cref="ArgumentNullException" />
        ///     if it is null or an <see cref="ArgumentException" /> if it is empty.
        /// </summary>
        /// <param name="argument">The string argument to be checked.</param>
        /// <param name="argumentName">The name of the string argument to be checked.</param>
        /// <param name="argumentDescription">The description of the string argument to be checked.</param>
        public static void CheckIfNullOrEmpty([ValidatedNotNull] string argument, string argumentName,
            string argumentDescription)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);
            VerifyArgumentDescriptionIsNotNullOrEmpty(argumentDescription);

            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, argumentDescription + " cannot be null.");
            }
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException(argumentDescription + " cannot be empty.", argumentName);
            }
        }

        /// <summary>
        ///     Determine whether a string argument is null or empty, throws an <see cref="ArgumentNullException" />
        ///     if it is null or an <see cref="ArgumentException" /> if it is empty.
        /// </summary>
        /// <param name="argument">The string argument to be checked.</param>
        /// <param name="argumentName">The name of the string argument to be checked.</param>
        public static void CheckIfNullOrEmpty([ValidatedNotNull] string argument, string argumentName)
        {
            CheckIfNullOrEmpty(argument, argumentName, argumentName);
        }

        /// <summary>
        ///     Determine whether a given database id is valid, i.e. at least 1.
        /// </summary>
        /// <param name="databaseId">The database id to be checked.</param>
        /// <param name="argumentName">The name of the database id parameter to be checked.</param>
        /// <param name="argumentDescription">The description of the database id parameter to be checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">if database id is less than 1.</exception>
        public static void CheckDatabaseId([ValidatedNotNull] int databaseId, string argumentName,
            string argumentDescription)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);
            VerifyArgumentDescriptionIsNotNullOrEmpty(argumentDescription);


            if (databaseId < 1)
            {
                throw new ArgumentOutOfRangeException(argumentName, databaseId,
                    argumentDescription + " must be at least 1.");
            }
        }

        /// <summary>
        ///     Determine whether a given database id is valid, i.e. at least 1.
        /// </summary>
        /// <param name="databaseId">The database id to be checked.</param>
        /// <param name="databaseIdName">The name of the database id to be checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">if database id is less than 1.</exception>
        public static void CheckDatabaseId([ValidatedNotNull] int databaseId, string databaseIdName)
        {
            CheckDatabaseId(databaseId, databaseIdName, databaseIdName);
        }

        /// <summary>
        ///     Checks if list is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CheckIfNullOrEmpty<T>([ValidatedNotNull] IEnumerable<T> enumerable, string argumentName)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);

            if (enumerable == null)
            {
                throw new ArgumentNullException(argumentName, "The list cannot be null.");
            }
            if (enumerable.Count() == 0)
            {
                throw new ArgumentException("The list cannot be empty.", argumentName);
            }
        }

        /// <summary>
        ///     Checks the length of the string is less than or equal to the specified maxLength
        /// </summary>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">The argument name.</param>
        /// <param name="maxLength">Length of the max.</param>
        public static void CheckStringNullOrLength([ValidatedNotNull] string argument, string argumentName,
            int maxLength)
        {
            CheckIfNull(argument, argumentName);
            if (argument.Length > maxLength)
            {
                throw new ArgumentException(
                    "The length of {0} exceeds {1} characters".FormatWith(argumentName, maxLength), argumentName);
            }
        }

        /// <summary>
        ///     Checks the length of the argument is not empty and is less than or equal to the specified length.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="maxLength">Length of the max.</param>
        public static void CheckNullEmptyOrLength<T>([ValidatedNotNull] IEnumerable<T> argument, string argumentName,
            int maxLength)
        {
            CheckIfNullOrEmpty(argument, argumentName);

            if (argument.Count() > maxLength)
            {
                throw new ArgumentException("The length of {0} exceeds {1}".FormatWith(argumentName, maxLength),
                    argumentName);
            }
        }

        /// <summary>
        ///     Checks the length of the argument is not empty and is less than or equal to the specified length.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="maxLength">Length of the max.</param>
        public static void CheckNullEmptyOrLength([ValidatedNotNull] string argument, string argumentName, int maxLength)
        {
            CheckIfNullOrEmpty(argument, argumentName);
            CheckStringNullOrLength(argument, argumentName, maxLength);
        }

        /// <summary>
        ///     Checks the database unique identifier.
        /// </summary>
        public static void CheckDatabaseId(Guid argument, string argumentName)
        {
            VerifyArgumentNameIsNotNullOrEmpty(argumentName);
            if (argument == Guid.Empty)
            {
                throw new ArgumentException("The database id of {0} is empty".FormatWith(argumentName), argumentName);
            }
        }

        public static string FormatWith(this string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return string.Format(format, args);
        }

        public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return string.Format(provider, format, args);
        }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}