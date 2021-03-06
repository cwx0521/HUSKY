﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Husky.Tests
{
	[TestClass()]
	public class StringCutTests
	{
		[TestMethod()]
		public void SplitWordsByCapitalTest() {
			Assert.AreEqual(StringCut.SplitWordsByCapital("HelloWorld"), "Hello World");
			Assert.AreEqual(StringCut.SplitWordsByCapital("Hello World"), "Hello World");
			Assert.AreEqual(StringCut.SplitWordsByCapital("ohHelloWorld"), "oh Hello World");
			Assert.AreEqual(StringCut.SplitWordsByCapital("ID"), "ID");
			Assert.AreEqual(StringCut.SplitWordsByCapital("IDE"), "IDE");
		}

		[TestMethod()]
		public void RemoveWordTest() {
			Assert.AreEqual("HelloWorld".RemoveWord("oWo"), "Hellrld");
		}

		[TestMethod()]
		public void RemoveSpacesTest() {
			Assert.AreEqual("\t He  l\tlo \t W\to \r\nrl\r\nd ".RemoveSpaces(), "HelloWorld");
		}

		[TestMethod()]
		public void RemoveHtmlTest() {
			Assert.AreEqual("<div>Hello World</div>".RemoveHtml(), "Hello World");
			Assert.AreEqual("<a>Hello</a> World".RemoveHtml(), "Hello World");
			Assert.AreEqual("Hello <label>World</label>".RemoveHtml(), "Hello World");
			Assert.AreEqual("Hello <span class='blue'>World</span>".RemoveHtml(), "Hello World");
		}

		[TestMethod()]
		public void RemoveRegexTest() {
			Assert.AreEqual("Hello World 123".RemoveRegex(@"\d+"), "Hello World ");
			Assert.AreEqual("Hello World 123".RemoveRegex(@"\s+\d+"), "Hello World");
			Assert.AreEqual("Hello World 12".RemoveRegex(@"[ol3]"), "He Wrd 12");

		}

		[TestMethod()]
		public void ExtractTest() {
			Assert.AreEqual("Hello World 123".Extract(@"\d+"), "123");
			Assert.AreEqual("Hello World 123".Extract(@"\s+\d+"), " 123");
			Assert.AreEqual("Hello World 12".Extract(@"[ol3]"), "l");
			Assert.AreEqual("Hello World 123".Extract<int>(@"\d+"), 123);
		}

		[TestMethod()]
		public void ExtractNumberTest() {
			Assert.AreEqual("Hello World".ExtractNumber(), 0);
			Assert.AreEqual("Hello Wo123rld".ExtractNumber(), 123);
			Assert.AreEqual("He123llo Wo456rld".ExtractNumber(), 123);
			Assert.AreEqual("He-123llo Wo456rld".ExtractNumber(), -123);
		}

		[TestMethod()]
		public void MidByTest() {
			Assert.AreEqual("HelloHelloBigWorld".MidBy("Hello", "World"), "HelloBig");
			Assert.AreEqual("HelloHelloBigWorld".MidBy("Hello", "World", useLastFoundAfterKeywordInsteadOfTheFirst: true), "Big");
		}

		[TestMethod()]
		public void LeftByTest() {
			Assert.AreEqual("HelloBigBigWorld".LeftBy("abc"), null);
			Assert.AreEqual("HelloBigBigWorld".LeftBy("Big"), "Hello");
			Assert.AreEqual("HelloBigBigWorld".LeftBy("Big", useLastFoundKeywordInsteadOfTheFirst: true), "HelloBig");
		}

		[TestMethod()]
		public void RightByTest() {
			Assert.AreEqual("HelloBigBigWorld".RightBy("abc"), null);
			Assert.AreEqual("HelloBigBigWorld".RightBy("Big"), "BigWorld");
			Assert.AreEqual("HelloBigBigWorld".RightBy("Big", useLastFoundKeywordInsteadOfTheFirst: true), "World");
		}

		[TestMethod()]
		public void LeftTest() {
			Assert.AreEqual("abcdefg".Left(10), "abcdefg");
			Assert.AreEqual("abcdefg".Left(5), "abcde");
			Assert.AreEqual("abcdefg".Left(5, true), "ab...");
		}

		[TestMethod()]
		public void LeftMonospacedTest() {
			Assert.AreEqual("你好abcdefg".LeftMonospaced(50), "你好abcdefg");
			Assert.AreEqual("你好abcdefg".LeftMonospaced(8), "你好ab...");
			Assert.AreEqual("你好abcdefg".LeftMonospaced(9), "你好abc...");
		}

		[TestMethod()]
		public void SplitTest() {
			var str = "1,22,345";
			var arr = str.Split<int>(',');
			Assert.AreEqual(arr.Length, 3);
			Assert.AreEqual(arr[0], 1);
			Assert.AreEqual(arr[1], 22);
			Assert.AreEqual(arr[2], 345);
		}
	}
}