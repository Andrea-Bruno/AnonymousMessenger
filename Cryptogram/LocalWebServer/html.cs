using System.Collections.Generic;
using System.Web;

namespace LocalWebServer
{
	class Attribute
	{
		public Attribute(Attributes attribute, string value)
		{
			Name = attribute.ToString();
			Value = value;
		}
		readonly string Name;
		string Value;
		public string Render()
		{
			return Value == null ? null : " " + Name + "=" + Value;
		}
	}

	class Tag
	{
		public Tag(Tags tag, List<Attribute> attributes = null, List<Tag> children= null)
		{
			Name = tag.ToString();
			Attributes = attributes;
			Children= children;
		}

		public Tag(Tags tag, List<Tag> children = null)
		{
			Name = tag.ToString();
			Children = children;
		}

		public List<Attribute> Attributes = new List<Attribute>();
		public readonly string Name;
		private string _innerHtml;
		public string InnerHtml
		{
			get
			{
				if (_innerHtml != null)
					return _innerHtml;
				else
				{
					string result = "";
					foreach (var child in Children)
					{
						result += child.OuterHtml;
					}
					return result;
				}
			}
			set { _innerHtml = value; }
		}
		public string InnerText
		{
			get
			{
				return HttpUtility.HtmlDecode(_innerHtml);
			}
			set { _innerHtml = HttpUtility.HtmlEncode(value); }
		}
		public List<Tag> Children = new List<Tag>();
		public string OuterHtml
		{
			get
			{

				string attributes = "";
				foreach (var attribute in Attributes)
				{
					attributes += attribute.Render();
				}
				return @"<" + Name + attributes + @">" + InnerHtml + @"</" + Name + @">";
			}
		}
		public string Render()
		{
			return OuterHtml;
		}
	}
	public enum Attributes
	{
		id,
		@class,
		title,
		style,
		dir,
		onclick,
		ondblclick,
		href,
		src,
		alt
	}
	public enum Tags
	{
		html,
		head,
		body,
		title,
		h1,
		h2,
		h3,
		h4,
		h5,
		h6,
		p,
		em,
		b,
		i,
		small,
		u,
		strike,
		a,
		li,
		ol,
		ul,
		marquee,
		center,
		font,
		br,
		img,
		link,
		hr,
		meta,
		table,
		tr,
		th,
		td,
		form,
		imput,
		option,
		span,
		div
	}
}
