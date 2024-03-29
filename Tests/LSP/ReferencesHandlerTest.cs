﻿using uld.server;
using uld.server.LSP;
using uld.server.Parsing.Impl;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Xunit;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using uld.definition;
using uld.definition.Serialization;

namespace Tests.LSP
{
    public class ReferencesHandlerTest : BaseTest
    {
        public static readonly ILanguageDefinition VarAndPrintLanguageDefintion =
            uld.definition.LanguageDefinition.FromXLinq(XElement.Parse(Helpers.ReadFile("Files.VarAndPrint.def")), InterfaceDeserializer.Instance);
        
        [Theory]
        [InlineData(@"Program a
var b = 0;
b print;

b print;", 1)]
        [InlineData(@"Program a
var var = 0;
var print;

var print;", 3)]
        public async void FindAllReferences(string text, int identifierLength)
        {
            // given
            var uri = "file:///test.txt";
            var docStore = DocumentStore(uri, text, VarAndPrintLanguageDefintion, new Parser(VarAndPrintLanguageDefintion));

            var refHandler = new ReferencesHandler(docStore.Object);

            var searchPosition = new Position(2, 0);
            var otherPositions = new List<Range>() {
                new Range(searchPosition, new Position(searchPosition.Line, searchPosition.Character + identifierLength)),
                new Range(new Position(1, 4), new Position(1, 4 + identifierLength)),
                new Range(new Position(4, 0), new Position(4, 0 + identifierLength))
            };

            // when
            var result = await refHandler.Handle(ReferenceParams(uri, searchPosition), new CancellationToken());

            // then: expectedContinuations are always the first options
            Assert.NotNull(result);

            foreach (var item in result)
            {
                Assert.Equal(uri, item.Uri.ToString());

                var positionIdx = otherPositions.IndexOf(item.Range);

                Assert.NotEqual(-1, positionIdx);

                otherPositions.RemoveAt(positionIdx);
            }

            Assert.Empty(otherPositions);
        }

        [Fact]
        public async void FindReferencesInMultipleDocuments()
        {
            // given
            var text1 = @"Program prog1
variable print;";
            var text2 = @"Program prog2
var variable = 0;";
            var document1Uri = new Uri("file:///program1");
            var document2Uri = new Uri("file:///program2");
            var parser = new Parser(VarAndPrintLanguageDefintion);
            var docStore = DocumentStore(
                new Dictionary<string, Document>()
                {
                    { document1Uri.ToString(), Document.FromText(document1Uri, text1, parser) },
                    { document2Uri.ToString(), Document.FromText(document2Uri, text2, parser) }
                },
                VarAndPrintLanguageDefintion,
                parser);

            var searchPosition = new Position(1, 0);
            var expectedLocations = new List<Location>() {
                new Location() {
                    Uri = document1Uri,
                    Range = new Range(searchPosition, new Position(searchPosition.Line, searchPosition.Character + 8))
                },
                new Location()
                {
                    Uri = document2Uri,
                    Range = new Range(new Position(1, 4), new Position(1, 4 + 8))
                }
            };

            var handler = new ReferencesHandler(docStore.Object);

            // when
            var result = await handler.Handle(ReferenceParams(document1Uri.ToString(), searchPosition), new CancellationToken());

            // then
            var resultAsList = result.ToList();

            Assert.Equal(2, resultAsList.Count);

            foreach (var item in result)
            {
                var positionIdx = expectedLocations.IndexOf(item);

                Assert.NotEqual(-1, positionIdx);

                Assert.Equal(expectedLocations[positionIdx].Uri, item.Uri);
                Assert.Equal(expectedLocations[positionIdx].Range, item.Range);

                expectedLocations.RemoveAt(positionIdx);
            }

            Assert.Empty(expectedLocations);
        }
    }
}
