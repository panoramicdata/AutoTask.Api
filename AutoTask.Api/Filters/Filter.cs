using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Filters
{
	public class Filter
	{
		private readonly string _itemString;
		private readonly string _fieldString;

		public Filter()
		{
		}

		public Filter(
			string filterString = null,
			string fieldString = null
			)
		{
			_itemString = filterString;
			_fieldString = fieldString;
			Items = string.IsNullOrWhiteSpace(filterString)
				? new List<FilterItem>()
				: filterString.Split(',').Select(text => new FilterItem(text)).ToList();
			Fields = string.IsNullOrWhiteSpace(fieldString)
				? new List<string>()
				: fieldString.Split(',').ToList();
		}

		public List<FilterItem> Items { get; set; }

		public List<string> Fields { get; set; }

		public override string ToString()
			=> $"Items={_itemString};Fields={_fieldString}";
	}
}
