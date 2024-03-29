﻿using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace uld.server.Parsing.Impl
{
    internal class ParseResult : IParseResult
    {
        public ParseResult(bool finished, CompletionItem[] possibleContinuations, Error[] errors, Identifier[] identifiers, Range[] foldingRanges, Range[] comments)
        {
            Finished = finished;
            PossibleContinuations = possibleContinuations;
            Errors = errors;
            Identifiers = identifiers;
            FoldingRanges = foldingRanges;
            Comments = comments;
        }

        public bool Finished { get; }

        public CompletionItem[] PossibleContinuations { get; }

        public Error[] Errors { get; }

        public Identifier[] Identifiers { get; }

        public Range[] FoldingRanges { get; }

        public Range[] Comments { get; }
    }
}
