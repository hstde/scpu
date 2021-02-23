namespace Sasm
{
    using System;
    using System.Collections.Generic;

    public struct ObjectCode
    {
        // this are labels that are specifically visible to other object files
        public List<AddressData> exportedLabels;
        // this are the labels that are defined in the code
        public List<AddressData> localLabels;
        // this are the labels that are referenced at a specific address
        public List<AddressData> referencedLabels;
        public byte[] code;
    }
}