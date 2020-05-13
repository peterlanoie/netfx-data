using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Csv
{
	public class CsvParseException : Exception
	{
		public CsvParseException(string message) : base(message) { }
	}
}
