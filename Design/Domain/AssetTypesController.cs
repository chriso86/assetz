using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Assetz;

namespace Assetz.Controllers
{
	public class AssetTypesController : Controller
	{
		private IUnitOfWorkFactory uowFactory;
		private IRepository<AssetType> repository;

		public AssetTypesController()
		{
			AssetzModel context = new AssetzModel();
			this.uowFactory = new EntityFrameworkUnitOfWorkFactory(context);
			this.repository = new EntityFrameworkRepository<AssetType>(context);
		}
		
    public AssetTypesController(IUnitOfWorkFactory uowFactory, IRepository<AssetType> repository )
		{
			this.uowFactory = uowFactory;
			this.repository = repository;
		}

		//
		// GET: /AssetTypes

		public ViewResult Index(int? page, int? pageSize, string sortBy, bool? sortDesc )
		{
			// Defaults
			if (!page.HasValue)
				page = 1;
			if (!pageSize.HasValue)
				pageSize = 10;

			IQueryable<AssetType> query = repository.All();
			query = query.OrderBy(x => x.Id);
			
			// Paging
			int pageCount = (int)((query.Count() + pageSize - 1) / pageSize);
			if (page > 1)
				query = query.Skip((page.Value - 1) * pageSize.Value);
			query = query.Take(pageSize.Value);
			if (page > 1)
				ViewBag.Page = page.Value;
			if (pageSize != 10)
				ViewBag.PageSize = pageSize.Value;
			if (pageCount > 1) {
				int currentPage = page.Value;
				const int visiblePages = 5;
				const int pageDelta = 2;
				List<Tuple<string, bool, int>> paginationData = new List<Tuple<string, bool, int>>(); // text, enabled, page index
				paginationData.Add(new Tuple<string, bool, int>("Prev", currentPage > 1, currentPage - 1));
				if (pageCount <= visiblePages * 2) {
					for (int i = 1; i <= pageCount; i++)
						paginationData.Add(new Tuple<string, bool, int>(i.ToString(), true, i));
				}
				else {
					if (currentPage < visiblePages) {
						// 12345..10
						for (int i = 1; i <= visiblePages; i++)
							paginationData.Add(new Tuple<string, bool, int>(i.ToString(), true, i));
						paginationData.Add(new Tuple<string, bool, int>("...", false, -1));
						paginationData.Add(new Tuple<string, bool, int>(pageCount.ToString(), true, pageCount));
					}
					else if (currentPage > pageCount - (visiblePages - 1)) {
						// 1..678910
						paginationData.Add(new Tuple<string, bool, int>("1", true, 1));
						paginationData.Add(new Tuple<string, bool, int>("...", false, -1));
						for (int i = pageCount - (visiblePages - 1); i <= pageCount; i++)
							paginationData.Add(new Tuple<string, bool, int>(i.ToString(), true, i));
					}
					else {
						// 1..34567..10
						paginationData.Add(new Tuple<string, bool, int>("1", true, 1));
						paginationData.Add(new Tuple<string, bool, int>("...", false, -1));
						for (int i = currentPage - pageDelta, count = currentPage + pageDelta; i <= count; i++)
							paginationData.Add(new Tuple<string, bool, int>(i.ToString(), true, i));
						paginationData.Add(new Tuple<string, bool, int>("...", false, -1));
						paginationData.Add(new Tuple<string, bool, int>(pageCount.ToString(), true, pageCount));
					}
				}
				paginationData.Add(new Tuple<string, bool, int>("Next", currentPage < pageCount, currentPage + 1));
				ViewBag.PaginationData = paginationData;
			}

			// Sorting
			if (!string.IsNullOrEmpty(sortBy)) {
				bool ascending = !sortDesc.HasValue || !sortDesc.Value;
				if (sortBy == "Name")
					query = OrderBy(query, x => x.Name, ascending);
				if (sortBy == "Description")
					query = OrderBy(query, x => x.Description, ascending);
				if (sortBy == "Code")
					query = OrderBy(query, x => x.Code, ascending);
				if (sortBy == "Image")
					query = OrderBy(query, x => x.Image, ascending);
				ViewBag.SortBy = sortBy;
				if (sortDesc != null && sortDesc.Value)
					ViewBag.SortDesc = sortDesc.Value;
			}

			ViewBag.Entities = query.ToList();
			return View();
		}

		//
		// GET: /AssetTypes/Create

		public ActionResult Create()
		{
		    return View();
		} 
		
		//
		// POST: /AssetTypes/Create
		
		[HttpPost]
		public ActionResult Create(AssetType entity)
		{
			if (ModelState.IsValid)
				using (IUnitOfWork uow = uowFactory.Create()) {
					repository.Add(entity);
					uow.Save();
					return RedirectToAction("Index");
				}
			else
				return View();
		}

		//
		// GET: /AssetTypes/Details
		
		public ViewResult Details(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}


		//
		// GET: /AssetTypes/Edit
				
		public ActionResult Edit(int Id)
		{
			var entity = repository.All().Single(x => x.Id == Id);
			return View(entity);
		}
				
		//
		// POST: /AssetTypes/Edit
				
		[HttpPost]
		public ActionResult Edit(AssetType entity)
		{
			if (ModelState.IsValid)
				using (IUnitOfWork uow = uowFactory.Create()) {
					AssetType original = repository.All().Single(x => x.Id == entity.Id);
					original.Id = entity.Id;
					original.Name = entity.Name;
					original.Description = entity.Description;
					original.Code = entity.Code;
					original.Image = entity.Image;
					uow.Save();
					return RedirectToAction("Index");
				}
			else
				return View();
		}
		
		//
		// GET: /AssetTypes/Delete
		
		public ActionResult Delete(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}
		
		//
		// POST: /AssetTypes/Delete
		
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int Id)
		{
			using (IUnitOfWork uow = uowFactory.Create()) {
				repository.Remove(repository.All().Single(x => x.Id == Id));
				uow.Save();
				return RedirectToAction("Index");
			}
		}

		private static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(IQueryable<TSource> source, System.Linq.Expressions.Expression<Func<TSource, TKey>> keySelector, bool ascending) {

			return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
		}
	}
}

