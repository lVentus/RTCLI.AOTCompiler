using RTCLI.AOTCompiler;
using RTCLI.AOTCompiler.ILConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using Mono.Cecil.Cil;

namespace RTCLI.AOTCompiler.Translators
{
    public struct CXXTranslateOptions
    {
        public DebugInformationOptions DebugInformationOption;
        public bool StaticAssertOnUnimplementatedILs;
        public IEnumerable<string> assemblyPaths;
    }

    public class CXXTranslator
    {
        private static bool TypeNameFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.ToString() == criteriaObj.ToString())
                return true;
            else
                return false;
        }

        public CXXTranslator(TranslateContext translateContext, CXXTranslateOptions options)
        {
            this.options = options;
            this.translateContext = translateContext;

            Assembly assembly = Assembly.GetAssembly(typeof(ILConverters.IILConverter));
            TypeFilter typeNameFilter = new TypeFilter(TypeNameFilter);
            foreach (var type in assembly.GetTypes())
            {
                Type[] typeInterfaces = type.FindInterfaces(typeNameFilter, typeof(ILConverters.ICXXILConverter));
                if (typeInterfaces.Length > 0)
                {
                    var newConv = System.Activator.CreateInstance(type) as ILConverters.ICXXILConverter;
                    convertersCXX.Add(newConv.TargetOpCode(), newConv);
                }
            }
        }

        private string NoteILInstruction(Instruction inst, MethodTranslateContext methodContext)
        {
            if (convertersCXX.TryGetValue(inst.OpCode, out ICXXILConverter targetConverter))
                return targetConverter.Note(inst, methodContext);
            return $"//{inst.ToString()}";
        }
        private string TranslateILInstruction(Instruction inst, MethodTranslateContext methodContext)
        {
            if (convertersCXX.TryGetValue(inst.OpCode, out ICXXILConverter targetConverter))
                return targetConverter.Convert(inst, methodContext);
            return options.StaticAssertOnUnimplementatedILs 
                ? $"static_assert(0, \"[{inst.ToString()}] Has No Converter Implementation!\");"
                : $"RTCLI::unimplemented_il();//{inst.ToString()}";
        }

        public void WriteSource(CodeTextStorage storage)
        {
            using (var _ = storage.EnterScope(translateContext.FocusedAssemblyInformation.IdentName))
            {

                foreach (var module in translateContext.FocusedAssemblyInformation.Modules.Values)
                {
                    foreach (var type in module.Types.Values)
                    {
                        var codeWriter = storage.Wirter(type.TypeName + ".cpp");
                        typeSourceWriters[type.FullName] = codeWriter;
                        foreach (var method in type.Methods)
                        {
                            CXXMethodTranslateContext methodContext = new CXXMethodTranslateContext(translateContext);
                            codeWriter.WriteLine(method.CXXMethodName);
                            codeWriter.WriteLine("{");
                            // Method Body
                            codeWriter.indent();
                            foreach (var instruction in method.Body.Instructions)
                            {
                                codeWriter.WriteLine(NoteILInstruction(instruction, methodContext));
                                codeWriter.WriteLine(TranslateILInstruction(instruction, methodContext));
                            }
                            codeWriter.unindent();
                            codeWriter.WriteLine("}");
                        }
                        typeSourceWriters[type.FullName].Flush();
                    }
                }

            }//End Dispose EnterScope

        }

        public readonly CXXTranslateOptions options;
        private readonly TranslateContext translateContext = null;
        private readonly Dictionary<string, CodeTextWriter> typeSourceWriters = new Dictionary<string, CodeTextWriter>();
        private readonly Dictionary<OpCode, ICXXILConverter> convertersCXX = new Dictionary<OpCode, ICXXILConverter>(); 
    }
}