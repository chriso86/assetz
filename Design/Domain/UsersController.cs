using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Assetz;

namespace Assetz.Controllers
{
	public class UsersController : Controller
	{
		private IUnitOfWorkFactory uowFactory;
		private IRepository<User> repository;
		private IRepository<Account> AccountRepository;

		public UsersController()
		{
			AssetzModel context = new AssetzModel();
			this.uowFactory = new EntityFrameworkUnitOfWorkFactory(context);
			this.repository = new EntityFrameworkRepository<User>(context);
			this.AccountRepository = new EntityFrameworkRepository<Account>(context);
		}
		
    public UsersController(IUnitOfWorkFactory uowFactory, IRepository<User> repository , IRepository<Account> AccountRepository)
		{
			this.uowFactory = uowFactory;
			this.repository = repository;
			this.AccountRepository = AccountRepository;
		}

		//
		// GET: /Users

		public ViewResult Index(int? page, int? pageSize, string sortBy, bool? sortDesc , int? AccountId)
		{
			// Defaults
			if (!page.HasValue)
				page = 1;
			if (!pageSize.HasValue)
				pageSize = 10;

			IQueryable<User> query = repository.All();
			query = query.OrderBy(x => x.Id);
			// Filtering
			List<SelectListItem> selectList;
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
				if (sortBy == "Id")
					query = OrderBy(query, x => x.Id, ascending);
				if (sortBy == "FirstName")
					query = OrderBy(query, x => x.FirstName, ascending);
				if (sortBy == "LastName")
					query = OrderBy(query, x => x.LastName, ascending);
				if (sortBy == "Role")
					query = OrderBy(query, x => x.Role, ascending);
				if (sortBy == "Email")
					query = OrderBy(query, x => x.Email, ascending);
				if (sortBy == "PasswordSalt")
					query = OrderBy(query, x => x.PasswordSalt, ascending);
				ViewBag.SortBy = sortBy;
				if (sortDesc != null && sortDesc.Value)
					ViewBag.SortDesc = sortDesc.Value;
			}

			ViewBag.Entities = query.ToList();
			return View();
		}

		//
		// GET: /Users/Create

		public ActionResult Create()
		{
			List<SelectListItem> selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AccountRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Id.ToString(), Value = x.Id.ToString(), Selected = null == x.Id }));
			ViewBag.Account = selectList;
		    return View();
		} 
		
		//
		// POST: /Users/Create
		
		[HttpPost]
		public ActionResult Create(User entity)
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
		// GET: /Users/Details
		
		public ViewResult Details(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}


		//
		// GET: /Users/Edit
				
		public ActionResult Edit(int Id)
		{
			var entity = repository.All().Single(x => x.Id == Id);
			List<SelectListItem> selectList;
			selectList = new List<SelectListItem>();
			selectList.AddRange(AccountRepository.All().ToList().Select(x => new SelectListItem() { Text = x.Id.ToString(), Value = x.Id.ToString(), Selected = entity.AccountId == x.Id }));
			ViewBag.Account = selectList;
			return View(entity);
		}
				
		//
		// POST: /Users/Edit
				
		[HttpPost]
		public ActionResult Edit(User entity)
		{
			if (ModelState.IsValid)
				using (IUnitOfWork uow = uowFactory.Create()) {
					User original = repository.All().Single(x => x.Id == entity.Id);
					original.Id = entity.Id;
					original.FirstName = entity.FirstName;
					original.LastName = entity.LastName;
					original.Role = entity.Role;
					original.Email = entity.Email;
					original.PasswordSalt = entity.PasswordSalt;
					original.AccountId = entity.AccountId;
					uow.Save();
					return RedirectToAction("Index");
				}
			else
				return View();
		}
		
		//
		// GET: /Users/Delete
		
		public ActionResult Delete(int Id)
		{
			return View(repository.All().Single(x => x.Id == Id));
		}
		
		//
		// POST: /Users/Delete
		
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

