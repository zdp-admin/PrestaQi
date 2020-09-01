using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class EncryptedAttribute : Attribute
	{
		readonly string _fieldName;

		public EncryptedAttribute(string fieldName)
		{
			_fieldName = fieldName;
		}

		public string FieldName => _fieldName;
	}
}
