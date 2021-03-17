﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace LambdaExpressionUtils.Tests
{
    [TestClass]
    public class StringHelperTests
    {
        [TestMethod]
        public void TemplateThreeSegmentsTest() {
            var template = "{0} - {1}";

            var expectedSegments = new List<Segment>() { 
                new Segment(true, "{0}"), 
                new Segment(false, " - "), 
                new Segment(true, "{1}") 
            };
            var actualSegments = template.GetSegments();

            Assert.AreEqual(expectedSegments.Count, actualSegments.Count);
            foreach (var pair in expectedSegments.Zip(actualSegments, (ExpectedSegment, ActualSegment) => new { ExpectedSegment, ActualSegment })) {
                Assert.AreEqual(pair.ExpectedSegment.IsMatch, pair.ActualSegment.IsMatch);
                Assert.AreEqual(pair.ExpectedSegment.Value, pair.ActualSegment.Value);
            }
        }

        [TestMethod]
        public void TemplateFourSegmentsTest() {
            var template = ">{0}{1}<";

            var expectedSegments = new List<Segment>() {
                new Segment(false, ">"),
                new Segment(true, "{0}"),
                new Segment(true, "{1}"),
                new Segment(false, "<"),
            };
            var actualSegments = template.GetSegments();

            Assert.AreEqual(expectedSegments.Count, actualSegments.Count);
            foreach (var pair in expectedSegments.Zip(actualSegments, (ExpectedSegment, ActualSegment) => new { ExpectedSegment, ActualSegment })) {
                Assert.AreEqual(pair.ExpectedSegment.IsMatch, pair.ActualSegment.IsMatch);
                Assert.AreEqual(pair.ExpectedSegment.Value, pair.ActualSegment.Value);
            }
        }

        [TestMethod]
        public void TemplateTwoSegmentsTest() {
            var template = "{0}{1}";

            var expectedSegments = new List<Segment>() {
                new Segment(true, "{0}"),
                new Segment(true, "{1}"),
            };
            var actualSegments = template.GetSegments();

            Assert.AreEqual(expectedSegments.Count, actualSegments.Count);
            foreach (var pair in expectedSegments.Zip(actualSegments, (ExpectedSegment, ActualSegment) => new { ExpectedSegment, ActualSegment })) {
                Assert.AreEqual(pair.ExpectedSegment.IsMatch, pair.ActualSegment.IsMatch);
                Assert.AreEqual(pair.ExpectedSegment.Value, pair.ActualSegment.Value);
            }
        }
    }
}
