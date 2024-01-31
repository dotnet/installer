// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.DotNet.Build.Tasks
{
    // Takes a path to a json file and a string that represents a dotted path to an attribute
    // and returns the attribute value.
    public class GetJSonAttributeValue : Task
    {
        [Required]
        public string JsonFilePath { get; set; }

        [Required]
        public string PathToAttribute { get; set; }

        /// <summary>
        /// The results returned by this task.
        /// </summary>
        [Output]
        public ITaskItem[] Result { get; private set; }

        public override bool Execute()
        {
            // Using a character that isn't allowed in the package id
            const char Delimiter = ':';

            string json = File.ReadAllText(JsonFilePath);
            JObject jsonObj = JObject.Parse(json);

            string[] escapedPathToAttributeParts = PathToAttribute.Split(Delimiter);
            string value = GetAttributeValue(jsonObj, escapedPathToAttributeParts);
            if (!String.IsNullOrEmpty(value))
            {
                Result = new ITaskItem[1];
                Result[0] = new TaskItem(value);
            }

            return true;
        }

        private string GetAttributeValue(JToken jsonObj, string[] path)
        {
            string pathItem = path[0];
            if (jsonObj[pathItem] == null)
            {
                return null;
            }

            if (path.Length == 1)
            {
                return (string)jsonObj[pathItem];
            }

            return GetAttributeValue(jsonObj[pathItem], path.Skip(1).ToArray());
        }
    }
}
