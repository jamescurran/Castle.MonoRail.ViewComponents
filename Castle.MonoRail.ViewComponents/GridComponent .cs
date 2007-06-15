﻿// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.ViewComponents
{
	using System;
	using System.Collections;
	using System.IO;
	using Castle.MonoRail.Framework;
	using Castle.MonoRail.Framework.Helpers;

	public class GridComponent : ViewComponent
	{
		private static readonly string[] sections = new string[]
			{
				"header", "footer",
				"pagination", "empty",
				"item", "alternateItem",
				"tablestart", "tableend",
				"paginationLink",
			};

		public override bool SupportsSection(string name)
		{
			foreach(string section in sections)
			{
				if (section.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public override void Render()
		{
			IEnumerable source = ComponentParams["source"] as IEnumerable;
			
			if (source == null)
			{
				throw new ViewComponentException(
					"The grid requires a view component parameter named 'source' which should contain 'IEnumerable' instance");
			}

			ShowStartTable();
			ShowHeader(source);

			bool hasItems = ShowRows(source);

			ShowFooter();
			ShowEndTable();

			if (hasItems == false)
			{
				ShowEmpty();
			}

			IPaginatedPage page = source as IPaginatedPage;
			
			if (page != null)
			{
				ShowPagination(page);
			}
		}

		/// <summary>
		/// Render the rows from the source, return true if there are rows to render,
		/// false otherwise.
		/// </summary>
		protected virtual bool ShowRows(IEnumerable source)
		{
			bool hasAlternate = Context.HasSection("alternateItem");
			bool isAlternate = false;
			bool hasItems = false;
			
			foreach(object item in source)
			{
				hasItems = true;
				PropertyBag["item"] = item;

				if (hasAlternate && isAlternate)
				{
					Context.RenderSection("alternateItem");
				}
				else
				{
					Context.RenderSection("item");
				}

				isAlternate = !isAlternate;
			}
			
			return hasItems;
		}

		private void ShowEmpty()
		{
			if (Context.HasSection("empty"))
			{
				Context.RenderSection("empty");
			}
			else
			{
				RenderText("Grid has no data");
			}
		}

		private void ShowPagination(IPaginatedPage page)
		{
			PropertyBag["currentPage"] = page;
			
			if (Context.HasSection("pagination"))
			{
				Context.RenderSection("pagination");
				return;
			}
			
			PaginationHelper paginationHelper = (PaginationHelper) Context.ContextVars["PaginationHelper"];
			StringWriter output = new StringWriter();
			output.Write(
				@"<div class='pagination'><span class='paginationLeft'>
Showing {0} - {1} of {2} 
</span><span class='paginationRight'>",
				page.FirstItem, page.LastItem, page.TotalItems);
			
			if (page.HasFirst)
			{
				CreateLink(output, paginationHelper, 1, "first");
			}
			else
			{
				output.Write("first");
			}

			output.Write(" | ");
			
			if (page.HasPrevious)
			{
				CreateLink(output, paginationHelper, page.PreviousIndex, "prev");
			}
			else
			{
				output.Write("prev");
			}

			output.Write(" | ");
			
			if (page.HasNext)
			{
				CreateLink(output, paginationHelper, page.NextIndex, "next");
			}
			else
			{
				output.Write("next");
			}

			output.Write(" | ");
			
			if (page.HasLast)
			{
				CreateLink(output, paginationHelper, page.LastIndex, "last");
			}
			else
			{
				output.Write("last");
			}

			output.Write(@"</span></div>");

			RenderText(output.ToString());
		}

		private void CreateLink(TextWriter output, PaginationHelper paginationHelper, int pageIndex, string title)
		{
			if (Context.HasSection("paginationLink"))
			{
				PropertyBag["pageIndex"] = pageIndex;
				PropertyBag["title"] = title;
				Context.RenderSection("paginationLink", output);
			}
			else
			{
				output.Write(paginationHelper.CreatePageLink(pageIndex, title, null, QueryStringAsDictionary()));
			}
		}

		private IDictionary QueryStringAsDictionary()
		{
			Hashtable table = new Hashtable();

			foreach(string key in Request.QueryString)
			{
				if (key == null) continue;
				
				table[key] = Request.QueryString[key];
			}

			return table;
		}

		private void ShowStartTable()
		{
			if (Context.HasSection("tablestart"))
			{
				Context.RenderSection("tablestart");
			}
			else
			{
				RenderText("<table class='grid' cellspacing='0' cellpadding='0'>");
			}
		}

		private void ShowEndTable()
		{
			if (Context.HasSection("tableend"))
			{
				Context.RenderSection("tableend");
			}
			else
			{
				RenderText("</table>");
			}
		}

		private void ShowFooter()
		{
			if (Context.HasSection("footer"))
			{
				Context.RenderSection("footer");
			}
		}

		protected virtual void ShowHeader(IEnumerable source)
		{
			if (Context.HasSection("header") == false)
			{
				throw new ViewComponentException("A GridComponent must has a header");
			}
			Context.RenderSection("header");
		}
	}
}