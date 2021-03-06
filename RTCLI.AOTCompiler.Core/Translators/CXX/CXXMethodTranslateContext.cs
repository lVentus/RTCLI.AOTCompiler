﻿using RTCLI.AOTCompiler.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace RTCLI.AOTCompiler.Translators
{
    public class CXXMethodTranslateContext : MethodTranslateContext
    {
        public CXXMethodTranslateContext(TranslateContext translateContext, MethodInformation methodInformation)
            : base(translateContext, methodInformation)
        {

        }

        private int CmptStackObjectIndex = -1;
        public string CmptStackObjectName => CmptStackValidate() ? $"s{CmptStack.Peek()}" : "ERROR_CMPT_STACK_EMPTY";
        public string CmptStackPushObject
        {
            get
            {
                CmptStackObjectIndex++;
                CmptStack.Push(CmptStackObjectIndex);
                return $"s{CmptStack.Peek()}";
            }
        }
        public string CmptStackPopObject
        {
            get
            {
                if (CmptStackValidate())
                    return $"s{CmptStack.Pop()}";
                return "ERROR_CMPT_STACK_EMPTY";
            }
        }
        public string CmptStackPopAll
        {
            get
            {
                string result = "";
                while (CmptStackValidate())
                    result += $"s{CmptStack.Pop()}";
                return result;
            }
        }
        private bool CmptStackValidate()
        {
            return CmptStack.Count > 0;
        }

        public int ArgumentsCount => ArgumentsStack.Count;

        Stack<int> CmptStack = new Stack<int>();
        List<string> ArgumentsStack = new List<string>();
    }
}
