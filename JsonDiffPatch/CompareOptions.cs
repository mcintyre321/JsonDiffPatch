using System;

namespace JsonDiffPatch
{
    public class CompareOptions
    {
        public static readonly CompareOptions Default = new CompareOptions();

        public bool EnableIdentifierCheck { get; }
        public string IdentifierProperty { get; }

        public CompareOptions(bool enableIdentifierCheck = false, string identifierProperty = "id")
        {
            EnableIdentifierCheck = enableIdentifierCheck;
            IdentifierProperty = identifierProperty;
        }
    }
}
