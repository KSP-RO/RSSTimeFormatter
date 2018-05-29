using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSTimeFormatter
{
	public enum VersionCompareResult
	{
		INCOMPATIBLE_TOO_EARLY,
		COMPATIBLE,
		INCOMPATIBLE_TOO_LATE,
		INVALID
	}
}
