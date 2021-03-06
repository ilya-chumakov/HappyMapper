﻿using System;
using HappyMapper.Benchmarks.Types;
using HappyMapper.Text.Legacy;
using NUnit.Framework;

namespace HappyMapper.Tests.Text.Legacy
{
    public class LegacyTextBuilder_Tests
    {
        [Test]
        public void CreateText_SimpleTypes_ProducesText()
        {
            LegacyMapContext context = new LegacyMapContext(typeof(Src), typeof(Dest));
            string text = LegacyTextBuilder.CreateText(context);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);
        }

        [Test]
        public void CreateText_NestedTypes_ProducesText()
        {
            LegacyMapContext context = new LegacyMapContext(typeof(NestedSrc), typeof(NestedDest));
            string text = LegacyTextBuilder.CreateText(context);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);
        }
    }
}