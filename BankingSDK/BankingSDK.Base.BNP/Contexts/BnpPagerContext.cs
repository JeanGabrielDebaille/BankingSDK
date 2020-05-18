using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BNP.Contexts
{
    public class BnpPagerContext : IPagerContext
    {
        [JsonProperty]
        private byte limit;
        [JsonProperty]
        private uint page;
        [JsonProperty]
        private uint pageTotal;
        [JsonProperty]
        private uint nextPage;
        [JsonProperty]
        private uint? total;

        public BnpPagerContext() : this(10)
        {
        }

        public BnpPagerContext(byte limit)
        {
            this.limit = limit;
            page = 1;
            nextPage = 1;
        }

        public void FirstPage()
        {
            page = 1;
        }

        public bool IsFirstPage()
        {
            return page == 1;
        }

        public bool IsLastPage()
        {
            return page >= pageTotal;
        }

        public void NextPage()
        {
            nextPage = page + 1;
        }

        public void PreviousPage()
        {
            if (page > 1)
            {
                nextPage = page - 1;
            }
            else
            {
                nextPage = 1;
            }
        }

        public void SetLimit(byte limit)
        {
            if (limit < 1)
            {
                this.limit = 1;
            }
            else if (limit > 100)
            {
                this.limit = 100;
            }
            else
            {
                this.limit = limit;
            }

            page = 1;
            nextPage = 1;
        }

        public byte GetLimit()
        {
            return limit;
        }

        internal uint GetPage()
        {
            return page;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void GoToPage(uint page)
        {
            nextPage = page;
        }

        internal void SetTotal(uint total)
        {
            this.total = total;
        }

        internal void SetPageTotal(uint pageTotal)
        {
            this.pageTotal = pageTotal;
        }

        public uint? RecordCount()
        {
            return total;
        }

        public uint? PageCount()
        {
            return pageTotal;
        }

        internal string GetRequestParams()
        {
            page = nextPage;
            return $"?page={page}&pageSize={limit}";
        }
    }
}
