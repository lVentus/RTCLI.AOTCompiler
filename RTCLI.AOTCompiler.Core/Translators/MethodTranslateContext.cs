using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using Mono.Cecil.Cil;
using RTCLI.AOTCompiler.Metadata;

namespace RTCLI.AOTCompiler.Translators
{
    public class MethodTranslateContext
    {
        public MethodTranslateContext(TranslateContext translateContext, MethodInformation methodInfo)
        {
            this.TranslateContext = translateContext;
            this.MetadataContext = translateContext.MetadataContext;
            this.MethodInfo = methodInfo;
        }
        public MethodInformation MethodInfo { get; }
        public TranslateContext TranslateContext { get; }
        public MetadataContext MetadataContext { get; }
    }
}