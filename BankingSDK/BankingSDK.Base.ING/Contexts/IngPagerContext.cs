using BankingSDK.Common;
using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankingSDK.Base.ING.Contexts
{
    public class IngPagerContext : IPagerContext
    {
        private enum PageDiraction
        {
            Same,
            Next,
            Previous
        }

        private PageDiraction pageDiraction = PageDiraction.Same;
        [JsonProperty]
        private string nextPageId;
        [JsonProperty]
        private Stack<string> previousPages = new Stack<string>();
        [JsonProperty]
        private byte limit;
        [JsonProperty]
        private string currentPageId = string.Empty;
        [JsonProperty]
        private uint? total;
        [JsonProperty]
        private uint page = 1;
        [JsonProperty]
        private uint nextPage = 1;

        public IngPagerContext() : this(10)
        {
        }

        public IngPagerContext(byte limit)
        {
            this.limit = limit;
        }

        public void FirstPage()
        {
            previousPages.Clear();
        }

        public byte GetLimit()
        {
            return limit;
        }

        public void GoToPage(uint page)
        {
            if (total != null && (page - 1) * limit < total)
            {
                nextPage = page;
            }
            else
            {
                throw new Exception();
            }
        }

        public bool IsFirstPage()
        {
            return total == null ? !previousPages.Any() : page == 1;
        }

        public bool IsLastPage()
        {
            return total == null ? string.IsNullOrEmpty(nextPageId) : page * limit > total;
        }

        public void NextPage()
        {
            if (total != null)
            {
                nextPage = page + 1;
                return;
            }

            if (string.IsNullOrEmpty(nextPageId))
            {
                throw new Exception();
            }
            pageDiraction = PageDiraction.Next;
        }

        public uint? PageCount()
        {
            if (total != null)
            {
                nextPage = page + 1;
                return (uint)Math.Ceiling((decimal)total / limit);
            }

            return null;
        }

        public void PreviousPage()
        {
            if (IsFirstPage())
            {
                throw new Exception();
            }

            if (total != null)
            {
                nextPage = page - 1;
                return;
            }

            pageDiraction = PageDiraction.Previous;
        }

        public uint? RecordCount()
        {
            return total;
        }

        public void SetLimit(byte limit)
        {
            if (limit < 1)
            {
                this.limit = 1;
            }
            else
            {
                if (limit > 99)
                {
                    this.limit = 99;
                }
                else
                {
                    this.limit = limit;
                }
            }

            nextPageId = null;
            previousPages.Clear();
            pageDiraction = PageDiraction.Same;
            currentPageId = string.Empty;
            total = null;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal void SetTotal(uint total)
        {
            this.total = total;
        }

        internal void SetPage(uint page)
        {
            this.page = page;
            this.nextPage = page;
        }

        internal uint GetNextPage()
        {
            return nextPage;
        }

        internal void SetNextPageId(string nextPageId)
        {
            this.nextPageId = nextPageId;
        }

        internal string GetRequestParams()
        {
            switch (pageDiraction)
            {
                case PageDiraction.Next:
                    previousPages.Push(currentPageId);
                    currentPageId = nextPageId;
                    break;
                case PageDiraction.Previous:
                    currentPageId = previousPages.Pop();
                    break;
            }

            return $"?{(SdkApiSettings.IsSandbox ? "dateFrom=2019-01-09&dateTo=2019-01-10&" : "")}limit={limit}{(string.IsNullOrEmpty(currentPageId) ? "" : "&next=")}{currentPageId}";
        }
    }
}
