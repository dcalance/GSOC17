using System;
using CSCacheLib;

namespace CSCacheEntry
{
	class CSCacheEntry
	{
		static void Main(string[] args)
		{
            CSCache a = new CSCache(args);
            a.Cache();
        }
	}
}