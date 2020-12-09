using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Business.Interfaces;

namespace StartProjectGuide.Models.ViewModels
{
    public class PageViewModel<T> : IPageViewModel<T> where T : BasePageData
    {
        public PageViewModel(T currentPage)
        {
            CurrentPage = currentPage;
        }

        public T CurrentPage { get; private set; }

        public IContent Section { get; set; }
    }
    public static class PageViewModel
    {
        public static PageViewModel<T> Create<T>(T page) where T : BasePageData
        {
            return new PageViewModel<T>(page);
        }
    }
}