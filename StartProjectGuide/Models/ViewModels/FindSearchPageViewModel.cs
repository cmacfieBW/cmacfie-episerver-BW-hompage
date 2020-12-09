using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Find.UnifiedSearch;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Models.ViewModels
{
    public class FindSearchPageViewModel : PageViewModel<FindSearchPage>
    {
        public FindSearchPageViewModel(FindSearchPage currentPage, string searchQuery)
            : base(currentPage)
        {
            SearchQuery = searchQuery;
        }
        public string SearchQuery { get; private set; }
        public UnifiedSearchResults Results { get; set; }

        public string GetPagingUrl(int pageNumber)
        {
            return UriUtil.AddQueryString(HttpContext.Current.Request.RawUrl, "p", pageNumber.ToString());
        }

        public int PagingPage
        {
            get
            {
                int pagingPage;
                if (!int.TryParse(HttpContext.Current.Request.QueryString["p"], out pagingPage))
                {
                    pagingPage = 1;
                }

                return pagingPage;
            }
        }
    }
}