using System;
using AutoMapper.Extended.Net4;
using NUnit.Framework;

namespace OrdinaryMapper.Tests
{
    public class NamingTools_Tests
    {
        [Test]
        public void Clean_WhenCalled_RemovesChars()
        {
            string origin = "søme string.";
            string actual = NamingTools.ToAlphanumericOnly(origin);
            Console.WriteLine(actual);
            Assert.AreEqual("sme string", actual);
        } 
    }
}