using System;
using System.Collections.Generic;
using System.Linq;
using HappyMapper;
namespace HappyMapper
{
    public static class Mapper_A_B_79f07b009bad4922b0f760fa85f69d14
    {
        public static HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B Map
 (HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.A src,
  HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B dest)
        {
            if (HappyMapper.ConditionStore.Condition_7daa41d1327c45379884c06780195625(src, dest))

                dest.P1 = src.P1;


            return dest;
        }

    }
}

namespace HappyMapper
{
    public static class ConditionStore
    {
        public static Func<HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.A, HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B, bool> Condition_7daa41d1327c45379884c06780195625;

    }
}

namespace HappyMapper
{
    public static class Mapper_A_B_21ef0429953949d5a5fe062ac91eb3a8
    {
        public static ICollection<HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B> MapCollection
 (ICollection<HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.A> srcList,
  ICollection<HappyMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B> destList)
        {
            for (int index_1b03 = 0; index_1b03 < srcList.Count; index_1b03++)
            {
                var src = srcList.ElementAt(index_1b03);
                var dest = destList.ElementAt(index_1b03);
                if (HappyMapper.ConditionStore.Condition_7daa41d1327c45379884c06780195625(src, dest))

                    dest.P1 = src.P1;


            }

            return destList;
        }

    }
}


