﻿namespace autosupport_lsp_server.Symbols
{
    public interface IAction : ISymbol
    {
        string Command { get; }

        public const string IDENTIFIER = "identifier";
    }
}
