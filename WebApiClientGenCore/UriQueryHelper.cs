﻿using Fonlow.Web.Meta;
using System;
using System.Diagnostics;
using System.Linq;
using Tavis.UriTemplates;

namespace Fonlow.CodeDom.Web
{
	/// <summary>
	/// Generate a client function upon ApiDescription
	/// </summary>
	public static class UriQueryHelper
	{
		static readonly Type typeofString = typeof(string);
		static readonly Type typeofDateTime = typeof(DateTime);
		static readonly Type typeofDateTimeNullable = typeof(DateTime?);
		static readonly Type typeofDateTimeOffset = typeof(DateTimeOffset);
		static readonly Type typeofDateTimeOffsetNullable = typeof(DateTimeOffset?);

		public static string CreateUriQuery(string uriText, ParameterDescription[] parameterDescriptions)
		{
			var template = new UriTemplate(uriText);
			var parameterNames = template.GetParameterNames().ToArray();
			if (parameterNames.Length == 0)
				return null;

			string newUriText = uriText;

			Func<ParameterDescription, string> GetUriText = (d) =>
			{
				if (d.ParameterDescriptor.ParameterType == typeofString)
				{
					return newUriText.Replace($"{{{d.Name}}}", $"\"+Uri.EscapeDataString({d.Name})+\"");
				}
				else if ((d.ParameterDescriptor.ParameterType == typeofDateTime) || (d.ParameterDescriptor.ParameterType == typeofDateTimeOffset))
				{
					return newUriText.Replace($"{{{d.Name}}}", $"\"+{d.Name}.ToUniversalTime().ToString(\"yyyy-MM-ddTHH:mm:ss.fffffffZ\")+\"");
				}
				else
				{
					return newUriText.Replace($"{{{d.Name}}}", $"\"+{d.Name}+\"");
				}
			};

			for (int i = 0; i < parameterNames.Length; i++)
			{
				var name = parameterNames[i];//PathSegmentVariableNames[i] always give uppercase
				var d = parameterDescriptions.FirstOrDefault(r => r.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
				Debug.Assert(d != null);

				newUriText = GetUriText(d);
			}

			return newUriText;
		}

		public static string CreateUriQueryForTs(string uriText, ParameterDescription[] parameterDescriptions)
		{
			var template = new UriTemplate(uriText);
			var parameterNames = template.GetParameterNames().ToArray();
			if (parameterNames.Length == 0)
				return null;

			string newUriText = uriText;

			for (int i = 0; i < parameterNames.Length; i++)
			{
				var name = parameterNames[i];//PathSegmentVariableNames[i] always give uppercase
				var d = parameterDescriptions.FirstOrDefault(r => r.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
				Debug.Assert(d != null);
				if (d.ParameterDescriptor.ParameterType == typeofString)
				{
					newUriText = newUriText.Replace($"{{{d.Name}}}", $"' + encodeURIComponent({d.Name}) + '");
				}
				else if (d.ParameterDescriptor.ParameterType == typeofDateTime || d.ParameterDescriptor.ParameterType == typeofDateTimeOffset || d.ParameterDescriptor.ParameterType == typeofDateTimeNullable || d.ParameterDescriptor.ParameterType == typeofDateTimeOffsetNullable)
				{
					newUriText = newUriText.Replace($"{{{d.Name}}}", $"' + {d.Name}?{d.Name}.toISOString():null + '");
				}
				else
				{
					newUriText = newUriText.Replace($"{{{d.Name}}}", $"' + {d.Name} + '");
				}
			}

			return newUriText;
		}

	}

}