using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.LU.SocieteGenerale.Contexts
{
    public class SocieteGeneralePagerContext : IPagerContext
    {
        [JsonProperty]
        private byte limit;
        [JsonProperty]
        private uint offset;
        [JsonProperty]
        private uint nextOffset;
        [JsonProperty]
        private uint total;

        public SocieteGeneralePagerContext() : this(10)
        {
        }

        public SocieteGeneralePagerContext(byte limit)
        {
            this.limit = limit;
            offset = 0;
        }

        public void FirstPage()
        {
            offset = 0;
        }

        public bool IsFirstPage()
        {
            return offset == 0;
        }

        public bool IsLastPage()
        {
            return offset + limit >= total;
        }

        public void NextPage()
        {
            nextOffset = offset + limit;
        }

        public void PreviousPage()
        {
            if (offset > limit)
            {
                nextOffset = offset - limit;
            }
            else
            {
                nextOffset = 0;
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

            offset = 0;
            nextOffset = 0;
        }

        public byte GetLimit()
        {
            return limit;
        }

        internal void SetOffset(uint offset)
        {
            this.offset = offset;
        }

        internal uint GetOffset()
        {
            return offset;
        }

        internal void SetTotal(uint total)
        {
            this.total = total;
        }

        internal uint GetNextOffset()
        {
            return nextOffset;
        }

        internal string GetRequestParams()
        {
            return $"?limit={limit}&offset={nextOffset}";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void GoToPage(uint page)
        {
            nextOffset = total * (page - 1);
        }

        public uint? RecordCount()
        {
            return total;
        }

        public uint? PageCount()
        {
            return (uint)Math.Ceiling((decimal)total / (decimal)limit);
        }
    }
}
