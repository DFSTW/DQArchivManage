namespace DQArchivManageClt
{
    using System;
    using Thyt.TiPLM.UIL.Common;

    public sealed class DelegatesOfAm
    {
        public PLMSimpleDelegate D_AfterPrintTabClose;
        public PLMSimpleDelegate D_AfterSentTabClose;
        public PLMSimpleDelegate D_AfterTsdCreate;
        public PLMSimpleDelegate D_AfterTsdTabClose;
        public static DelegatesOfAm Instance = new DelegatesOfAm();
    }
}

