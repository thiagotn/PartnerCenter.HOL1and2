namespace PartnerCenter.HOL
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.Store.PartnerCenter.Models;

    public static class Helpers
    {
        /// <summary>
        /// Writes an object and its properties recursively to the console. Properties are automatically indented.
        /// </summary>
        /// <param name="object">The object to print to the console.</param>
        /// <param name="title">An optional title to display.</param>
        /// <param name="indent">The starting indentation.</param>
        public static void WriteObject(object @object, string title = default(string), int indent = 0)
        {
            if (@object == null)
            {
                return;
            }

            const int TabSize = 4;
            bool isTitlePresent = !string.IsNullOrWhiteSpace(title);
            string indentString = new string(' ', indent * TabSize);
            Type objectType = @object.GetType();
            var collection = @object as ICollection;

            if (objectType.Assembly.FullName == typeof(ResourceBase).Assembly.FullName && objectType.IsClass)
            {
                // this is a partner SDK model class, iterate over it's properties recursively
                if (indent == 0 && !string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine(title);
                    Console.WriteLine(new string('-', 90));
                }
                else if (isTitlePresent)
                {
                    Helpers.WriteColored(string.Format(CultureInfo.InvariantCulture, "{0}{1}: ", indentString, title), ConsoleColor.Yellow);
                }
                else
                {
                    // since the current element does not have a title, we do not want to shift it's children causing a double shift appearance
                    // to the user
                    indent--;
                }

                PropertyInfo[] properties = objectType.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    Helpers.WriteObject(property.GetValue(@object), property.Name, indent + 1);
                }

                if (indent == 0 && !string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine(new string('-', 90));
                }
            }
            else if (collection != null)
            {
                // this is a collection, loop through its element and print them recursively
                Helpers.WriteColored(string.Format(CultureInfo.InvariantCulture, isTitlePresent ? "{0}{1}: " : string.Empty, indentString, title), ConsoleColor.Yellow);

                foreach (var element in collection)
                {
                    Helpers.WriteObject(element, indent: indent + 1);
                    var elementType = element.GetType();

                    if (indent == 1)
                    {
                        Console.WriteLine(new string('-', 80));
                    }
                }
            }
            else
            {
                // print the object as is
                Helpers.WriteColored(string.Format(CultureInfo.InvariantCulture, isTitlePresent ? "{0}{1}: " : "{0}", indentString, title), ConsoleColor.DarkYellow, false);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}", @object));
            }
        }

        /// <summary>
        /// Writes a message with the requested color to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="color">The console color to use.</param>
        /// <param name="newLine">Whether or not to write a new line after the message.</param>
        private static void WriteColored(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            Console.Write(message + (newLine ? "\n" : string.Empty));
            Console.ResetColor();
        }
    }
}
