using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bones.Akka.Monitoring.Weaver.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        const string MONITORED_RECEIVE_ACTOR = "MonitoredReceiveActor";
        const string BONES_AKKA_MONITORING = "Bones.Akka.Monitoring";
        const string RECEIVE = "Receive";
        const string MONITORED_RECEIVE = "MonitoredReceive";
        const string RECEIVE_ASYNC = "ReceiveAsync";
        const string MONITORED_RECEIVE_ASYNC = "MonitoredReceiveAsync";
        const string RECEIVE_ACTOR = "ReceiveActor";

        public override void Execute()
        {
            WriteMessage("Weaving started", MessageImportance.High);

            var module = ModuleDefinition;

            //Get the reference for the type and methods
            var monitoredReceiveType = FindTypeDefinition(MONITORED_RECEIVE_ACTOR);
            var monitoredConstructor = monitoredReceiveType.Methods.FirstOrDefault(m => m.IsConstructor);
            var monitoredReceiveMethod = monitoredReceiveType.Methods.FirstOrDefault(m => m.Name == MONITORED_RECEIVE);
            var monitoredReceiveAsyncMethod = monitoredReceiveType.Methods.FirstOrDefault(m => m.Name == MONITORED_RECEIVE_ASYNC);
            var overidedMethods = monitoredReceiveType.Methods.Where(m => m.IsVirtual && m.IsReuseSlot);

            //Get all types we want to modify
            var internalTypes = module.GetTypes().ToList();
            var receiveActors = internalTypes.Where(t => t.BaseType != null && t.BaseType.Name == RECEIVE_ACTOR).ToList();
            foreach (var actor in receiveActors)
            {
                WriteMessage("Processing : " + actor.Name, MessageImportance.High);
                //Importe the references to the type
                var typeReference = module.ImportReference(monitoredReceiveType);
                var constructorReference = module.ImportReference(monitoredConstructor);
                var receiveReference = module.ImportReference(monitoredReceiveMethod);
                var receiveAsyncReferend = module.ImportReference(monitoredReceiveAsyncMethod);

                //Replace the base type (herited type) for the new one
                actor.BaseType = typeReference;

                //Get the constructor and replace the call to the base constructor loading the needed parameters
                var constructor = actor.Methods.FirstOrDefault(m => m.IsConstructor);
                var processor = constructor.Body.GetILProcessor();
                var instructions = constructor.Body.Instructions.ToList();
                var baseCallInstruction = Instruction.Create(OpCodes.Call, constructorReference);
                var serviceProvider = (constructor as MethodDefinition).Parameters.FirstOrDefault(p => p.ParameterType.Name == "IServiceProvider");
                var spLoadingInstruction = Instruction.Create(OpCodes.Ldarg, serviceProvider);
                var oldBaseCall = constructor.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Call);
                processor.Replace(oldBaseCall, baseCallInstruction);
                processor.InsertBefore(baseCallInstruction, spLoadingInstruction);

                //Replace the calls to the Receive and ReceiveAsync methods for the monitored ones
                ReplaceAllCalls(actor, RECEIVE, receiveReference);
                ReplaceAllCalls(actor, RECEIVE_ASYNC, receiveAsyncReferend);

                foreach (var method in overidedMethods)
                {
                    var methodReference = module.ImportReference(method);
                    ReplaceAllCalls(actor, method.Name, methodReference);
                }
            }

            WriteMessage("Weaving terminated", MessageImportance.High);
        }

        private void ReplaceAllCalls(TypeDefinition type, string methodName, MethodReference methodReference)
        {
            foreach (var m in type.Methods)
            {
                if (!m.HasBody) continue;
                var processor = m.Body.GetILProcessor();
                var instructions = m.Body.Instructions.ToList();
                var methodCalls = instructions.Where(i => i.OpCode == OpCodes.Call && (i.Operand as MethodReference)?.Name == methodName).ToList();

                foreach (var mc in methodCalls)
                {

                    if (mc.Operand is GenericInstanceMethod genericInstanceMethod)
                    {
                        //Only fit type with one generic argument for now
                        var genericType = genericInstanceMethod.GenericArguments.SingleOrDefault();

                        if(genericType == null)
                            throw new Exception("Generic type not found for method " + (mc.Operand as MethodReference).Name);

                        var toCall = new GenericInstanceMethod(methodReference);
                        toCall.GenericArguments.Add(genericType);
                        toCall.Parameters.Clear();
                        foreach (var p in (mc.Operand as MethodReference).Parameters)
                        {
                            toCall.Parameters.Add(p);
                        }
                        mc.Operand = toCall;
                    }
                    else 
                    {
                        mc.Operand = methodReference;
                        return;
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return BONES_AKKA_MONITORING;
        }
    }
}
