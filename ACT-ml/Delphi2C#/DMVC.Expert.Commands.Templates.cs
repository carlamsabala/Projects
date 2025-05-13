using System.Collections.Generic;
using DMVC.Expert.CodeGen.Executor;       
using DMVC.Expert.CodeGen.Commands;        

namespace DMVCExpertCommandsTemplates
{
    public static class CommandsTemplates
    {
        public static void FillProgramTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitProgramCommand(),
                new TUnitRunServerProcBody(),
                new TUnitMainBeginEndCommand()
            });
        }

        public static void FillControllerTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitControllerCommand(),
                new TUnitControllerControllerDeclarationCommand(),
                new TUnitFooterCommand()
            });
        }

        public static void FillWebModuleTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitWebModuleDeclarationCommand()
            });
        }

        public static void FillWebModuleDFMTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TWebModuleDFMCommand()
            });
        }

        public static void FillJSONRPCTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitJSONRPCDeclarationCommand()
            });
        }

        public static void FillTemplateProTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitTemplateProHelpersDeclarationCommand()
            });
        }

        public static void FillWebStencilsTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitWebStencilsHelpersDeclarationCommand()
            });
        }

        public static void FillMustacheTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitMustacheHelpersDeclarationCommand()
            });
        }

        public static void FillEntitiesTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitControllerEntityDeclarationCommand()
            });
        }

        public static void FillServicesTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<IGenCommand>
            {
                new TUnitServicesDeclarationCommand()
            });
        }
    }
}
