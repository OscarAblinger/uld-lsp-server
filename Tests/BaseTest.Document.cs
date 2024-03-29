﻿using uld.server;
using uld.server.Parsing;
using uld.server.Parsing.Impl;
using Moq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using uld.definition.Symbols;
using uld.definition;

namespace Tests
{
    public abstract partial class BaseTest
    {
        protected IParser NoOpParser()
        {
            var parser = new Mock<IParser>();

            parser.Setup(p => p.Parse(new Uri("file://noopDoc"), It.IsAny<string[]>())).Returns(ParseResult().Object);

            return parser.Object;
        }

        protected CommentRules CommentRules(
                params (string Start, string End, string Replacement)[] docComment
            ) => new CommentRules(
                  new CommentRule[0],
                  docComment.Select(com
                        => new CommentRule(com.Start, com.End, com.Replacement))
                      .ToArray()
                );

        protected Mock<ILanguageDefinition> LanguageDefinition(
                IRule rule,
                string? languageId = null,
                string? languageFilePattern = null
            )
        {
            return LanguageDefinition(
                languageId,
                languageFilePattern,
                new string[] { rule.Name },
                new Dictionary<string, IRule>()
                {
                    { rule.Name, rule }
                });
        }

        protected Mock<ILanguageDefinition> LanguageDefinition(
                string? languageId = null,
                string? languageFilePattern = null,
                string[]? startRules = null,
                IDictionary<string, IRule>? rules = null,
                CommentRules? commentRules = null
            )
        {
            var langDef = new Mock<ILanguageDefinition>();

            if (languageId != null)
                langDef.SetupGet(ld => ld.LanguageId).Returns(languageId);

            if (languageFilePattern != null)
                langDef.SetupGet(ld => ld.LanguageFilePattern).Returns(languageFilePattern);

            if (startRules != null)
                langDef.SetupGet(ld => ld.StartRules).Returns(startRules);

            if (rules != null)
                langDef.SetupGet(ld => ld.Rules).Returns(rules);

            if (commentRules.HasValue)
                langDef.SetupGet(ld => ld.CommentRules).Returns(commentRules.Value);

            return langDef;
        }

        protected Mock<IDocumentStore> DocumentStore(
            IDictionary<string, Document>? documents = null,
            ILanguageDefinition? languageDefinition = null,
            IParser? defaultParser = null
            )
        {
            var documentStore = new Mock<IDocumentStore>();

            if (documents != null)
                documentStore.SetupGet(ds => ds.Documents).Returns(documents);

            if (languageDefinition != null)
                documentStore.SetupGet(ds => ds.LanguageDefinition).Returns(languageDefinition);

            if (defaultParser != null)
                documentStore.Setup(ds => ds.CreateDefaultParser()).Returns(defaultParser);

            return documentStore;
        }

        protected Mock<IDocumentStore> DocumentStore(
            string documentUri, string text,
            IEnumerable<ISymbol> onlyRuleSymbols,
            IParseResult parseResult
            )
        {
            var uri = new Uri(documentUri);
            var parser = Parser(uri, parseResult).Object;

            return DocumentStore(
                new Dictionary<string, Document>()
                {
                    { documentUri, Document.FromText(uri, text, parser) }
                },
                LanguageDefinition(Rule("S", onlyRuleSymbols.ToArray()).Object).Object,
                parser
                );
        }

        protected Mock<IDocumentStore> DocumentStore(
            string documentUri, string text,
            ILanguageDefinition? languageDefinition = null,
            IParser? defaultParser = null
            )
        {
            var docParser = defaultParser ?? new Mock<IParser>().Object;

            return DocumentStore(
                new Dictionary<string, Document>()
                {
                    { documentUri, Document.FromText(new Uri(documentUri), text, docParser) }
                },
                languageDefinition,
                defaultParser
            );
        }

        protected Mock<IParser> Parser(
                Uri? uri = null,
                IParseResult? parseResult = null
            )
        {
            var parser = new Mock<IParser>();

            if (uri == null)
                uri = new Uri("file://");

            if (parseResult != null)
                parser.Setup(p => p.Parse(uri, It.IsAny<string[]>())).Returns(parseResult);

            return parser;
        }

        protected Mock<IParseResult> ParseResult(
                Error[]? errors = null,
                CompletionItem[]? possibleContinuations = null
            )
        {
            var parseResult = new Mock<IParseResult>();

            if (possibleContinuations != null)
                parseResult.SetupGet(pr => pr.PossibleContinuations).Returns(possibleContinuations);

            if (errors != null)
                parseResult.SetupGet(pr => pr.Errors).Returns(errors);

            return parseResult;
        }

        protected CompletionParams CompletionParams(string uri, string text)
        {
            return new CompletionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = new Uri(uri) },
                Position = new Position(text.LongCount(c => c == '\n'), text.Split('\n')[^1].Length)
            };
        }

        protected ReferenceParams ReferenceParams(string uri, Position position)
        {
            return new ReferenceParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = new Uri(uri) },
                Position = position
            };
        }
    }
}
