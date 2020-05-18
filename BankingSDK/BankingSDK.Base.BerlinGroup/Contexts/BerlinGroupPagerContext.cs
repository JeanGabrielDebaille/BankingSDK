using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Contexts
{
    public class BerlinGroupPagerContext : IPagerContext
    {
        [JsonProperty]
        private byte limit;
        [JsonProperty]
        private uint page;
        [JsonProperty]
        private uint nextPage;
        [JsonProperty]
        private uint totalPage;
        [JsonProperty]
        private uint? total;

        public BerlinGroupPagerContext() : this(10)
        {
        }

        public BerlinGroupPagerContext(byte limit)
        {
            this.limit = limit;
            page = 0;
        }

        public void FirstPage()
        {
            page = 0;
        }

        public bool IsFirstPage()
        {
            return page == 0;
        }

        public bool IsLastPage()
        {
            return page == totalPage;
        }

        public void NextPage()
        {
            nextPage = page + 1;
        }

        public void PreviousPage()
        {
            if (page > 0)
            {
                nextPage = page - 1;
            }
            else
            {
                nextPage = 0;
            }
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

            page = 0;
            nextPage = 0;
        }

        public byte GetLimit()
        {
            return limit;
        }

        public void SetPageTotal(uint pageTotal)
        {
            this.totalPage = pageTotal;
        }

        internal void SetTotal(uint total)
        {
            this.total = total;
        }

        public void SetPage(uint page)
        {
            this.page = page;
        }

        internal uint GetPage()
        {
            return page;
        }

        public uint GetNextPage()
        {
            return nextPage;
        }

        public string GetRequestParams()
        {
            return $"?dateFrom=1980-01-01&bookingStatus=both&size={limit}&page={nextPage}";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void GoToPage(uint page)
        {
            nextPage = (page - 1);
        }

        public uint? RecordCount()
        {
            return total ?? totalPage * limit + 1;
        }

        public uint? PageCount()
        {
            return totalPage + 1;
        }
    }
}
