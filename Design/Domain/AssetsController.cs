using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Assetz;

namespace Assetz.Controllers
{
	public class AssetsController : Controller
	{
		private IUnitOfWorkFactory uowFactory;
		private IRepository<Asset> repository;
		private IRepository<AssetType> AssetTypeRepository;
		private IRepository<Account> AccountRepository;

		public AssetsController()
		{
			AssetzModel context = new AssetzModel();
			this.uowFactory = new EntityFrameworkUnitOfWorkFactory(context);
			this.repository = new EntityFrameworkRepository<Asset>(context);
			this.AssetTypeRepository = new EntityFrameworkRepository<AssetType>(context);
			this.AccountRepository = new EntityFrameworkRepository<Account>(context);
		}
		
    public AssetsController(IUnitOfWorkFactory uowFactory, IRepository<Asset> repository , IRepository<AssetType> AssetTypeRepository, IRepository<Account> AccountRepository)
		{
			this.uowFactory = uowFactory;
			this.repository = repository;
			this.AssetTypeRepository = AssetTypeRepository;
			this.AccountRepository = AccountRepository;
		}

		//
		// GET: /Assets

		public ViewResult Index(int? page, int? pageSize, string sortBy, bool? sortDesc , int? AssetTypeId, int? AccountId)
		{
			// Defaults
			if (!page.HasValue)
				page = 1;
			if (!pageSize.HasValue)
				pageSize = 10;

			IQueryable<Asset> query = repository.All();
			query = query.OrderBy(x => x.Id);
			// Filtering
			List<SelectListItem> selectList;
			if (AssetTypeId != null) {
				query = query.Where(x => x.AssetTypeId == AssetTypeId);
				ViewBag.AssetTypeId = AssetTypeId;
			}
			selectList = new List<SelectListItem>();
			selectList.Add(new SelectListItem() { Text = null, Value = null, Selected = AssetTypeId == null });
			selectList.AddRange(AssetTypeRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Name.ToString(), Value = x.Id.ToString(), Selected = AssetTypeId != null && AssetTypeId == x.Id }));
			ViewBag.AssetTypes = selectList;
			ViewBag.SelectedAssetType = selectList.Where(x => x.Selected).Select(x => x.Text).FirstOrDefault();
			if (AccountId != null) {
				query = query.Where(x => x.AccountId == AccountId);
				ViewBag.AccountId = AccountId;
			}
			selectList = new List<SelectListItem>();
			selectList.Add(new SelectListItem() { Text = null, Value = null, Selected = AccountId == null });
			selectList.AddRange(AccountRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Id.ToString(), Value = x.Id.ToString(), Selected = AccountId != null && AccountId == x.Id }));
			ViewBag.Accounts = selectList;
			ViewBag.SelectedAccount = selectList.Where(x => x.Selected).Select(x => x.Text).FirstOrDefault();
			
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
				if (sortBy == "UsefulLifespan")
					query = OrderBy(query, x => x.UsefulLifespan, ascending);
				if (sortBy == "Code")
					query = OrderBy(query, x => x.Code, ascending);
				if (sortBy == "DigitalCode")
					query = OrderBy(query, x => x.DigitalCode, ascending);
				if (sortBy == "PurchaseDate")
					query = OrderBy(query, x => x.PurchaseDate, ascending);
				if (sortBy == "PurchasePrice")
					query = OrderBy(query, x => x.PurchasePrice, ascending);
				if (sortBy == "SalvageValue")
					query = OrderBy(query, x => x.SalvageValue, ascending);
				if (sortBy == "Image")
					query = OrderBy(query, x => x.Image, ascending);
				if (sortBy == "Longitude")
					query = OrderBy(query, x => x.Longitude, ascending);
				if (sortBy == "Latitude")
					query = OrderBy(query, x => x.Latitude, ascending);
				ViewBag.SortBy = sortBy;
				if (sortDesc != null && sortDesc.Value)
					ViewBag.SortDesc = sortDesc.Value;
			}

			ViewBag.Entities = query.ToList();
			return View();
		}

		//
		// GET: /Assets/Create

		public ActionResult Create()
		{
			List<SelectListItem> selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AssetTypeRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Name.ToString(), Value = x.Id.ToString(), Selected = null == x.Id }));
			ViewBag.AssetType = selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AccountRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Id.ToString(), Value = x.Id.ToString(), Selected = null == x.Id }));
			ViewBag.Account = selectList;
		    return View();
		} 
		
		//
		// POST: /Assets/Create
		
		[HttpPost]
		public ActionResult Create(Asset entity)
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
		// GET: /Assets/Details
		
		public ViewResult Details(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}


		//
		// GET: /Assets/Edit
				
		public ActionResult Edit(int Id)
		{
			var entity = repository.All().Single(x => x.Id == Id);
			List<SelectListItem> selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AssetTypeRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Name.ToString(), Value = x.Id.ToString(), Selected = entity.AssetTypeId == x.Id }));
			ViewBag.AssetType = selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AccountRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Id.ToString(), Value = x.Id.ToString(), Selected = entity.AccountId == x.Id }));
			ViewBag.Account = selectList;
			return View(entity);
		}
				
		//
		// POST: /Assets/Edit
				
		[HttpPost]
		public ActionResult Edit(Asset entity)
		{
			if (ModelState.IsValid)
				using (IUnitOfWork uow = uowFactory.Create()) {
					Asset original = repository.All().Single(x => x.Id == entity.Id);
					original.Id = entity.Id;
					original.Name = entity.Name;
					original.Description = entity.Description;
					original.UsefulLifespan = entity.UsefulLifespan;
					original.Code = entity.Code;
					original.DigitalCode = entity.DigitalCode;
					original.PurchaseDate = entity.PurchaseDate;
					original.PurchasePrice = entity.PurchasePrice;
					original.SalvageValue = entity.SalvageValue;
					original.Image = entity.Image;
					original.AssetTypeId = entity.AssetTypeId;
					original.AccountId = entity.AccountId;
					original.Longitude = entity.Longitude;
					original.Latitude = entity.Latitude;
					uow.Save();
					return RedirectToAction("Index");
				}
			else
				return View();
		}
		
		//
		// GET: /Assets/Delete
		
		public ActionResult Delete(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}
		
		//
		// POST: /Assets/Delete
		
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

