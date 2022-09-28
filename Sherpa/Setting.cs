namespace Sherpa
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Win32;

    /// <summary>
    /// A setting for this service.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Gets the registry value.
        /// </summary>
        /// <typeparam name="T">Type of the registry value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <returns>The registry value in the desired type.</returns>
        protected T GetRegistryValue<T>(RegistryKey key, string name)
        {
            object keyValue = key.GetValue(name);
            if (keyValue != null)
            {
                return this.ConvertEncodedValueToType<T>(keyValue.ToString());
            }

            return default(T);
        }

        /// <summary>
        /// Converts the type of the encoded value to.
        /// </summary>
        /// <typeparam name="T">Type that the encoded value will be converted to.</typeparam>
        /// <param name="encodedValue">The encoded value.</param>
        /// <returns>
        /// Value that was encoded, in its proper type.
        /// </returns>
        protected T ConvertEncodedValueToType<T>(string encodedValue)
        {
            try
            {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
                return (T)tc.ConvertFromString(null, CultureInfo.InvariantCulture, encodedValue);
            }
            catch (Exception e)
            {
                // Unfortunately, TypeConverter wraps any exceptions with a generic Exception, so
                // we need to unwrap it here and ensure it is an exception we expect instead of
                // something nasty as NullReferenceException.
                if (e.InnerException == null ||
                    (!(e.InnerException is NotSupportedException) &&
                    !(e.InnerException is ArgumentException) &&
                    !(e.InnerException is OverflowException) &&
                    !(e.InnerException is FormatException)))
                {
                    throw e;
                }

                return default(T);
            }
        } 
    }
}
