﻿using FishNet.CodeGenerating.Helping.Extension;
using FishNet.CodeGenerating.Processing;
using FishNet.Configuring;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Helping;
using FishNet.Object.Prediction.Delegating;
using MonoFN.Cecil;
using MonoFN.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FishNet.CodeGenerating.Helping
{
    internal class NetworkBehaviourHelper
    {
        #region Reflection references.
        //Names.
        internal string FullName;
        //Prediction.
        internal MethodReference ClearReplicateCache_MethodRef;
        internal MethodReference SetLastReconcileTick_MethodRef;
        internal MethodReference TransformMayChange_MethodRef;
        internal MethodReference SendReplicateRpc_MethodRef;
        internal MethodReference SendReconcileRpc_MethodRef;
        internal MethodReference RegisterReplicateRpc_MethodRef;
        internal MethodReference RegisterReconcileRpc_MethodRef;
        internal MethodReference ReplicateRpcDelegateConstructor_MethodRef;
        internal MethodReference ReconcileRpcDelegateConstructor_MethodRef;
        //RPCs.
        internal MethodReference SendServerRpc_MethodRef;
        internal MethodReference SendObserversRpc_MethodRef;
        internal MethodReference SendTargetRpc_MethodRef;
        internal MethodReference DirtySyncType_MethodRef;
        internal MethodReference RegisterServerRpc_MethodRef;
        internal MethodReference RegisterObserversRpc_MethodRef;
        internal MethodReference RegisterTargetRpc_MethodRef;
        internal MethodReference ServerRpcDelegateConstructor_MethodRef;
        internal MethodReference ClientRpcDelegateConstructor_MethodRef;
        //Is checks.
        internal MethodReference IsClient_MethodRef;
        internal MethodReference IsOwner_MethodRef;
        internal MethodReference IsServer_MethodRef;
        internal MethodReference IsHost_MethodRef;
        //Misc.
        internal TypeReference TypeRef;
        internal MethodReference CompareOwner_MethodRef;
        internal MethodReference LocalConnection_MethodRef;
        internal MethodReference Owner_MethodRef;
        internal MethodReference ReadSyncVar_MethodRef;
        internal MethodReference NetworkInitializeInternal_MethodRef;
        //TimeManager.
        internal MethodReference TimeManager_MethodRef;
        #endregion

        #region Const.
        internal const uint MAX_RPC_ALLOWANCE = ushort.MaxValue;
        internal const string AWAKE_METHOD_NAME = "Awake";
        internal const string DISABLE_LOGGING_TEXT = "This message may be disabled by setting the Logging field in your attribute to LoggingType.Off";
        #endregion

        internal bool ImportReferences()
        {
            Type networkBehaviourType = typeof(NetworkBehaviour);
            TypeRef = CodegenSession.ImportReference(networkBehaviourType);
            FullName = networkBehaviourType.FullName;
            CodegenSession.ImportReference(networkBehaviourType);
        
            //ServerRpcDelegate and ClientRpcDelegate constructors.
            ServerRpcDelegateConstructor_MethodRef = CodegenSession.ImportReference(typeof(ServerRpcDelegate).GetConstructors().First());
            ClientRpcDelegateConstructor_MethodRef = CodegenSession.ImportReference(typeof(ClientRpcDelegate).GetConstructors().First());
            //Prediction Rpc delegate constructors.
            ReplicateRpcDelegateConstructor_MethodRef = CodegenSession.ImportReference(typeof(ReplicateRpcDelegate).GetConstructors().First());
            ReconcileRpcDelegateConstructor_MethodRef = CodegenSession.ImportReference(typeof(ReconcileRpcDelegate).GetConstructors().First());

            foreach (MethodInfo mi in networkBehaviourType.GetMethods((BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                //CreateDelegates.
                if (mi.Name == nameof(NetworkBehaviour.RegisterServerRpc))
                    RegisterServerRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.RegisterObserversRpc))
                    RegisterObserversRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.RegisterTargetRpc))
                    RegisterTargetRpc_MethodRef = CodegenSession.ImportReference(mi);
                //SendPredictions.
                else if (mi.Name == nameof(NetworkBehaviour.SendReplicateRpc))
                    SendReplicateRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.SendReconcileRpc))
                    SendReconcileRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.RegisterReplicateRpc))
                    RegisterReplicateRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.RegisterReconcileRpc))
                    RegisterReconcileRpc_MethodRef = CodegenSession.ImportReference(mi);
                //SendRpcs.
                else if (mi.Name == nameof(NetworkBehaviour.SendServerRpc))
                    SendServerRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.SendObserversRpc))
                    SendObserversRpc_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.SendTargetRpc))
                    SendTargetRpc_MethodRef = CodegenSession.ImportReference(mi);
                //Prediction.
                else if (mi.Name == nameof(NetworkBehaviour.SetLastReconcileTick))
                    SetLastReconcileTick_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.ClearReplicateCache))
                    ClearReplicateCache_MethodRef = CodegenSession.ImportReference(mi);
                //Misc.
                else if (mi.Name == nameof(NetworkBehaviour.TransformMayChange))
                    TransformMayChange_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.CompareOwner))
                    CompareOwner_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.ReadSyncVar))
                    ReadSyncVar_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.DirtySyncType))
                    DirtySyncType_MethodRef = CodegenSession.ImportReference(mi);
                else if (mi.Name == nameof(NetworkBehaviour.NetworkInitializeIfDisabledInternal))
                    NetworkInitializeInternal_MethodRef = CodegenSession.ImportReference(mi);
            }

            foreach (PropertyInfo pi in networkBehaviourType.GetProperties((BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                //Server/Client states.
                if (pi.Name == nameof(NetworkBehaviour.IsClient))
                    IsClient_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                else if (pi.Name == nameof(NetworkBehaviour.IsServer))
                    IsServer_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                else if (pi.Name == nameof(NetworkBehaviour.IsHost))
                    IsHost_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                else if (pi.Name == nameof(NetworkBehaviour.IsOwner))
                    IsOwner_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                //Owner.
                else if (pi.Name == nameof(NetworkBehaviour.Owner))
                    Owner_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                else if (pi.Name == nameof(NetworkBehaviour.LocalConnection))
                    LocalConnection_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
                //Misc.
                else if (pi.Name == nameof(NetworkBehaviour.TimeManager))
                    TimeManager_MethodRef = CodegenSession.ImportReference(pi.GetMethod);
            }

            return true;
        }

        /// <summary>
        /// Returnsthe child most Awake by iterating up childMostTypeDef.
        /// </summary>
        /// <param name="childMostTypeDef"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        internal MethodDefinition GetAwakeMethodDefinition(TypeDefinition typeDef)
        {
            return typeDef.GetMethod(AWAKE_METHOD_NAME);
        }


        /// <summary>
        /// Creates a replicate delegate.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="originalMethodDef"></param>
        /// <param name="readerMethodDef"></param>
        /// <param name="rpcType"></param>
        internal void CreateReplicateDelegate(MethodDefinition originalMethodDef, MethodDefinition readerMethodDef, uint methodHash)
        {
            MethodDefinition methodDef = originalMethodDef.DeclaringType.GetMethod(NetworkBehaviourProcessor.NETWORKINITIALIZE_EARLY_INTERNAL_NAME);
            ILProcessor processor = methodDef.Body.GetILProcessor();

            List<Instruction> insts = new List<Instruction>();
            insts.Add(processor.Create(OpCodes.Ldarg_0));

            insts.Add(processor.Create(OpCodes.Ldc_I4, (int)methodHash));

            /* Create delegate and call NetworkBehaviour method. */
            insts.Add(processor.Create(OpCodes.Ldnull));
            insts.Add(processor.Create(OpCodes.Ldftn, readerMethodDef));

            /* Has to be done last. This allows the NetworkBehaviour to
             * initialize it's fields first. */
            processor.InsertLast(insts);
        }



        /// <summary>
        /// Creates a RPC delegate for rpcType.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="originalMethodDef"></param>
        /// <param name="readerMethodDef"></param>
        /// <param name="rpcType"></param>
        internal void CreateRpcDelegate(bool runLocally, TypeDefinition typeDef, MethodDefinition readerMethodDef, RpcType rpcType, uint methodHash, CustomAttribute rpcAttribute)
        {
            

            MethodDefinition methodDef = typeDef.GetMethod(NetworkBehaviourProcessor.NETWORKINITIALIZE_EARLY_INTERNAL_NAME);
            ILProcessor processor = methodDef.Body.GetILProcessor();

            List<Instruction> insts = new List<Instruction>();
            insts.Add(processor.Create(OpCodes.Ldarg_0));

            //uint methodHash = originalMethodDef.FullName.GetStableHash32();
            insts.Add(processor.Create(OpCodes.Ldc_I4, (int)methodHash));

            /* Create delegate and call NetworkBehaviour method. */
            insts.Add(processor.Create(OpCodes.Ldnull));
            insts.Add(processor.Create(OpCodes.Ldftn, readerMethodDef));
            //Server.
            if (rpcType == RpcType.Server)
            {
                insts.Add(processor.Create(OpCodes.Newobj, ServerRpcDelegateConstructor_MethodRef));
                insts.Add(processor.Create(OpCodes.Call, RegisterServerRpc_MethodRef));
            }
            //Observers.
            else if (rpcType == RpcType.Observers)
            {
                insts.Add(processor.Create(OpCodes.Newobj, ClientRpcDelegateConstructor_MethodRef));
                insts.Add(processor.Create(OpCodes.Call, RegisterObserversRpc_MethodRef));
            }
            //Target
            else if (rpcType == RpcType.Target)
            {
                insts.Add(processor.Create(OpCodes.Newobj, ClientRpcDelegateConstructor_MethodRef));
                insts.Add(processor.Create(OpCodes.Call, RegisterTargetRpc_MethodRef));
            }

            /* Has to be done last. This allows the NetworkBehaviour to
             * initialize it's fields first. */
            processor.InsertLast(insts);
        }

        /// <summary>
        /// Creates exit method condition if local client is not owner.
        /// </summary>
        /// <param name="retIfOwner">True if to ret when owner, false to ret when not owner.</param>
        /// <returns>Returns Ret instruction.</returns>
        internal Instruction CreateLocalClientIsOwnerCheck(MethodDefinition methodDef, LoggingType loggingType, bool canDisableLogging, bool retIfOwner, bool insertFirst)
        {
            List<Instruction> instructions = new List<Instruction>();
            /* This is placed after the if check.
             * Should the if check pass then code
             * jumps to this instruction. */
            ILProcessor processor = methodDef.Body.GetILProcessor();
            Instruction endIf = processor.Create(OpCodes.Nop);

            instructions.Add(processor.Create(OpCodes.Ldarg_0)); //argument: this
            //If !base.IsOwner endIf.
            instructions.Add(processor.Create(OpCodes.Call, IsOwner_MethodRef));
            if (retIfOwner)
                instructions.Add(processor.Create(OpCodes.Brfalse, endIf));
            else
                instructions.Add(processor.Create(OpCodes.Brtrue, endIf));
            //If logging is not disabled.
            if (loggingType != LoggingType.Off)
            {
                string disableLoggingText = (canDisableLogging) ? DISABLE_LOGGING_TEXT : string.Empty;
                string msg = (retIfOwner) ?
                    $"Cannot complete action because you are the owner of this object. {disableLoggingText}." :
                    $"Cannot complete action because you are not the owner of this object. {disableLoggingText}.";

                instructions.AddRange(
                    CodegenSession.GeneralHelper.CreateDebugWithCanLogInstructions(processor, msg, loggingType, false, true)
                    );
            }
            //Return block.
            Instruction retInst = processor.Create(OpCodes.Ret);
            instructions.Add(retInst);
            //After if statement, jumped to when successful check.
            instructions.Add(endIf);

            if (insertFirst)
            {
                processor.InsertFirst(instructions);
            }
            else
            {
                foreach (Instruction inst in instructions)
                    processor.Append(inst);
            }

            return retInst;
        }

        /// <summary>
        /// Creates exit method condition if remote client is not owner.
        /// </summary>
        /// <param name="processor"></param>
        internal Instruction CreateRemoteClientIsOwnerCheck(ILProcessor processor, ParameterDefinition connectionParameterDef)
        {
            /* This is placed after the if check.
             * Should the if check pass then code
             * jumps to this instruction. */
            Instruction endIf = processor.Create(OpCodes.Nop);

            processor.Emit(OpCodes.Ldarg_0); //argument: this
            //If !base.IsOwner endIf.
            processor.Emit(OpCodes.Ldarg, connectionParameterDef);
            processor.Emit(OpCodes.Call, CompareOwner_MethodRef);
            processor.Emit(OpCodes.Brtrue, endIf);
            //Return block.
            Instruction retInst = processor.Create(OpCodes.Ret);
            processor.Append(retInst);

            //After if statement, jumped to when successful check.
            processor.Append(endIf);

            return retInst;
        }

        /// <summary>
        /// Creates exit method condition if not client.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="retInstruction"></param>
        /// <param name="warn"></param>
        internal void CreateIsClientCheck(MethodDefinition methodDef, LoggingType loggingType, bool useStatic, bool insertFirst)
        {
            /* This is placed after the if check.
             * Should the if check pass then code
             * jumps to this instruction. */
            ILProcessor processor = methodDef.Body.GetILProcessor();
            Instruction endIf = processor.Create(OpCodes.Nop);

            List<Instruction> instructions = new List<Instruction>();
            //Checking against the NetworkObject.
            if (!useStatic)
            {
                instructions.Add(processor.Create(OpCodes.Ldarg_0)); //argument: this
                //If (!base.IsClient)
                instructions.Add(processor.Create(OpCodes.Call, IsClient_MethodRef));
            }
            //Checking instanceFinder.
            else
            {
                instructions.Add(processor.Create(OpCodes.Call, CodegenSession.ObjectHelper.InstanceFinder_IsClient_MethodRef));
            }
            instructions.Add(processor.Create(OpCodes.Brtrue, endIf));
            //If warning then also append warning text.
            if (loggingType != LoggingType.Off)
            {
                string msg = $"Cannot complete action because client is not active. This may also occur if the object is not yet initialized or if it does not contain a NetworkObject component. {DISABLE_LOGGING_TEXT}.";
                instructions.AddRange(
                    CodegenSession.GeneralHelper.CreateDebugWithCanLogInstructions(processor, msg, loggingType, useStatic, true)
                    );
            }
            //Add return.
            instructions.AddRange(CreateRetDefault(methodDef));
            //After if statement, jumped to when successful check.
            instructions.Add(endIf);

            if (insertFirst)
            {
                processor.InsertFirst(instructions);
            }
            else
            {
                foreach (Instruction inst in instructions)
                    processor.Append(inst);
            }
        }


        /// <summary>
        /// Creates exit method condition if not server.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="warn"></param>
        internal void CreateIsServerCheck(MethodDefinition methodDef, LoggingType loggingType, bool useStatic, bool insertFirst)
        {
            /* This is placed after the if check.
            * Should the if check pass then code
            * jumps to this instruction. */
            ILProcessor processor = methodDef.Body.GetILProcessor();
            Instruction endIf = processor.Create(OpCodes.Nop);

            List<Instruction> instructions = new List<Instruction>();
            if (!useStatic)
            {
                instructions.Add(processor.Create(OpCodes.Ldarg_0)); //argument: this
                //If (!base.IsServer)
                instructions.Add(processor.Create(OpCodes.Call, IsServer_MethodRef));
            }
            //Checking instanceFinder.
            else
            {
                instructions.Add(processor.Create(OpCodes.Call, CodegenSession.ObjectHelper.InstanceFinder_IsServer_MethodRef));
            }
            instructions.Add(processor.Create(OpCodes.Brtrue, endIf));
            //If warning then also append warning text.
            if (loggingType != LoggingType.Off)
            {
                string msg = $"Cannot complete action because server is not active. This may also occur if the object is not yet initialized or if it does not contain a NetworkObject component. {DISABLE_LOGGING_TEXT}";
                instructions.AddRange(
                    CodegenSession.GeneralHelper.CreateDebugWithCanLogInstructions(processor, msg, loggingType, useStatic, true)
                    );
            }
            //Add return.
            instructions.AddRange(CreateRetDefault(methodDef));
            //After if statement, jumped to when successful check.
            instructions.Add(endIf);

            if (insertFirst)
            {
                processor.InsertFirst(instructions);
            }
            else
            {
                foreach (Instruction inst in instructions)
                    processor.Append(inst);
            }
        }

        /// <summary>
        /// Creates a return using the ReturnType for methodDef.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="methodDef"></param>
        /// <returns></returns>
        public List<Instruction> CreateRetDefault(MethodDefinition methodDef, ModuleDefinition importReturnModule = null)
        {
            ILProcessor processor = methodDef.Body.GetILProcessor();
            List<Instruction> instructions = new List<Instruction>();
            //If requires a value return.
            if (methodDef.ReturnType != methodDef.Module.TypeSystem.Void)
            {
                //Import type first.
                methodDef.Module.ImportReference(methodDef.ReturnType);
                if (importReturnModule != null)
                    importReturnModule.ImportReference(methodDef.ReturnType);
                VariableDefinition vd = CodegenSession.GeneralHelper.CreateVariable(methodDef, methodDef.ReturnType);
                instructions.Add(processor.Create(OpCodes.Ldloca_S, vd));
                instructions.Add(processor.Create(OpCodes.Initobj, vd.VariableType));
                instructions.Add(processor.Create(OpCodes.Ldloc, vd));
            }
            instructions.Add(processor.Create(OpCodes.Ret));

            return instructions;
        }
    }
}