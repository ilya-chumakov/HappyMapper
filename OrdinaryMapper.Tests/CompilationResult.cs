                                                    
using System;

namespace RoslynMappers
{
    public static class Mapper_A_B_441ad7a1a2ce441abb7bb5f36f570580
    {
        public static void Map
 (OrdinaryMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.A src,
  OrdinaryMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B dest)
        {
            if (OrdinaryMapper.ConditionStore.Condition_0b3e91967c48467ea50c5ad6f072180e(src, dest))

                dest.P1 = src.P1;


        }
    }
}

namespace OrdinaryMapper
{
    public static class BeforeMapActionStore
    {
    }
}

namespace OrdinaryMapper
{
    public static class ConditionStore
    {
        public static Func<OrdinaryMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.A, OrdinaryMapper.Tests.MemberConfigurationExpressionTests.Condition_Tests.B, bool> Condition_0b3e91967c48467ea50c5ad6f072180e;

    }
}