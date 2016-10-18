# HappyMapper
[![NuGet](http://img.shields.io/nuget/v/HappyMapper.svg)](https://www.nuget.org/packages/HappyMapper/)
[![Build status](https://ci.appveyor.com/api/projects/status/k3g84dwuav95hr08?svg=true)](https://ci.appveyor.com/project/chumakov-ilya/happymapper)

HappyMapper is a simple .NET library to map objects.

In 2016, a lot of awesome mapping tools exist so who in his right mind will waste time to create another one? All because of curiousity. Is it possible to achieve both [EmitMapper](https://emitmapper.codeplex.com/) high speed and [AutoMapper](https://github.com/AutoMapper/AutoMapper) flexibility? The only goal of this project is to say "Yes". Look at the approximate time to map 10 <sup>8</sup> simple POCO objects:

Mapper | Case | Time, s
------ | ----- | -------------
Handwritten mapper |  | 2,6
HappyMapper |  Map| 3,6
EmitMapper |  Map | 4,2
HappyMapper |  Search + Map  | 19,5
EmitMapper |  Search + Map | 69,9

 It seems promising for now!    

**What is the difference between Map and Search+Map cases?**

Many mappers are much alike. Mapping delegates are pre-compiled (if possible) and placed to a key-value storage. When the `mapper.Map` method is called, the mapper finds the delegate in this storage and call it. So the search time is added to actual map time (**Search+Map** case). To avoid that search and speed up the mapping, some libraries can produce a _typed mapper_ object already containing the proper delegate (**Map** case). For example, [EmitMapper](https://emitmapper.codeplex.com/wikipage?title=Getting%20started&referringTitle=Home) can do it, and AutoMapper not.  Also mappers are based on different code generation technologies (here is a [litte overview](https://github.com/chumakov-ilya/Demo.DynamicCodeGen) of it), and they may produce more or less effective code. It leads to conclusion: delegate perfomance and overall perfomance should be considered separately.

**Why do I see a lot of AutoMapper source code in HappyMapper repository?**

First of all, HappyMapper uses its own Roslyn-based code generation engine. AutoMapper code is partly used only to create property-to-property mapping rules from user input. There are several reasons to reuse existing code:

  - I love AutoMapper and its awesome API providing a lot of settings to customize your mappings. 
  - Actually, it's a lot of work - create a good API. AutoMapper does it well. I don't want to reinvent a wheel and waste days on it.
  - AutoMapper API is well-known and there is no reason to create [another one](https://xkcd.com/927/).

Unfortunately, AutoMapper provides no extension points to replace code generation engine. This is the only reason why I forked the code with all respect to its authors. There is [the original license file](https://github.com/chumakov-ilya/HappyMapper/blob/master/AutoMapper.ConfigurationAPI/LICENSE.txt) inside AutoMapper project of course (I hope its enough to resolve juridical issues). Also HappyMapper is distributed under the same MIT license as AutoMapper. 


**ОК, how do I use HappyMapper?**

Simple!

    var config = new HappyConfig(cfg =>
    {
        cfg.CreateMap<Src, Dest>();
    });
    var mapper = config.CompileMapper();
    
    Dest dest = mapper.Map<Dest>(new Src()); //Search + Map case (untyped mapper)
    
    var singleMapper = mapper.GetSingleMapper<Src, Dest>();
    singleMapper.Map(new Src(), new Dest()); //Map case (typed mapper)

**Which is supported for now?**

- Classes 
- Generic collections (implementation of `ICollection<T>` is required).
- ForMemper option (similar to AutoMapper)
- Before/After map actions (similar to AutoMapper)
- Condition map predicate (similar to AutoMapper)


**Which is NOT supported?**

- Structs, enums, interfaces on both sides.
- Proxy creation
- Inheritance maps
- ALL rest of AutoMapper customization API.

So there is a lot of work to do!
