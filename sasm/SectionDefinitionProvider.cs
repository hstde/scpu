using System;
using System.Collections.Generic;

namespace Sasm
{
    public class SectionDefinitionProvider
    {
        public IReadOnlyList<SectionDefinition> GetSections()
        {
            return new SectionDefinition[]
            {
                new SectionDefinition
                {
                    name = "text",
                    baseAddress = 0
                }
            };
        }
    }
}